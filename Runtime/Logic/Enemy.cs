using System.Collections;
using System.Collections.Generic;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        public enum EnemyState { Idle = 1, Track, Attack, PrepareStrike, Strike, Dead };
        public EnemyState enemyState;
        // Start is called before the first frame update
        void Start()
        {

        }

        public virtual  void OnEnable()
        {
            UnityEngine.AI.NavMeshAgent n = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (n != null) n.enabled = true;
        }
        public virtual void OnDisable()
        {

            UnityEngine.AI.NavMeshAgent n = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (n != null) n.enabled = false;
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