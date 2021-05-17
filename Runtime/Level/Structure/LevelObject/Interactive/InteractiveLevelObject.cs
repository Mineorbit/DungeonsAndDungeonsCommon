using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class InteractiveLevelObject : NetworkLevelObject
    {
        public List<Wire> inBoundWires = new List<Wire>();
        public List<Wire> outBoundWires = new List<Wire>();

        private bool activated;


        public Dictionary<Vector3, InteractiveLevelObject>
            receivers = new Dictionary<Vector3, InteractiveLevelObject>();


        private readonly Queue<Vector3> receiversToAdd = new Queue<Vector3>();

        public virtual void OnDestroy()
        {
            foreach (var w in inBoundWires) LevelManager.currentLevel.RemoveDynamic(w);
            foreach (var w in outBoundWires) LevelManager.currentLevel.RemoveDynamic(w);
        }


        public virtual void Activate()
        {
            Debug.Log(this + " activated");
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
                var k = new Vector3(0, 0, 0);
                foreach (var receiver in receivers)
                    if (receiver.Value == o)
                        k = receiver.Key;
                receivers.Remove(k);
            }

            outBoundWires.Remove(wireToO);
        }

        public void AddReceiver(Vector3 location)
        {
            Debug.Log("Adding Receiver at " + location);
            if (!(receivers.ContainsKey(location) || receiversToAdd.Contains(location)))
                receiversToAdd.Enqueue(location);
            FindReceivers();
        }

        public void AddReceiverDynamic(Vector3 location, InteractiveLevelObject r)
        {
            Debug.Log("HELLO");
            Debug.Log("Added receiver at location really" + location);
            if (r != null)
                if (!receivers.ContainsKey(location))
                {
                    receivers.Add(location, r);
                    var line = Wire.Create(this, r, Color.black);
                    outBoundWires.Add(line);
                    r.inBoundWires.Add(line);
                }
        }

        // Will try to find receiver at that location, if not found, will drop that receiver
        private void FindReceivers()
        {
            while (receiversToAdd.Count > 0)
            {
                var targetLocation = receiversToAdd.Dequeue();
                var receiver = LevelManager.currentLevel.GetLevelObjectAt(targetLocation);
                if (receiver != null)
                    Debug.Log(gameObject.name + " found " + receiver.gameObject.name + "");
                if (receiver != null && receiver.GetType().IsSubclassOf(typeof(InteractiveLevelObject)) &&
                    receiver != this)
                {
                    var r = (InteractiveLevelObject) receiver;
                    Debug.Log("Found " + r);
                    AddReceiverDynamic(r.transform.position, r);
                }
            }
        }


        public override void OnInit()
        {
            base.OnInit();


            FindReceivers();
        }
    }
}