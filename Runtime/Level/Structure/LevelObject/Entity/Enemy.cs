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
        
        public int damage = 5;
        
        public float currentDamage;

        private BehaviorTree _behaviorTree;
        
        public EnemyController GetController()
        {
            return (EnemyController) controller;
        }
        
        public virtual void OnEnable()
        {
            controller = GetComponent<EnemyController>();
            var n = GetComponent<NavMeshAgent>();
            if (n != null) n.enabled = true;
            controller.enabled = true;
        }

        public virtual void OnDisable()
        {
            controller = GetComponent<EnemyController>();
            var n = GetComponent<NavMeshAgent>();
            if (n != null) n.enabled = false;
            controller.enabled = false;
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

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _behaviorTree.Tick();
        }
    }
}