using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Aura : MonoBehaviour
    {
        // Start is called before the first frame update
        public Mesh mesh;
        public GameObject[] gearwheels;
        private float[] speeds = new float[4];

        void Start()
        {
            mesh = GetComponentInChildren<MeshFilter>().mesh;
            for (int i = 0; i < 4; i++)
            {
                speeds[i] = Random.Range(0, 1);
            }
        }

        private float speed = 0.125f;

        public float rotationHeight = 0.03f;

        public float rotationSpeed = 8;

        // Update is called once per frame
        private float t = 0;

        void Update()
        {
            t += Time.deltaTime;
            for(int i = 0;i<4;i++)
                gearwheels[i].transform.Rotate(Vector3.up, Time.deltaTime * speeds[i]);
            Vector2[] uvs = mesh.uv;
            Vector2[] newuvs = (Vector2[]) uvs.Clone();
            for (int i = 0; i < uvs.Length; i++)
            {
                Vector2 uv = uvs[i];
                uv.y += Time.deltaTime * speed;
                if (uv.y > 1) uv.y = 0;
                uv.x += Time.deltaTime * speed;
                if (uv.x > 1) uv.x = 0;
                newuvs[i] = uv;
            }

            mesh.uv = newuvs;
        }
    }
}