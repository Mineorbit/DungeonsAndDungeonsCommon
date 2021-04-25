using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{


    public class Wire : LevelObject
    {
        public static LevelObjectData wirePreset;

        public InteractiveLevelObject sender;
        public InteractiveLevelObject receiver;

        public LineRenderer lr;
        public MeshCollider mc;
        public override void OnInit()
        {
            base.OnInit();
            lr.positionCount = 2;
        }


        public static Wire Create(InteractiveLevelObject start, InteractiveLevelObject end, Color color)
        {
            if (wirePreset == null)
            {
                wirePreset = Resources.Load<LevelObjectData>("LevelObjectData/Environment/Interactive/Wire");
            }

            GameObject wire = LevelManager.currentLevel.AddDynamic(wirePreset, new Vector3(0,0,0), new Quaternion(0, 0, 0, 0));
            Wire w = wire.GetComponent<Wire>();
            w.SetSender(start);
            w.SetReceiver(end);
            w.Render();

            return w;


        }

        public static Wire CreateDynamic(Vector3 start, Vector3 end, Color color)
        {
            if (wirePreset == null)
            {
                wirePreset = Resources.Load<LevelObjectData>("LevelObjectData/Environment/Interactive/Wire");
            }

            GameObject wire = LevelManager.currentLevel.AddDynamic(wirePreset, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
            Wire w = wire.GetComponent<Wire>();
            w.SetSenderPosition(start);
            w.SetReceiverPosition(end);
            //w.Render();

            return w;


        }


        public void Render()
        {
            Mesh mesh = new Mesh();
            lr.BakeMesh(mesh, true);
            mc.sharedMesh = mesh;
            Debug.Log("Rendered mesh ");
            foreach(var x in mesh.vertices)
            {
                Debug.Log(x);
            }
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            lr.enabled = false;
            mc.enabled = false;
        }
        public override void OnEndRound()
        {
            base.OnEndRound();
            lr.enabled = true;
            mc.enabled = true;
        }

        public void SetSenderPosition(Vector3 s)
        {
            lr.SetPosition(0, s);
        }
        public void SetReceiverPosition(Vector3 r)
        {
            lr.SetPosition(1, r);
        }

        public void SetSender(InteractiveLevelObject s)
        {
            sender = s;
            lr.SetPosition(0,s.transform.position);
        }
        public void SetReceiver(InteractiveLevelObject r)
        {
            receiver = r;
            lr.SetPosition(1,r.transform.position);
        }

        public void OnDestroy()
        {
            if (receiver != null)
                receiver.inBoundWires.Remove(this);
            if(sender != null)
                sender.RemoveReceiver(receiver,this);
        }
    }
}
