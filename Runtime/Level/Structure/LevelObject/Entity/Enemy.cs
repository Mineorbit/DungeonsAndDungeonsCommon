using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        EnemyController enemyController;


        // Enemy Properties


        public bool seeThroughWalls = false;
        public int damage = 5;


        public float currentDamage;

        public class EnemyState : CustomEnum
        {

            public EnemyState(string val) : base(val)
            {
                Value = val;
            }
        }

        public static EnemyState Idle = new EnemyState("Idle");
        public static EnemyState PrepareStrike = new EnemyState("PrepareStrike");
        public static EnemyState Attack = new EnemyState("Attack");


        public class EnemyAction : CustomEnum
        {
            public EnemyAction(string val) : base(val)
            {
                Value = val;
            }
        }

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


        public override void OnInit()
        {
            base.OnInit();

            //overriden by subclass
            enemyController.enemyStateFSM = new FSM<EnemyState,EnemyAction>();
            enemyController.seenPlayer = null;
            enemyController.seenAlly = null;
            enemyController.lastSeenPlayer = null;
        }

        

        public EnemyState GetState()
        {
            return enemyController.enemyStateFSM.state;
        }

        public void SetState(EnemyState state)
        {
            enemyController.enemyStateFSM.state = state;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}