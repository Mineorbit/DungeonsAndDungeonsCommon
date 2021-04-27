using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        public EnemyController enemyController;


        // Enemy Properties


        public bool seeThroughWalls = false;
        public int damage = 5;


        public float currentDamage;

        public class EnemyState : CustomEnum
        {

            public EnemyState(string val, int card) : base(val, card)
            {
                Value = val;
                cardinal = card;
            }
            public static EnemyState Idle = new EnemyState("Idle",1);
            public static EnemyState PrepareStrike = new EnemyState("PrepareStrike",2);
            public static EnemyState Attack = new EnemyState("Attack",3);

        }


        public class EnemyAction : CustomEnum
        {
            public static EnemyAction Engage = new EnemyAction("Engage",1);
            public static EnemyAction Disengage = new EnemyAction("Disengage",2);
            public EnemyAction(string val,int card) : base(val, card)
            {
                Value = val;
                cardinal = card;
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

            enemyController.seenPlayer = null;
            enemyController.seenAlly = null;
            enemyController.lastSeenPlayer = null;
        }

        

        public EnemyState getState()
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