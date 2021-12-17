using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        }

    public virtual  void FixedUpdate()
    {
        if(Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test) PerformSim();
        ComputeCurrentSpeed();
    }

    public void PerformSim()
    {
        if (transform.position.y < killHeight) Kill();
    }

    private Vector3 lastPosition;
        
    private readonly int k = 4;
        
    public List<float> lastSpeeds = new List<float>();
    
    public List<Vector3> lastSpeedDirections = new List<Vector3>();
    public float speed;

    public Vector3 speedDirection;
    private void ComputeCurrentSpeed()
    {
    lastSpeeds.Add((transform.position - lastPosition).magnitude / Time.deltaTime);
    lastSpeedDirections.Add(transform.position-lastPosition);
    
    if (lastSpeeds.Count > k) lastSpeeds.RemoveAt(0);
    if (lastSpeedDirections.Count > k) lastSpeedDirections.RemoveAt(0);
    float sum = 0;
        for (var i = 0; i < lastSpeeds.Count; i++) sum += lastSpeeds[i];
    speed = sum / k;

    speedDirection = new Vector3(lastSpeedDirections.Average(x => x.x), lastSpeedDirections.Average(x => x.y),
        lastSpeedDirections.Average(x => x.z));
    
    lastPosition = transform.position;
    }
        
        private void SetupItems()
        {


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
                GameConsole.Log($"{this} has no ItemHitBox");
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
            GameConsole.Log($"Spawning {this} at {location}");
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
            GameConsole.Log($"Despawning { this}");
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


        IEnumerator KickbackRoutine(Vector3 dir, float dist, float kickbackSpeed)
        {
            float t = 0;
            Vector3 start = transform.position;
            float kickbackTime =  dist / kickbackSpeed;
            while (t<kickbackTime)
            {
                t += Time.deltaTime;
                controller.controller.Move(0.05f*kickbackSpeed*dir);
                yield return new WaitForEndOfFrame();
            }
        }
        
        public void Kickback(Vector3 dir, float kickbackDistance, float kickbackSpeed)
        {
            Vector3 direction = dir;
            direction.Normalize();
            StartCoroutine(KickbackRoutine(direction,kickbackDistance, kickbackSpeed));
        }
        
        public void CreateKickback(Vector3 dir,float kickbackDistance, float kickbackSpeed)
        {
            // FOR NOW ONLY MANIPULATE ON PLANE
            dir.y = 0;
            Kickback(dir, kickbackDistance, kickbackSpeed);
        }
        
        public virtual void HitEffect(Vector3 hitPosition)
        {
            baseAnimator.Hit();
            Vector3 dir = transform.position - hitPosition;
            EffectCaster.HitFX(transform.position+0.5f*dir);
            
        }

        
        
        
        // ApplyMovement controls whether entity should have local simulation movement appliance or not
        public bool ApplyMovement = true;
        
        public Quaternion aimRotation;

        
        
        public Quaternion GetAimRotation()
        {
            return aimRotation;
        }
        
        
        public virtual bool HitBlocked(Vector3 hitDirection)
        {
            return false;
        }

        public virtual void BlockHit()
        {
            
        }

        public virtual void DazeEffect(Vector3 hitPosition)
        {
            Vector3 dir = transform.position - hitPosition;
            CreateKickback(dir,0.125f,1f);
        }

        public virtual void UndazeEffect()
        {
            
        }
        
        private float dazeTime = 1f;
        public void Daze(Entity dazer)
        {
            setMovementStatus(false);
            DazeEffect(dazer.transform.position);
            Invoke("Undaze",dazeTime);
        }

        public void Undaze()
        {
            UndazeEffect();
            setMovementStatus(true);
        }

        private int pointsForKill = 100;

        public virtual void Hit(LevelObject hitter, int damage)
        {

            if (Level.instantiateType == Level.InstantiateType.Play ||
                Level.instantiateType == Level.InstantiateType.Test)
                if (!invincible && !hitCooldown)
                {
                    if (HitBlocked( ( hitter.transform.position - transform.position)))
                    {
                        BlockHit();
                        if(hitter.GetType().IsSubclassOf(typeof(Entity)))
                        {
                            ( (Entity) hitter).Daze(this);
                        }
                        return;
                    }
                    
                    Invoke(setMovementStatus, false);
                    
                    Vector3 dir = transform.position - hitter.transform.position;
                    CreateKickback(dir,0.5f, 2f);
                    
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