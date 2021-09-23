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
        private float distance = 15;
        
        private float speed = 0.2f;
        private float maxFlyingTime = 5f;

        Quaternion GetAimDirection()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            RaycastHit hit;
            Physics.Raycast(ray.origin, ray.direction, out hit, distance);

            Vector3 target = hit.point;
            Vector3 dir = target - transform.position;
            Debug.DrawRay(transform.position,dir,Color.green);
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