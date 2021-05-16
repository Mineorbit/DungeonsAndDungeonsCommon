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

        public List<Item> items = new List<Item>();
        public ItemHandle[] itemHandles;

        public EntityBaseAnimator baseAnimator;

        public UnityEvent<Entity> onHitEvent = new UnityEvent<Entity>();


        public UnityEvent<Vector3> onSpawnEvent =  new UnityEvent<Vector3>();

        public UnityEvent onDespawnEvent = new UnityEvent();

        public new EntityNetworkHandler levelObjectNetworkHandler;

        public EntityController controller;

        public bool invincible = false;

        ItemHandle handle;

        public LevelLoadTarget loadTarget;

        public Hitbox itemHitbox;

        public List<Item> itemsInProximity = new List<Item>();



        // this needs to be prettier
        public virtual void Update()
        {
            

            if(transform.position.y < -8)
            {
                Invoke(Kill);
            }
        }

        public virtual void Start()
        {
            SetupItems();
        }




        void SetupItems()
        {
            // Temporary
            itemHandles = gameObject.GetComponentsInChildren<ItemHandle>();


            if (itemHitbox != null)
            {
            Debug.Log("ITEM HITBOX");
            itemHitbox.Attach("Item");
            itemHitbox.enterEvent.AddListener((x) => { Debug.Log("HANLOOOO :D"); Item i = x.GetComponent<Item>(); if (!itemsInProximity.Contains(i)) itemsInProximity.Add(i); });
            itemHitbox.exitEvent.AddListener((x) => { Debug.Log(x.name); itemsInProximity.RemoveAll((p) => p == x.GetComponent<Item>()); });
            }else
            {
                Debug.Log(this+" has no ItemHitBox");
            }
        }



        bool hitCooldown = false;

        IEnumerator HitTimer(float time)
        {
            yield return new WaitForSeconds(time);
            hitCooldown = false;
            Invoke(setMovementStatus,true);
        }

        public virtual void setMovementStatus(bool allowedToMove)
        {
        }

        public void UseHandle(ItemHandle h)
        {
            h.Use();
        }

        public void StopUseHandle(ItemHandle h)
        {
            h.StopUse();
        }


        void StartHitCooldown()
        {
            hitCooldown = true;
            StartCoroutine(HitTimer(1.5f));
        }
        //EVENTS ALWAYS LAST
        public virtual void Spawn(Vector3 location, Quaternion rotation, bool allowedToMove)
        {
            health = 100;
            alive = true;
            gameObject.SetActive(true);
            Debug.Log("Spawning "+this+" at "+location);
            transform.position = location;
            transform.rotation = rotation;
            controller.OnSpawn(location);
            onSpawnEvent.Invoke(location);
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


        public void HitEffect()
        {
            baseAnimator.Hit();
        }

        public virtual void Hit(Entity hitter,int damage)
        {

            Invoke(HitEffect);


            if (Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test)
            {
                if (!invincible && !hitCooldown)
                {
                onHitEvent.Invoke(hitter);
                Invoke(setMovementStatus,false);
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
        }

        public virtual void Kill()
        {
            // SetState(Enemy.EnemyState.Dead);
            //eventually call
            Despawn();
        }
    }
}
