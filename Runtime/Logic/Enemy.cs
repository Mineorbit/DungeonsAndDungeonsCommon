using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        EnemyController enemyController;
        public enum EnemyState { Idle = 1, Track,  PrepareStrike, Strike, Attack, Dead };
        public EnemyState enemyState;

        public int damage = 5;

        // Start is called before the first frame update
        void Start()
        {

        }

        public virtual  void OnEnable()
        {
            enemyController = GetComponent<EnemyController>();
            UnityEngine.AI.NavMeshAgent n = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (n != null) n.enabled = true;
            enemyController.enabled = true;
        }
        public virtual void OnDisable()
        {

            enemyController = GetComponent<EnemyController>();
            UnityEngine.AI.NavMeshAgent n = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (n != null) n.enabled = false;
            enemyController.enabled = false;
        }

        public void TryDamage(GameObject g, float hitDamage)
        {
            Entity c = g.GetComponentInParent<Entity>(includeInactive: true);
            if (c != null)
            {
                c.Hit((int) hitDamage);
            }
        }

        public EnemyState GetState()
        {
            return enemyState;
        }

        public void SetState(EnemyState state)
        {
            enemyState = state;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}