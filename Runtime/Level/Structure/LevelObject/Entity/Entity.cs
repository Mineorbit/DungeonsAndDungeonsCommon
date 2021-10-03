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

        public UnityEvent<LevelObject> onHitEvent = new UnityEvent<LevelObject>();

        public UnityEvent<int> onPointsChangedEvent = new UnityEvent<int>();

        public UnityEvent<Vector3> onSpawnEvent = new UnityEvent<Vector3>();

        public UnityEvent onDespawnEvent = new UnityEvent();

        public UnityEvent<Vector3> onTeleportEvent = new UnityEvent<Vector3>();


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

        public EntityNetworkHandler GetNetworkHandler()
        {
            return (EntityNetworkHandler) levelObjectNetworkHandler;
        }
        
        public virtual void Start()
        {
            SetupItems();
        }

		float killHeight = -8f;

        // this needs to be prettier
        public virtual void Update()
        {
            if (transform.position.y < killHeight) Invoke(Kill);
        }

    public virtual  void FixedUpdate()
    {
        ComputeCurrentSpeed();
    }

    private Vector3 lastPosition;
        
    private readonly int k = 10;
        
    public List<float> lastSpeeds = new List<float>();
    
    public float speed;
    private void ComputeCurrentSpeed()
    {
    lastSpeeds.Add((transform.position - lastPosition).magnitude / Time.deltaTime);
    if (lastSpeeds.Count > k) lastSpeeds.RemoveAt(0);
    float sum = 0;
        for (var i = 0; i < lastSpeeds.Count; i++) sum += lastSpeeds[i];
    speed = sum / k;
    lastPosition = transform.position;
    }
        
        private void SetupItems()
        {
            // Temporary
            itemHandles = gameObject.GetComponentsInChildren<ItemHandle>();


            if (itemHitbox != null)
            {
                itemHitbox.Attach("Item");
                itemHitbox.enterEvent.AddListener(x =>
                {
                    var i = x.GetComponent<Item>();
                    if (!itemsInProximity.Contains(i)) itemsInProximity.Add(i);
                });
                itemHitbox.exitEvent.AddListener(x =>
                {
                    itemsInProximity.RemoveAll(p => p == x.GetComponent<Item>());
                });
            }
            else
            {
                GameConsole.Log(this + " has no ItemHitBox");
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
            if(loadTarget != null)
                Destroy(loadTarget.gameObject);
        }

        public void UseHandle(ItemHandle h)
        {
            GameConsole.Log($"Using {h.slot}");
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
            GameConsole.Log("Spawning " + this + " at " + location);
            Teleport(location);
            gameObject.SetActive(true);
            transform.rotation = rotation;
            controller.OnSpawn(location);
            onSpawnEvent.Invoke(location);
        }

        public virtual void Despawn()
        {
            health = 0;
            alive = false;
            gameObject.SetActive(false);
            GameConsole.Log("Despawning " + this);
            transform.position = new Vector3(0, 0, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
            onDespawnEvent.Invoke();
        }


        public void Teleport(Vector3 position)
        {
            GameConsole.Log($"Teleporting {this} to {position}");
            onTeleportEvent.Invoke(position);
            gameObject.SetActive(false);
            transform.position = position;
            if(loadTarget != null)
                loadTarget.WaitForChunkLoaded(position, () => { gameObject.SetActive(true); });
        }


        private float kickbackDist = 1f;
        private float kickbackSpeed = 2f;
        IEnumerator KickbackRoutine(Vector3 dir, float dist)
        {
            float t = 0;
            Vector3 start = transform.position;
            float kickbackTime =  kickbackDist / kickbackSpeed;
            while (t<kickbackTime)
            {
                t += Time.deltaTime;
                controller.controller.Move(kickbackSpeed*dir);
                yield return new WaitForEndOfFrame();
            }
        }
        
        public void Kickback(Vector3 dir, float distance)
        {
            Vector3 direction = dir;
            direction.Normalize();
            StartCoroutine(KickbackRoutine(direction,distance));
        }
        
        public void HitEffect(Vector3 hitPosition)
        {
            baseAnimator.Hit();
            Vector3 dir = transform.position - hitPosition;
            dir.Normalize();
            // FOR NOW ONLY MANIPULATE ON PLANE
            dir.y = 0;
            EffectCaster.HitFX(transform.position+0.5f*dir);
            Kickback(dir,2f);
        }
        
        public Quaternion aimRotation;

        
        
        public Quaternion GetAimRotation()
        {
            return aimRotation;
        }
        


        private int pointsForKill = 100;

        public virtual void Hit(LevelObject hitter, int damage)
        {

            if (Level.instantiateType == Level.InstantiateType.Play ||
                Level.instantiateType == Level.InstantiateType.Test)
                if (!invincible && !hitCooldown)
                {
                    Invoke(setMovementStatus, false);
                    Invoke(HitEffect,hitter.transform.position);

                    if (hitter.GetType().IsInstanceOfType(typeof(Entity)))
                    {
                        ((Entity)hitter).points += damage;
                    }
                    
                    StartHitCooldown();
                    GameConsole.Log($"{hitter.gameObject.name}  HIT  {gameObject.name} AND CAUSED {damage} HP DAMAGE");
                    health = health - damage;

                    onHitEvent.Invoke(hitter);
                    if (health <= 0)
                    {
                        if (hitter.GetType().IsInstanceOfType(typeof(Entity)))
                        {
                            ((Entity)hitter).points += pointsForKill;
                        }
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