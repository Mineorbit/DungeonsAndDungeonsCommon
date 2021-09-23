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
            if (other.transform.GetComponentInParent<Bow>() != shootingBow ||
                other.transform.GetComponentInChildren<Bow>() != shootingBow)
            {
                flying = false;
                Invoke("Drop",15f);
            }
        }

        private bool flying = false;
        private float distance = 5;
        private float aimDistance = 20;
        
        private float speed = 0.2f;
        private float maxFlyingTime = 10f;

        Quaternion GetAimDirection()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            Vector3 start = ray.GetPoint((Camera.main.transform.position-shootingBow.transform.position).magnitude); 
            RaycastHit hit;
            int mask = LayerMask.NameToLayer("HitBox");
            GameConsole.Log("MASK: "+mask);
            int realmask = ~mask;
            GameConsole.Log("MASK: "+realmask);
            Vector3 target;
            
            Debug.DrawRay(start,ray.direction*5f,Color.red,200);
            if (Physics.Raycast(start, ray.direction, out hit, aimDistance, realmask))
            {
                 target = hit.point;
            }
            else
            {
                target = start+ray.direction*distance;
            }

            //LevelManager.currentLevel.AddDynamic(test, target, Quaternion.identity, null);
            GameConsole.Log($"WE WANT TO HIT HERE: {target}");
            Gizmos.DrawSphere(target,1);
            Vector3 dir = target - transform.position;
            Debug.DrawLine(transform.position,dir,Color.green,200);
            return Quaternion.LookRotation(dir,Vector3.up);
        }

        public LevelObjectData test;
        
        public void Shoot()
        {
            transform.parent = LevelManager.currentLevel.dynamicObjects;

            transform.rotation = GetAimDirection();
            flying = true;
            hitBox.Attach("Entity");
            hitBox.enterEvent.AddListener(x => { TryDamage(x); });
            Invoke("Drop",maxFlyingTime);
        }

        public void Drop()
        {
            LevelManager.currentLevel.RemoveDynamic(this, true);
        }
        
        private void Update()
        {
            Gizmos.DrawSphere(transform.position,1);
        }

        void FixedUpdate()
        {
            if(flying)
            {
                transform.position += speed*transform.forward;
            }
        }
    }
}