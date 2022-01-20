using System.Collections;
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
        
        
        public readonly float gravity = 4f;
        
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

        public virtual void Deactivate()
        {
            enabled = false;
            if (controller != null)
            {
                controller.enabled = false;
            }
        }

        public virtual void Activate()
        {
            enabled = true;
            if (controller != null)
            {
                controller.enabled = true;
            }
        }

            /*
            public IEnumerator KickbackRoutine(Vector3 dir, float dist, float kickbackSpeed)
            {
                float t = 0;
                Vector3 start = transform.position;
                float kickbackTime =  dist / kickbackSpeed;
                float speedY = 0;
                while (t<kickbackTime)
                {
                    t += Time.deltaTime;
                    if (!controller.isGrounded)
                    {
                        speedY += Time.deltaTime * gravity;
                    }
                    controller.Move(0.05f*kickbackSpeed*dir+speedY*Vector3.down);
                    yield return new WaitForEndOfFrame();
                }
                entity.FinishKickback();
            }
            */
        
        public void OnSpawn(Vector3 location)
        {
        }

        public virtual void FixedUpdate()
        {
        }


    }
}