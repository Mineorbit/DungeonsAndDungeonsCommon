using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{


    public class InteractiveLevelObject : LevelObject
    {
        public Dictionary<Vector3,InteractiveLevelObject> receivers = new Dictionary<Vector3, InteractiveLevelObject>();

        public List<InteractiveLevelObject> receiversList = new List<InteractiveLevelObject>();

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
            Debug.Log(gameObject.name+" adding receiver at location "+location);
            if(! (receivers.ContainsKey(location) || receiversToAdd.Contains(location)))
            {
                receiversToAdd.Enqueue(location);
            }
        }

        public void AddReceiver(Vector3 location, InteractiveLevelObject r)
        {
            if(r != null)
            { 
                receivers.Add(location,r);
                receiversList.Add(r);
            DrawLine(transform.position, location, Color.black);
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
                if(receiver != null && receiver.GetType() == typeof(InteractiveLevelObject) && receiver != this)
                {
                    InteractiveLevelObject r = (InteractiveLevelObject) receiver;
                    AddReceiver(receiver.transform.position, r);
                }
            }
        }

        void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            GameObject myLine = new GameObject();
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();

            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.2f;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
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
