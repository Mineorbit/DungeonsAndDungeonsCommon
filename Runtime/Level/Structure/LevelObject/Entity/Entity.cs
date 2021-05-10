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

        public new EntityNetworkHandler levelObjectNetworkHandler;

        public EntityController controller;

        public bool invincible = false;


        float skipDistance = 1f;
        // this needs to be prettier
        public void Update()
        {
            if(NetworkManager.isConnected)
            {

                //This needs to be replaced by a EntityTeleport (EntitySetBack) packet
                float distance = (controller.controllerPosition - levelObjectNetworkHandler.networkPosition).magnitude;
                if(distance<skipDistance)
                {
                transform.position = (controller.controllerPosition + levelObjectNetworkHandler.networkPosition) / 2;
                }
                else
                {
                    transform.position = levelObjectNetworkHandler.networkPosition;
                    controller.UpdatePosition(levelObjectNetworkHandler.networkPosition);
                }
            }
            else
            {
                transform.position = controller.controllerPosition;
            }

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

        ItemHandle handle;

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
            transform.position = location;
            transform.rotation = rotation;
            controller.OnSpawn(location);
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
            LevelManager.currentLevel.RemoveDynamic(this);
        }
    }
}
