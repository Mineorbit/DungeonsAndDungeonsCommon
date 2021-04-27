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

        public Transform A;

        public Transform B;

        float t = 0;

        public int numberOfPoints = 20;

        public void Start()
        {
            lr.SetWidth(0.4f, 0.4f);
        }
        public void Update()
        {
            AnimationCurve curve = new AnimationCurve();
            t += Time.deltaTime;
            for(float f = 0;f<1;f+=1f/((float)numberOfPoints))
            {

                curve.AddKey(f, 0.3f+(0.125f/2)*Mathf.Sin(-8*t + 8 * f));
            }
            lr.widthCurve = curve;
        }
        public override void OnInit()
        {
            base.OnInit();
            this.enabled = true; 
            numberOfPoints = 16;
            lr.positionCount = numberOfPoints;
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
            lr.SetWidth(1, 1);
            lr.BakeMesh(mesh, true);
            lr.SetWidth(0.4f, 0.4f);
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
            A.gameObject.SetActive(false);
            B.gameObject.SetActive(false);
            mc.enabled = false;
        }
        public override void OnEndRound()
        {
            base.OnEndRound();
            lr.enabled = true;
            A.gameObject.SetActive(true);
            B.gameObject.SetActive(true);
            mc.enabled = true;
        }

        Vector3 start = new Vector3(0,0,0);

        public void SetSenderPosition(Vector3 s)
        {
            start = s;
            lr.SetPosition(0, s);
            A.position = s;
        }

        public void SetReceiverPosition(Vector3 r)
        {
            for(int i = 1; i < numberOfPoints; i++)
            {
                float t = ((float)i / (float)(numberOfPoints-1));
                Vector3 pos = (1 - t) * start + t * r;
                Debug.Log(pos);
                lr.SetPosition(i, pos);
            }
            B.position = r;
        }

        public void SetSender(InteractiveLevelObject s)
        {
            sender = s;
            SetSenderPosition(s.transform.position);
        }
        public void SetReceiver(InteractiveLevelObject r)
        {
            receiver = r;
            SetReceiverPosition(r.transform.position);
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
