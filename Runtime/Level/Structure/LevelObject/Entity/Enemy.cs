using UnityEngine;
using UnityEngine.AI;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        public float viewDistance = 15;
        
        public float attackDistance = 4;

        // Enemy Properties

        public bool forgetPlayer;

        public bool seeThroughWalls;
        
        public float currentDamage;

        public BehaviorTree behaviorTree;
        
        public float damageMultiplier;
        
        public float baseDamage;
        
        public Entity targetEntity;
        
        public EnemyController GetController()
        {
            return (EnemyController) controller;
        }

        public override void UpdateGround()
        {
            base.UpdateGround();
            isGrounded = isGrounded ||
                         (GetController().navMeshAgent.enabled && GetController().navMeshAgent.isOnNavMesh);
        }

        public virtual void OnEnable()
        {
            controller = GetComponent<EnemyController>();
            var n = GetComponent<NavMeshAgent>();
            if (n != null) n.enabled = false;
            controller.enabled = true;
        }

        public override void OnStartRound()
        {
            
            base.OnStartRound();
            health = maxHealth;
            setMovementStatus(true);
        }

        public virtual void OnDisable()
        {
            controller = GetComponent<EnemyController>();
            var n = GetComponent<NavMeshAgent>();
            if (n != null) n.enabled = false;
            controller.enabled = false;
        }

        public void Counter(Entity attacker)
        {
            targetEntity = attacker;
        }


        public override void Spawn(Vector3 location, Quaternion rotation, bool allowedToMove)
        {
            base.Spawn(location, rotation, allowedToMove);
            responsive = true;
        }
        
        public void TryDamage(GameObject g, float damage)
        {
            var c = g.GetComponentInParent<Entity>(true);
            if (c != null)
            {
                c.Hit(this, (int) damage);
            }
        }

        public override void OnInit()
        {
            base.OnInit();

            GetController().seenPlayer = null;
            GetController().seenAlly = null;
            GetController().lastSeenPlayer = null;
           
        }


        private bool responsive = true;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(behaviorTree != null && alive && responsive)
                behaviorTree.Tick();
        }

        public override void Kill()
        {
            base.Kill();
            responsive = false;
        }
    }
}