using System;
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

        public UnityEvent<int> onPointsChangedEvent = new UnityEvent<int>();

        public UnityEvent<Vector3> onSpawnEvent = new UnityEvent<Vector3>();

        public UnityEvent onDespawnEvent = new UnityEvent();

        public UnityEvent<Vector3> onTeleportEvent = new UnityEvent<Vector3>();

        public new EntityNetworkHandler levelObjectNetworkHandler;

        public EntityController controller;

        public bool invincible;

        public LevelLoadTarget loadTarget;

        public Hitbox itemHitbox;

        public List<Item> itemsInProximity = new List<Item>();

        private ItemHandle handle;


        public int maxHealth = 100;

        internal bool hitCooldown;

        public float hitCooldownTime = 1f;

        public bool movementOverride;

        int _points = 0;

        public int points
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
                onPointsChangedEvent.Invoke(points);
            }
        }
        
        public virtual void Start()
        {
            SetupItems();
        }


        // this needs to be prettier
        public virtual void Update()
        {
            if (transform.position.y < -8) Invoke(Kill);
        }


        
        
        private void SetupItems()
        {
            // Temporary
            itemHandles = gameObject.GetComponentsInChildren<ItemHandle>();


            if (itemHitbox != null)
            {
                Debug.Log("ITEM HITBOX for " + this);
                itemHitbox.Attach("Item");
                itemHitbox.enterEvent.AddListener(x =>
                {
                    var i = x.GetComponent<Item>();
                    if (!itemsInProximity.Contains(i)) itemsInProximity.Add(i);
                });
                itemHitbox.exitEvent.AddListener(x =>
                {
                    Debug.Log(x.name);
                    itemsInProximity.RemoveAll(p => p == x.GetComponent<Item>());
                });
            }
            else
            {
                Debug.Log(this + " has no ItemHitBox");
            }
        }

        private IEnumerator HitTimer(float time)
        {
            yield return new WaitForSeconds(time);
            hitCooldown = false;
            Invoke(setMovementStatus, true);
        }

        public virtual void setMovementStatus(bool allowedToMove)
        {
            movementOverride = allowedToMove;
        }

        public virtual void OnDestroy()
        {
            Destroy(loadTarget.gameObject);
        }

        public void UseHandle(ItemHandle h)
        {
            h.Use();
        }

        public void StopUseHandle(ItemHandle h)
        {
            h.StopUse();
        }


        private void StartHitCooldown()
        {
            hitCooldown = true;
            StartCoroutine(HitTimer(hitCooldownTime));
        }

        //EVENTS ALWAYS LAST
        public virtual void Spawn(Vector3 location, Quaternion rotation, bool allowedToMove)
        {
            health = maxHealth;
            alive = true;
            gameObject.SetActive(true);
            Debug.Log("Spawning " + this + " at " + location);
            Teleport(location);
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
            transform.position = new Vector3(0, 0, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
            onDespawnEvent.Invoke();
        }


        public void Teleport(Vector3 position)
        {
            onTeleportEvent.Invoke(position);
            gameObject.SetActive(false);
            Debug.Log(this+" deactivated for TP");
            transform.position = position;
            loadTarget.WaitForChunkLoaded(position, () => { gameObject.SetActive(true); 
                Debug.Log(this+" reactivated for TP");});
        }


        private float kickbackTime = 1f;
        private float kickbackSpeed = 2f;
        IEnumerator KickbackRoutine(Vector3 dir, float dist)
        {
            float t = 0;
            Vector3 start = transform.position;
            while (t<kickbackTime)
            {
                t += kickbackSpeed * Time.deltaTime;
                transform.position = start + dir * dist * t;
                yield return new WaitForEndOfFrame();
            }
            transform.position = start + dir * dist;
        }
        
        public void Kickback(Vector3 dir, float distance)
        {
            Vector3 direction = dir;
            direction.Normalize();
            StartCoroutine(KickbackRoutine(direction,distance));
        }
        
        public void HitEffect(Vector3 dir)
        {
            baseAnimator.Hit();
            
            Kickback(dir,2f);
        }

        private int pointsForKill = 100;

        public virtual void Hit(Entity hitter, int damage)
        {

            if (Level.instantiateType == Level.InstantiateType.Play ||
                Level.instantiateType == Level.InstantiateType.Test)
                if (!invincible && !hitCooldown)
                {
                    onHitEvent.Invoke(hitter);
                    Invoke(setMovementStatus, false);
                    
                    
                    Invoke(HitEffect,transform.position - hitter.transform.position);

                    hitter.points += damage;
                    
                    StartHitCooldown();
                    Debug.Log(hitter.gameObject.name + " HIT " + gameObject.name + " AND CAUSED " + damage +
                              " HP DAMAGE");
                    health = health - damage;

                    if (health <= 0)
                    {
                        Invoke(Kill);
                        hitter.points += pointsForKill;
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