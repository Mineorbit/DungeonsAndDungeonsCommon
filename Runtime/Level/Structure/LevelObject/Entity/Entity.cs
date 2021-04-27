using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Entity : LevelObject
    {
        public int health;

        public UnityEvent<Entity> onAttackEvent;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if(transform.position.y < 0)
            {
                Kill();
            }
        }


        bool hitCooldown = false;

        IEnumerator HitTimer(float time)
        {
            yield return new WaitForSeconds(time);
            hitCooldown = false;
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
                    Kill();
                }
                //FreezeFramer.freeze(0.0075f);
            }

        }

        public void Kill()
        {
            // SetState(Enemy.EnemyState.Dead);
            //eventually call
            LevelManager.currentLevel.RemoveDynamic(this);
        }
    }
}
