using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityController : MonoBehaviour
    {
        public Vector3 controllerPosition;
        private bool dirty;
        private Vector3 positionUpdate;
        
        
        public float currentSpeed;


        private Vector3 lastPosition;
        
        private readonly int k = 10;
        
        public List<float> lastSpeeds = new List<float>();

        public virtual void Start()
        {
            controllerPosition = transform.position;
            
            lastPosition = controllerPosition;
        }

        public virtual void Update()
        {
            if (dirty)
            {
                controllerPosition = positionUpdate;
                dirty = false;
            }
        }

        public void OnSpawn(Vector3 location)
        {
            controllerPosition = location;
        }

        public virtual void FixedUpdate()
        {
            ComputeCurrentSpeed();
        }

        private void ComputeCurrentSpeed()
        {
            lastSpeeds.Add((transform.position - lastPosition).magnitude / Time.deltaTime);
            if (lastSpeeds.Count > k) lastSpeeds.RemoveAt(0);
            float sum = 0;
            for (var i = 0; i < lastSpeeds.Count; i++) sum += lastSpeeds[i];
            currentSpeed = sum / k;
            lastPosition = transform.position;
        }
        
        
        public void UpdatePosition(Vector3 position)
        {
            dirty = true;
            positionUpdate = position;
        }
    }
}