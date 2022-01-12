using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Sword : Item
    {
        public Object hitboxPrefab;

        public ParticleSystem effectSystem;

        public UnityEvent onHitEvent = new UnityEvent();

        private bool block;

        private readonly int damage = 20;
        
        private Hitbox hitBox;



        private readonly float whipeTime = 0.02f;

        private void Start()
        {
        }


        public void OnDestroy()
        {
            if (hitBox != null)
                Destroy(hitBox.gameObject);
        }

        public override void OnAttach()
        {
            base.OnAttach();
            SetAttachmentPosition();
            hitBox = (Instantiate(hitboxPrefab) as GameObject).GetComponent<Hitbox>();
            hitBox.Attach(owner.transform.Find("Model").gameObject, "Entity", new Vector3(0, 0, -2));
            

            hitBox.enterEvent.AddListener(x => { TryDamage(x); });
            hitBox.Deactivate();
        }

        public override void SetAttachmentPosition()
        {
        }
    
        public override void Use()
        {
            if (!block)
            {
                block = true;
                base.Use();

                owner.baseAnimator.Strike();
                hitBox.Activate();

                // factor out just like audio component
                effectSystem.Play();
                TimerManager.StartTimer(whipeTime, () => { effectSystem.Stop(); });
            }
        }


        private void TryDamage(GameObject g)
        {
            var c = g.GetComponentInParent<Entity>(true);
            if (c != null && c != owner)
            {
                onHitEvent.Invoke();
                c.Hit(owner, damage);
            }
        }

        public override void StopUse()
        {
            base.StopUse();
            effectSystem.Stop();
            hitBox.Deactivate();
            block = false;
        }

        public override void OnDettach()
        {
            base.OnDettach();
            if (hitBox != null)
                Destroy(hitBox.gameObject);
        }

        public UnityEvent onCollideEvent = new UnityEvent();
        
        void OnCollisionEnter(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            }
            if (collision.relativeVelocity.magnitude > 2)
                onCollideEvent.Invoke();
        }
    }
}