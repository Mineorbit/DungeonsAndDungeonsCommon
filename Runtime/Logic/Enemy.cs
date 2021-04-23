using System.Collections;
using System.Collections.Generic;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        EnemyController enemyController;
        public enum EnemyState { Idle = 1, Track, Attack, PrepareStrike, Strike, Dead };
        public EnemyState enemyState;
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