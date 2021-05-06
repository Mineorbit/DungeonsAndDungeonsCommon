using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Entity : NetworkLevelObject
    {
        public int health;

        public List<Item> items;
        public ItemHandle[] itemHandles;

        public EntityBaseAnimator baseAnimator;

        public UnityEvent<Entity> onAttackEvent;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if(transform.position.y < -8)
            {
                Invoke(Kill);
            }
        }


        bool hitCooldown = false;

        IEnumerator HitTimer(float time)
        {
            yield return new WaitForSeconds(time);
            hitCooldown = false;
        }

        public virtual void setMovementStatus(bool allowedToMove)
        {
        }

        ItemHandle handle;

        public void UseHandle(ItemHandle h)
        {
            if (h != null)
            {
                handle = h;
                Invoke(UseSelectedHandle);
            }
        }

        public void UseSelectedHandle()
        {
            handle.Use();
        }

        void StartHitCooldown()
        {
            hitCooldown = true;
            StartCoroutine(HitTimer(0.5f));
        }


        public virtual void Hit(Entity hitter,int damage)
        {
            if (!hitCooldown)
            {
                onAttackEvent.Invoke(hitter);
                StartHitCooldown();
                Debug.Log(hitter.gameObject.name+" HIT "+this.gameObject.name+" AND CAUSED "+damage+" HP DAMAGE");
                health = health - damage;
                if (health <= 0)
                {
                    Invoke(Kill);
                }
                //FreezeFramer.freeze(0.0075f);
            }

        }

        public virtual void Kill()
        {
            // SetState(Enemy.EnemyState.Dead);
            //eventually call
            LevelManager.currentLevel.RemoveDynamic(this);
        }
    }
}
