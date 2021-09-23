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

        public bool shotArrow = false;
        private bool flying = false;
        
        private float speed = 0.2f;
        private float maxFlyingTime = 10f;

        private Vector3 lastTarget;
        
        Quaternion GetAimDirection()
        {
            Vector3 target = Player.GetTargetPoint();
            //LevelManager.currentLevel.AddDynamic(test, target, Quaternion.identity, null);
            GameConsole.Log($"WE WANT TO HIT HERE: {target}");
            Vector3 dir = target - transform.position;
            lastTarget = target;
            Debug.DrawLine(transform.position,target,Color.green,200);
            return Quaternion.LookRotation(dir,Vector3.up);
        }


        
        public LevelObjectData test;
        
        public void Shoot()
        {
            transform.parent = LevelManager.currentLevel.dynamicObjects;

            transform.rotation = GetAimDirection();
            flying = true;
            shotArrow = true;
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
            if (!shotArrow)
            {
                transform.rotation = GetAimDirection();
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position,0.5f);
            Gizmos.DrawSphere(lastTarget,0.5f);
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