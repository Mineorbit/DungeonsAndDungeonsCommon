using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{


    public class InteractiveLevelObject : LevelObject
    {
        public Dictionary<Vector3,InteractiveLevelObject> receivers = new Dictionary<Vector3, InteractiveLevelObject>();

        Queue<Vector3> receiversToAdd = new Queue<Vector3>();

        bool activated = false;

        public virtual void Activate()
        {
            if(!activated)
            {
                foreach(var receiver in receivers.Values)
                {
                    receiver.Activate();
                }
            }
        }

        public void AddReceiver(Vector3 location)
        {
            if(! (receivers.ContainsKey(location) || receiversToAdd.Contains(location)))
            {
                receiversToAdd.Enqueue(location);
            }
        }

        void Start()
        {

        }

        // Will try to find receiver at that location, if not found, will drop that receiver
        void FindReceivers()
        {
            while(receiversToAdd.Count > 0)
            {
                Vector3 targetLocation = receiversToAdd.Dequeue();
                Debug.Log("Trying to add receiver " +targetLocation);
                LevelObject receiver = LevelManager.currentLevel.GetLevelObjectAt(targetLocation);
                if(receiver != null && receiver.GetType() == typeof(InteractiveLevelObject))
                {
                    InteractiveLevelObject r = (InteractiveLevelObject) receiver;
                    receivers.Add(targetLocation, r);
                }
            }
        }

        public override void OnInit()
        {
            base.OnInit();
            FindReceivers();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
