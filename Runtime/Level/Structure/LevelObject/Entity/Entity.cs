using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Entity : NetworkLevelObject
    {
        public int health;

        public bool alive;

        public List<Item> items;
        public ItemHandle[] itemHandles;

        public EntityBaseAnimator baseAnimator;

        public UnityEvent<Entity> onAttackEvent;


        public UnityEvent<Vector3> onSpawnEvent =  new UnityEvent<Vector3>();

        public UnityEvent onDespawnEvent = new UnityEvent();

        public new EntityNetworkHandler levelObjectNetworkHandler;

        public EntityController controller;

        public bool invincible = false;

        ItemHandle handle;


        // this needs to be prettier
        public virtual void Update()
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
            setMovementStatus(true);
        }

        public virtual void setMovementStatus(bool allowedToMove)
        {
        }

        public void UseHandle(ItemHandle h)
        {
            if (h != null)
            {
                handle = h;
                Invoke(UseSelectedHandle);
            }
        }

        public void StopUseHandle(ItemHandle h)
        {
            if (h != null)
            {
                handle = h;
                Invoke(StopUseSelectedHandle);
            }
        }

        public void UseSelectedHandle()
        {
            if(handle != null)
            handle.Use();
        }

        public void StopUseSelectedHandle()
        {
            if (handle != null)
                handle.StopUse();
        }

        void StartHitCooldown()
        {
            hitCooldown = true;
            StartCoroutine(HitTimer(1.5f));
        }
        public virtual void Spawn(Vector3 location, Quaternion rotation, bool allowedToMove)
        {
            health = 100;
            alive = true;
            gameObject.SetActive(true);
            Debug.Log("Spawning "+this+" at "+location);
            transform.position = location;
            transform.rotation = rotation;

            onSpawnEvent.Invoke(location);
            controller.OnSpawn(location);
        }

        public virtual void Despawn()
        {
            health = 0;
            alive = false;
            gameObject.SetActive(false);
            Debug.Log("Despawning " + this);
            transform.position = new Vector3(0,0,0);
            transform.rotation = new Quaternion(0,0,0,0);
            onDespawnEvent.Invoke();
        }


        public virtual void Hit(Entity hitter,int damage)
        {
            if (!invincible && !hitCooldown)
            {
                onAttackEvent.Invoke(hitter);
                setMovementStatus(false);
                StartHitCooldown();
                Debug.Log(hitter.gameObject.name+" HIT "+this.gameObject.name+" AND CAUSED "+damage+" HP DAMAGE");
                health = health - damage;
                baseAnimator.Hit();
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
            Despawn();
        }
    }
}
