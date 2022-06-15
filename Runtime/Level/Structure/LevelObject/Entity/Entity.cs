using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
        
        public Rigidbody rigidbody;

        public bool invincible;


        public Hitbox itemHitbox;

        public List<Item> itemsInProximity = new List<Item>();

        private ItemHandle handle;
        
        public int maxHealth = 100;

        public bool cooldown;


        [FormerlySerializedAs("movementOverride")] public bool allowedToMove;

        int _points = 0;
        
        public float dazeTime = 1f;

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

    public virtual void FixedUpdate()
    {
        if(Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test) PerformSim();
       
        
        UpdateGround();
        ComputeCurrentSpeed();
    }

    // ONLY FOR MONSTERS AND PLAYERS?
    public void PerformSim()
    {
        if (transform.position.y < killHeight)
        {
            Kill();
        }
    }

    private Vector3 lastPosition;
        
    private readonly int k = 4;
        
    public List<float> lastSpeeds = new List<float>();
    
    public List<Vector3> lastSpeedDirections = new List<Vector3>();
    public float speed;

    public Vector3 speedDirection;
    public void ComputeCurrentSpeed()
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
        // TODO Eventually replace itemsInProximity with Hitbox collection feature
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

        public void FinishCooldown()
        {
            cooldown = false;
            setMovementStatus(true);
        }

        public virtual void setMovementStatus(bool allowedToMove)
        {
            this.allowedToMove = allowedToMove;
        }

        public virtual void OnDestroy()
        {
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


        private void StartCooldown(float cooldownTime)
        {
            cooldown = true;
            if (model != null)
            {
                StartCoroutine(HitCoolDownEffect(cooldownTime));
            }
            Invoke("FinishCooldown",cooldownTime);
        }

        private int numberOfBlinks = 8;
        public GameObject model;
        IEnumerator HitCoolDownEffect(float cooldownTime)
        {
            for (int i = 0; i < numberOfBlinks*2; i++)
            {
                yield return new WaitForSeconds(cooldownTime / numberOfBlinks);
                model.SetActive(false);
                yield return new WaitForSeconds(cooldownTime / numberOfBlinks);
                model.SetActive(true);
            }
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
            if(controller != null)
                controller.OnSpawn(location);
            onSpawnEvent.Invoke(location);
        }

        public virtual void Despawn()
        {
            health = 0;
            alive = false;
            gameObject.SetActive(false);
            GameConsole.Log($"Despawning { this}");
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            onDespawnEvent.Invoke();
        }


        public virtual void Teleport(Vector3 position)
        {
            GameConsole.Log($"Teleporting {this} to {position}");
            onTeleportEvent.Invoke(position);
            gameObject.SetActive(false);
            transform.position = position;
        }


        
        
        

        public virtual void FinishKickback()
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            setMovementStatus(true);
            if (controller != null)
            {
                controller.Activate();
            }
        }
        
        public void CreateKickback(Vector3 dir,float force, float time)
        {
            Vector3 direction = dir;
            // THIS IS A HOT FIX
            direction.y = Mathf.Max(0, direction.y);
            direction.Normalize();
            setMovementStatus(false);
            if(controller != null)
            {
                controller.Deactivate();
            }
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            GameConsole.Log($"Kickback Strength: {force} Direction: {direction}");
            rigidbody.AddForce(direction*force,ForceMode.VelocityChange);
            Invoke("FinishKickback",time);
        }

        
        public virtual void HitEffect(Vector3 hitPosition)
        {
            if(baseAnimator != null)
            {
                baseAnimator.Hit();
            }
            Vector3 dir = transform.position - hitPosition;
            EffectCaster.FX("HitFX",transform.position+0.5f*dir);
            
        }

        public virtual void StopHitEffect()
        {
            if(baseAnimator != null)
                baseAnimator.StopHit();
        }

        
        
        
        // ApplyMovement controls whether entity should have local simulation movement appliance or not
        public bool ApplyMovement = true;
        
        public Quaternion aimRotation;

        
        public bool lastGrounded = false;

        public bool isGrounded;
        
        public float heightRay;
        private float clip = 0.005f;

        public float speedY = 0;
        public float gravity = 1f;

        public (bool,RaycastHit) GroundCheck()
        {
            var mask = 1 << 10 | 1 << 11;
            var hit = new RaycastHit();
            bool raycast =  Physics.Raycast(transform.position, Vector3.down, out hit, heightRay,
                mask, QueryTriggerInteraction.Ignore);
            return (raycast,hit);
        }
        public virtual void UpdateGround()
        {
            lastGrounded = isGrounded;
            bool raycast;
            RaycastHit hit;
            (raycast,hit) = GroundCheck();
            
            // This variable is only updated on move
            //controller.controller.Move(Vector3.zero);
            bool controllerResult = false;
            if(controller != null && controller.controller != null) controllerResult = controller.controller.isGrounded;
            isGrounded = raycast || controllerResult;
            
           // GameConsole.Log($"{this} Distance {hit.distance}");
            //if (isGrounded && (hit.distance <  (heightRay - clip)))
            //{
             //   GameConsole.Log("raising up because of ground clip");
                //transform.position += Vector3.up * (heightRay - hit.distance);
            //}
        }
        
        public bool IsGrounded()
        {
            return isGrounded;
        }

        
        
        
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

        public float dazeKickbackDistance = 0.125f;
        
        public virtual void DazeEffect(Vector3 hitPosition)
        {
            Vector3 dir = transform.position - hitPosition;
            CreateKickback(dir,dazeKickbackDistance,1f);
        }

        public virtual void UndazeEffect()
        {
            
        }
        
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

        public float minCooldownTime = 1f;
        public float kickbackSpeed = 4f;

        public float DamageMultiplier(LevelObject hitter)
        {
            float elementTypeMultiplier = elementType.GetMultiplier(hitter.elementType);
            return 1*elementTypeMultiplier;
        }
        
        public virtual void Hit(LevelObject hitter, int outputDamage)
        {

            if (Level.instantiateType == Level.InstantiateType.Play ||
                Level.instantiateType == Level.InstantiateType.Test)
                if (!invincible && !cooldown && hitter != this)
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

                    int damage = (int)( DamageMultiplier(hitter) * outputDamage);
                    
                    
                    setMovementStatus(false);
                    
                    Vector3 dir = transform.position - hitter.transform.position;
                    float kickbackDistance = 0.5f + (float) damage / 10;
                    
                    
                    float time = kickbackDistance / kickbackSpeed;
                    float kickbackForce = damage / kickbackSpeed;
                    CreateKickback(dir,kickbackForce, time);
                    
                    Invoke(HitEffect,hitter.transform.position,true,true);

                    if (hitter.GetType().IsInstanceOfType(typeof(Entity)))
                    {
                        ((Entity)hitter).points += damage;
                    }

                    float cooldownTime = time + minCooldownTime;
                    GameConsole.Log($"CooldownTime {time}");
                    StartCooldown(cooldownTime);
                    Invoke("StopHitEffect", time);
                    GameConsole.Log($"{hitter.gameObject.name}  HIT  {gameObject.name} AND CAUSED {damage} HP DAMAGE");
                    health = health - damage;

                    onHitEvent.Invoke(hitter);
                    if (health <= 0)
                    {
                        if (hitter.GetType().IsInstanceOfType(typeof(Entity)))
                        {
                            ((Entity)hitter).points += pointsForKill;
                        }
                        Kill();
                    }
                    //FreezeFramer.freeze(0.0075f);
                }
        }

        private float timeForDeath = 1f;
        public virtual void Kill()
        {
            // SetState(Enemy.EnemyState.Dead);
            //eventually call
            setMovementStatus(false);
            Invoke(KillEffect);
            Invoke("Despawn", timeForDeath);
        }

        public virtual void KillEffect()
        {
            if(baseAnimator != null)
                baseAnimator.Death();
        }
    }
}