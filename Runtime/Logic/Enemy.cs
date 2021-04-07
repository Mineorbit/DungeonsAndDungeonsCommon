using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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