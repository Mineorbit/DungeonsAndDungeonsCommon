﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Sword : Item
    {
        public UnityEngine.Object hitboxPrefab;

        Hitbox hitBox;

        int damage = 20;

        public Rigidbody rigidBody;

        public override void OnAttach()
        {
            base.OnAttach();
            transform.localPosition = new Vector3(0, 0.00181f, 0);
            transform.localEulerAngles = new Vector3(0, -75, 90f);
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            GetComponent<Collider>().enabled = false;
            hitBox = (Instantiate(hitboxPrefab) as GameObject).GetComponent<Hitbox>();

            hitBox.Attach(owner.transform.Find("Model").gameObject, "Enemy", new Vector3(0, 0, 1));

            hitBox.enterEvent.AddListener((x) => { TryDamage(x); });
            hitBox.Deactivate();
        }

        public override void Use()
        {
            base.Use();
            owner.baseAnimator.Strike();
            hitBox.Activate();
        }

        void TryDamage(GameObject g)
        {
            Entity c = g.GetComponentInParent<Entity>(includeInactive: true);
            if (c != null)
            {
                c.Hit(this.owner,damage);
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

            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            GetComponent<Collider>().enabled = true;
        }

        public void OnDestroy()
        {
            Debug.Log("I am being destroyed");
            if (hitBox != null)
                Destroy(hitBox.gameObject);
        }
    }
}
