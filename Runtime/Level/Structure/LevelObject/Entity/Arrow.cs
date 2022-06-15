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
        private int bounces = 32;
        void Start()
        {
            shootingSpeed = 28f;
            transform.parent = shootingBow.transform;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.identity;
            rigidbody.centerOfMass = Vector3.forward * 0.45f;
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
        
        public float shootingSpeed = 24f;
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
            rigidbody.AddForce(transform.forward*shootingSpeed);
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

        public void OnCollisionEnter(Collision info)
        {
            if(flying)
            {
                // THIS CODE WAS IN PART COPIED FROM https://answers.unity.com/questions/279634/collisions-getting-the-normal-of-the-collision-sur.html
                var point = info.contacts[0].point;
                var dir = -info.contacts[0].normal;
                point -= dir;
                RaycastHit hitInfo;
                if(info.collider.Raycast( new Ray( point, dir ), out hitInfo, 2 ) )
                {
                    // this is the collider surface normal
                    var n = hitInfo.normal;
                    Vector3 u = rigidbody.velocity;
                    transform.position += n*0.125f;
                    Vector3 velocity = Vector3.Reflect(u, n);
                    GameConsole.Log($"Velocity changed from: {u} to: {velocity}");
                    rigidbody.velocity = velocity;
                    bounces--;
                    if (bounces == 0)
                    { 
                        Drop();
                    }
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