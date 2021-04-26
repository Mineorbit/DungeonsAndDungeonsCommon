using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
    {

        // FSM is shared between all components of any enemy but is  defined in the enemy monobehaviour

        FSM<Enemy.EnemyState, Enemy.EnemyAction> me;


        public Hitbox attackHitbox;

        System.Random rand;

        public override void OnInit()
        {
            base.OnInit();


            rand = new System.Random();

            
        }
        bool go = false;
        public override void OnStartRound()
        {
            base.OnStartRound();
            go = true;
            health = 100; 
            
            attackHitbox.Attach("Player");

            attackHitbox.enterEvent.AddListener((x) => { TryDamage(x); });

            attackHitbox.Deactivate();
        }


        public override void OnEnable()
        {
			base.OnEnable();
        }

        public override void OnDisable()
        {
			base.OnDisable();
        }



        public TimerManager.Timer strikeTimer;


        public void TryStrike()
        {
            if (!TimerManager.isRunning(strikeTimer))
            {
                Debug.Log("Blog trying to Strike");
                //me.SetState(Enemy.PrepareStrike);
                
                strikeTimer = TimerManager.StartTimer((1 + 5 * (float)rand.NextDouble()),()=> { Strike(10); } );
            }
        }


        void Strike(float damage)
        {

            //me.SetState(Enemy.Attack);
            Attack((float)damage);
        }

        TimerManager.Timer finishStrikeTimer;

        void Attack(float damage)
        {
            Debug.Log("Attacking");
            currentDamage = damage * (1 + 5 * (float)rand.NextDouble());

            attackHitbox.Activate();

            finishStrikeTimer = TimerManager.StartTimer(0.025f, ()=> { FinishStrike(); });
            
        }

        public void FinishStrike()
        {
            //me.SetState(Enemy.Attack);
            attackHitbox.Deactivate();
            Debug.Log("Finished Strike");
        }

        public void TryDamage(GameObject g)
        {
            Entity c = g.GetComponentInParent<Entity>(includeInactive: true);
            if (c != null)
            {
                Debug.Log(c+" hit with"+currentDamage);
                c.Hit((int)currentDamage);
            }
        }

        public void OnDestroy()
        {
            TimerManager.StopTimer(strikeTimer);
            TimerManager.StopTimer(finishStrikeTimer);

        }

        // Update is called once per frame
        void Update()
        { if(go)
            TryStrike();
        }
    }
}
