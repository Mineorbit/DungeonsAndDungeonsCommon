using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Sword : Item
    {
        public UnityEngine.Object hitboxPrefab;

        Hitbox hitBox;

        int damage = 20;

        public override void OnAttach()
        {
            base.OnAttach();
            GetComponent<Collider>().enabled = false;
            hitBox = (Instantiate(hitboxPrefab) as GameObject).GetComponent<Hitbox>();
            hitBox.Attach(owner, "Enemy", new Vector3(0, 0, 1));
            hitBox.enterEvent.AddListener((x) => { TryDamage(x); });
            hitBox.Deactivate();
        }

        public override void Use()
        {
            base.Use();
            hitBox.Activate();
        }

        void TryDamage(GameObject g)
        {
            Entity c = g.GetComponentInParent<Entity>(includeInactive: true);
            if (c != null)
            {
                c.Hit(damage);
            }
        }

        public override void StopUse()
        {
            hitBox.Deactivate();
        }

        public override void OnDettach()
        {
            if(hitBox != null)
            Destroy(hitBox.gameObject);

            GetComponent<Collider>().enabled = true;
        }
        public void OnDestroy()
        {
            if (hitBox != null)
                Destroy(hitBox.gameObject);
        }
    }
}
