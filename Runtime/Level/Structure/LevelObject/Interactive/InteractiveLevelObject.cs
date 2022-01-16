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

        
        
        Queue<int> receiversToAdd = new Queue<int>();
        
        public virtual void OnDestroy()
        {
            foreach (var w in inBoundWires) LevelManager.currentLevel.RemoveDynamic(w);
            foreach (var w in outBoundWires) LevelManager.currentLevel.RemoveDynamic(w);
        }



        public int activeIn = 0;
        public virtual void Activate()
        {
            GameConsole.Log( $"{this} activated");
            if (inBoundWires.Count > 0)
                activeIn = Math.Min(inBoundWires.Count, activeIn + 1);
            else
                activeIn = activeIn + 1;
            if (!activated && activeIn > 0)
            {
                activated = true;
                foreach (var receiver in receivers.Values) receiver.Invoke(receiver.Activate);
            }
        }

        public virtual void Deactivate()
        {
            activeIn = Math.Max(0,activeIn-1);
            if (activated && activeIn == 0)
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

        public void AddToQueue(int identity)
        {
            if (!(receivers.ContainsKey(identity)))
            {
                receiversToAdd.Enqueue(identity);
                GameConsole.Log($"Added {identity} to Receiver Adding Queue");
            }
            else
            {
                GameConsole.Log($"{identity} is allready a receiver");
            }
        }
        
        public void AddReceiver(int identity)
        {
            GameConsole.Log("Adding Receiver " + identity);
            AddToQueue(identity);
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
            GameConsole.Log($"Resetting State of {this}");
            activeIn = 0;
        }


        public void HandleReset()
        {
            (this as dynamic).ResetState();
        }

        public override void OnEndRound()
        {
            GameConsole.Log("Resetting "+this);
            base.OnEndRound();
            activeIn = 0;
            Invoke(HandleReset, doLocal:true, doServer: true);
        }
        
        // Will try to find receiver at that location, if not found, will drop that receiver
        private void FindReceivers()
        {
            int N = receiversToAdd.Count;
            for (int i = 0; i<N;i++)
            {
            	int receiverIdentity = receiversToAdd.Dequeue();
            	
                NetworkLevelObject receiver = NetworkLevelObject.FindByIdentity(receiverIdentity);
                	
                	if(receiver != null)
                	{
                        GameConsole.Log($"Found receiver {receiver} for {receiverIdentity}");
                        var r = (InteractiveLevelObject) receiver;
                        AddReceiverDynamic(receiverIdentity, r);
                    }else
                    {
                        AddToQueue(receiverIdentity);
                    }
            }
        }

        
        public override void OnInit()
        {
            base.OnInit();
            ChunkManager.onChunkLoaded.AddListener(FindReceivers);
            activeIn = 0;
            FindReceivers();
        }
    }
}
