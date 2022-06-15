using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Arrow : Entity
    {
        public Bow shootingBow;
        // Start is called before the first frame update
        public Hitbox hitBox;
        void Start()
        {
            transform.parent = shootingBow.transform;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.identity;
        }
        private void TryDamage(GameObject g)
        {
            GameConsole.Log($"Trying to hit {g}");
            var c = g.GetComponentInParent<Entity>(true);
            if (c != null && c.GetComponentInChildren<Bow>() != shootingBow)
            {
                //onHitEvent.Invoke();
                c.Hit(this, 20);
                Drop();
            }
        }
        

        // THIS SHOULD BE FIXED
        public void OnColliderEnter(Collider other)
        {
            if (other.transform.GetComponentInParent<Bow>() == null ||
                other.transform.GetComponentInChildren<Bow>() != shootingBow)
            {
                flying = false;
                Invoke("Drop",15f);
            }
        }

        public bool shotArrow = false;
        private bool flying = false;
        
        public float shootingSpeed = 4f;
        private float maxFlyingTime = 10f;

        private Vector3 lastTarget;
        
        Quaternion GetAimDirection()
        {
            return shootingBow.owner.GetAimRotation();
        }


        
        public LevelObjectData test;
        
        public void Shoot(Quaternion aimDirection)
        {
            transform.parent = LevelManager.currentLevel.dynamicObjects;

            transform.rotation = aimDirection;
            flying = true;
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            transform.position += transform.forward*0.5f;
            rigidbody.AddForce(transform.forward*shootingSpeed,ForceMode.VelocityChange);
            shotArrow = true;
            hitBox.Attach("Entity");
            hitBox.enterEvent.AddListener(x => { TryDamage(x); });
            Invoke(ArrowShot);
            Invoke("Drop",maxFlyingTime);
        }

        public GameObject trails;
        public void ArrowShot()
        {
            trails.SetActive(true);
        }
        
        public void Drop()
        {
            LevelManager.currentLevel.RemoveDynamic(this, true);
        }

        private int bounces = 3;
        public void OnCollisionEnter(Collision other)
        {
            if(flying)
            {
                Vector3 n = other.GetContact(0).normal;
                n.Normalize();
                
            Vector3 u = rigidbody.velocity;
            transform.position += n*0.125f;
            Vector3 velocity = Vector3.Reflect(u, n);
            GameConsole.Log($"Velocity changed to: {velocity}");
            rigidbody.velocity = velocity;
            bounces--;
            if (bounces == 0)
            {
                Drop();
            }
            }
        }

        private void Update()
        {
            if (!shotArrow)
            {
                transform.rotation = GetAimDirection();
            }
        }


    }
}