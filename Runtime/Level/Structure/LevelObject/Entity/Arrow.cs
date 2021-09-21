using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Arrow : Entity
    {
        private Bow shootingBow;
        // Start is called before the first frame update
        public Hitbox hitBox;
        void Start()
        {
            hitBox.enterEvent.AddListener(x => { TryDamage(x); });
        }
        private void TryDamage(GameObject g)
        {
            var c = g.GetComponentInParent<Entity>(true);
            if (c != null)
            {
                //onHitEvent.Invoke();
                c.Hit(this, 20);
            }
        }
        // Update is called once per frame
        void Update()
        {

        }

        // THIS SHOULD BE FIXED
        public void OnColliderEnter(Collider other)
        {
            if(other.transform.GetComponentInParent<Bow>() != shootingBow)
                Destroy(gameObject);
        }
        
        
        void FixedUpdate()
        {
            transform.position += transform.forward;
        }
    }
}