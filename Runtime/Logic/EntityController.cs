using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityController : MonoBehaviour
    {
        private bool dirty;
        
        public CharacterController controller;
        
        public float currentSpeed;

        public Entity entity;

        private Vector3 lastPosition;
        
        private readonly int k = 10;
        
        public List<float> lastSpeeds = new List<float>();

        public virtual void Start()
        {
            entity = GetComponent<Entity>();
            lastPosition = transform.position;
        }

        public virtual void Update()
        {
            if (dirty)
            {
                dirty = false;
            }
        }

        public void OnSpawn(Vector3 location)
        {
        }

        public virtual void FixedUpdate()
        {
        }


    }
}