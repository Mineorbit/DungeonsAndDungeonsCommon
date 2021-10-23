using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class InteractiveLevelObject : NetworkLevelObject
    {
        public List<Wire> inBoundWires = new List<Wire>();
        public List<Wire> outBoundWires = new List<Wire>();

        private bool activated;


        public Dictionary<int, InteractiveLevelObject>
            receivers = new Dictionary<int, InteractiveLevelObject>();

        public virtual void OnDestroy()
        {
            foreach (var w in inBoundWires) LevelManager.currentLevel.RemoveDynamic(w);
            foreach (var w in outBoundWires) LevelManager.currentLevel.RemoveDynamic(w);
        }


        public virtual void Activate()
        {
            GameConsole.Log( $"{this} activated");
            if (!activated)
            {
                activated = true;
                foreach (var receiver in receivers.Values) receiver.Invoke(receiver.Activate);
            }
        }

        public virtual void Deactivate()
        {
            if (activated)
            {
                activated = false;
                foreach (var receiver in receivers.Values) receiver.Invoke(receiver.Deactivate);
            }
        }


        public void RemoveReceiver(InteractiveLevelObject o, Wire wireToO)
        {
            if (receivers.ContainsValue(o))
            {
                var k = 0;
                foreach (var receiver in receivers)
                    if (receiver.Value == o)
                        k = receiver.Key;
                receivers.Remove(k);
            }

            outBoundWires.Remove(wireToO);
        }
	
	Queue<int> receiversToAdd = new Queue<int>();
        public void AddReceiver(int identity)
        {
            GameConsole.Log("Adding Receiver " + identity);
            if (!(receivers.ContainsKey(identity)))
                receiversToAdd.Enqueue(identity);
            FindReceivers();
        }

        public void AddReceiverDynamic(int identity, InteractiveLevelObject receiver)
        {   
            if (receiver != null)
                if (!receivers.ContainsKey(identity))
                {
                    receivers.Add(identity, receiver);
                    var line = Wire.Create(this, receiver, Color.black);
                    outBoundWires.Add(line);
                    receiver.inBoundWires.Add(line);
                }
        }


        public virtual void ResetState()
        {
            
        }


        public void HandleReset()
        {
            (this as dynamic).ResetState();
        }

        public override void OnEndRound()
        {
            GameConsole.Log("Resetting "+this);
            base.OnEndRound();
            Invoke(HandleReset, doLocal:true, doServer: false);
        }
        
        // Will try to find receiver at that location, if not found, will drop that receiver
        private void FindReceivers()
        {
            int N = receiversToAdd.Count;
            for (int i = 0; i<N;i++)
            {
            	int receiverIdentity = receiversToAdd.Dequeue();
            	
            	GameConsole.Log("Trying to add Receiver for real");
                NetworkLevelObject receiver = NetworkLevelObject.FindByIdentity(receiverIdentity);
                	
                	if(receiver != null)
                	{
                        var r = (InteractiveLevelObject) receiver;
                        AddReceiverDynamic(receiverIdentity, r);
                    }else
                    {
                        receiversToAdd.Enqueue(receiverIdentity);
                    }
            }
        }

        
        public override void OnInit()
        {
            base.OnInit();
            ChunkManager.onChunkLoaded.AddListener(FindReceivers);
            FindReceivers();
        }
    }
}
