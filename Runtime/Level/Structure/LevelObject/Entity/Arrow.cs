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
            transform.localRotation = Quaternion.identity;
        }
        private void TryDamage(GameObject g)
        {
            var c = g.GetComponentInParent<Entity>(true);
            if (c != null && c.GetComponentInChildren<Bow>() != shootingBow)
            {
                //onHitEvent.Invoke();
                c.Hit(this, 20);
                Destroy(gameObject);
            }
        }
        

        // THIS SHOULD BE FIXED
        public void OnColliderEnter(Collider other)
        {
            if (other.transform.GetComponentInParent<Bow>() != shootingBow ||
                other.transform.GetComponentInChildren<Bow>() != shootingBow)
                Drop();
        }

        private bool flying = false;
        private float distance = 15;
        
        private float speed = 0.2f;
        private float maxFlyingTime = 5f;

        Quaternion GetAimDirection()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 target = ray.GetPoint(distance);
            Vector3 dir = target - transform.position;
            return Quaternion.LookRotation(dir,Vector3.up);
        }
        
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
            if (!flying)
            {
                transform.localRotation = Quaternion.identity;
            }
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