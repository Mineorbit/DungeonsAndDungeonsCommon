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
            hitBox.Attach("Entity");
            hitBox.enterEvent.AddListener(x => { TryDamage(x); });
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
        // Update is called once per frame
        void Update()
        {

        }

        // THIS SHOULD BE FIXED
        public void OnColliderEnter(Collider other)
        {
            if(other.transform.GetComponentInParent<Bow>() != shootingBow || other.transform.GetComponentInChildren<Bow>() != shootingBow )
                Destroy(gameObject);
        }
        
        
        void FixedUpdate()
        {
            transform.position += 0.2f*transform.forward;
        }
    }
}