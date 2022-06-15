using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
            rigidbody.centerOfMass = Vector3.forward * 0.125f;
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

        private bool canBounce = true;

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(0.0125f);
            canBounce = true;
        }

        public float minSpeed = 5;        
        public float minBounceSpeed = 5;

        private void Bounce(Vector3 normal,Vector3 hitPoint)
        {
            if (rigidbody.velocity.magnitude < minBounceSpeed)
            {
                return;
            }

            if(canBounce)
            {
                canBounce = false;
                StartCoroutine(Wait());
                Vector3 u = rigidbody.velocity;
                Debug.DrawRay(transform.position,u,Color.green);
                Vector3 velocity = Vector3.Reflect(u, normal);
                //GameConsole.Log($"Velocity changed from: {u} to: {velocity}");
               // transform.position += velocity*0.025f;
                transform.rotation = Quaternion.LookRotation(velocity);
                transform.position = hitPoint;
                rigidbody.velocity = velocity/2;
                bounces--;
                if (bounces == 0 || velocity.magnitude < minSpeed )
                { 
                    Drop();
                } 
                
                Debug.DrawRay(transform.position,normal,Color.blue);
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red);
                Debug.Break();
            }
        }
        private void FixedUpdate()
        {
            if(flying)
            {
                GameConsole.Log(rigidbody.velocity.magnitude.ToString());
                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 0.4f))
                {
                    if (hit.collider.gameObject.CompareTag("Entity"))
                    {
                        return;
                    }
                    Vector3 normal = hit.normal;
                    /*
                    for (int i = -1; i < 1; i+=2)
                    {
                        for (int j = -1; i < 1; i+=2)
                        {
                            Vector3 offset = Vector3.zero;
                            offset += i * transform.up;
                            offset += j * transform.right;

                            if (Physics.Raycast(transform.position + offset, transform.TransformDirection(Vector3.forward), out hit, 0.5f))
                            {
                                normal += hit.normal;
                                hits++;
                            }
                        }
                    }
                    */
                    Bounce(hit.normal,hit.point);
                }
                
            }
        }

        /*
        void OnCollisionEnter(Collision collision)
        {
            Bounce(collision.contacts[0].normal);
        }
        */
        private void Update()
        {
            if (!shotArrow)
            {
                transform.rotation = GetAimDirection();
            }
        }


    }
}