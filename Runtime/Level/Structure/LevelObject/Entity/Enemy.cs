using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Enemy : Entity
    {

        public new EnemyController controller;


        public FSM<EnemyState, Enemy.EnemyAction> FSM;

        public float viewDistance = 15;
        public float attackDistance = 4;

        // Enemy Properties

        public bool forgetPlayer = false;

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
            public EnemyAction(string val,int card) : base(val, card)
            {
                Value = val;
                cardinal = card;
            }

            public static EnemyAction Engage = new EnemyAction("Engage", 1);
            public static EnemyAction Disengage = new EnemyAction("Disengage", 2);
            public static EnemyAction Attack = new EnemyAction("Attack", 3);
            
        }

        

        public void TryDamage(GameObject g, float damage)
        {
            Entity c = g.GetComponentInParent<Entity>(includeInactive: true);
            if (c != null)
            {
                Debug.Log(c.gameObject.name + " hit with " + damage);
                c.Hit(this, (int)damage);
            }
        }


        public virtual  void OnEnable()
        {
            controller = GetComponent<EnemyController>();
            UnityEngine.AI.NavMeshAgent n = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (n != null) n.enabled = true;
            controller.enabled = true;
        }
        public virtual void OnDisable()
        {

            controller = GetComponent<EnemyController>();
            UnityEngine.AI.NavMeshAgent n = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (n != null) n.enabled = false;
            controller.enabled = false;
        }


        public virtual void OnInit()
        {
            base.OnInit();

            Debug.Log("TEST2");

            controller.seenPlayer = null;
            controller.seenAlly = null;
            controller.lastSeenPlayer = null;
        }

        

        public EnemyState getState()
        {
            return FSM.state;
        }

        public void SetState(EnemyState state)
        {
            FSM.state = state;
        }
        
    }
}