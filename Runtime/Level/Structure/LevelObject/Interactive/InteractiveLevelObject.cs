using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{


    public class InteractiveLevelObject : LevelObject
    {


        public Dictionary<Vector3,InteractiveLevelObject> receivers = new Dictionary<Vector3, InteractiveLevelObject>();


        public List<Wire> inBoundWires = new List<Wire>();
        public List<Wire> outBoundWires = new List<Wire>();



        Queue<Vector3> receiversToAdd = new Queue<Vector3>();

        bool activated = false;


        public virtual void Activate()
        {
            if(this.enabled)
            { 
            if(!activated)
            {
                activated = true;
                foreach(var receiver in receivers.Values)
                {
                    receiver.Activate();
                }
            }
            }
        }
        public virtual void Deactivate()
        {
            if(this.enabled)
            { 
            if (activated)
            {
                activated = false;
                foreach (var receiver in receivers.Values)
                {
                    receiver.Deactivate();
                }
            }
            }
        }



        public void RemoveReceiver(InteractiveLevelObject o, Wire wireToO)
        {
            if(receivers.ContainsValue(o))
            {
                Vector3 k = new Vector3(0,0,0);
                foreach(KeyValuePair < Vector3,InteractiveLevelObject > receiver in receivers)
                {
                    if(receiver.Value == o)
                    {
                        k = receiver.Key;
                    }
                }
                receivers.Remove(k);
            }
            outBoundWires.Remove(wireToO);
        }

        public void AddReceiver(Vector3 location)
        {
            Debug.Log("Adding Receiver at "+location);
            if(! (receivers.ContainsKey(location) || receiversToAdd.Contains(location)))
            {
                receiversToAdd.Enqueue(location);
            }
            FindReceivers();
        }

        public void AddReceiverDynamic(Vector3 location, InteractiveLevelObject r)
        {
            Debug.Log("HELLO");
            Debug.Log("Added receiver at location really" + location);
            if (r != null)
            { 
                if(!receivers.ContainsKey(location))
                {
                receivers.Add(location,r);
                Wire line = Wire.Create(this, r, Color.black);
                outBoundWires.Add(line);
                r.inBoundWires.Add(line);
                }
            }
        }

        public virtual void OnDestroy()
        {
            foreach(Wire w in  inBoundWires)
            {
                LevelManager.currentLevel.RemoveDynamic(w);
            }
            foreach(Wire w in outBoundWires)
            {
                LevelManager.currentLevel.RemoveDynamic(w);
            }
        }

        // Will try to find receiver at that location, if not found, will drop that receiver
        void FindReceivers()
        {
            while(receiversToAdd.Count > 0)
            {
                Vector3 targetLocation = receiversToAdd.Dequeue();
                LevelObject receiver = LevelManager.currentLevel.GetLevelObjectAt(targetLocation);
                if(receiver != null)
                Debug.Log(this.gameObject.name+" found "+receiver.gameObject.name+"");
                if (receiver != null && receiver.GetType().IsSubclassOf(typeof(InteractiveLevelObject)) && receiver != this)
                {
                    InteractiveLevelObject r = (InteractiveLevelObject) receiver;
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
