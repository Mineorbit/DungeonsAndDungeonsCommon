using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
    {

        // FSM is shared between all components of any enemy but is  defined in the enemy monobehaviour

        FSM<EnemyState, Enemy.EnemyAction> me;

        float randomWalkTime = 8f;

        public class BlogState : EnemyState
        {

            public static BlogState Track = new BlogState("Track",5);


            public BlogState(string val, int card) : base(val,  card)
            {
                Value = val;
                cardinal = card;
            }
        }




        public static EnemyAction Striking = new EnemyAction("Striking", 3);

        float distanceToPlayer = 4f;
        float maximumTrackTime = 15f;
        float strikeDistance = 2f;

        public Hitbox attackHitbox;

        System.Random rand;

        TimerManager.Timer randomWalkTimer;

        void RandomWalk()
        {
            enemyController.SetTrackingAbility(true);
            if (!TimerManager.isRunning(randomWalkTimer))
            {
                float randomAngle = UnityEngine.Random.Range(0,Mathf.PI);
                float randomDistance = 6f;
                Vector3 randomGoal = transform.position+randomDistance*(transform.forward * Mathf.Sin(randomAngle) + transform.right * Mathf.Cos(randomAngle));
                enemyController.GoTo(randomGoal);
                randomWalkTimer = TimerManager.StartTimer(randomWalkTime,()=> { });
            }
        }



        public override void OnInit()
        {
            base.OnInit();

            rand = new System.Random();

            SetupBlogFSM();


            
        }

        float eps = 0.5f;

        float t = 0;
        Transform attackTarget;

        void SetupBlogFSM()
        {
            me = new FSM<EnemyState, EnemyAction>();


            me.stateAction.Add(BlogState.Idle,()=> {
                RandomWalk(); 
                if(enemyController.seenPlayer !=  null)
                {
                    me.Move(Enemy.EnemyAction.Engage);
                }
            });

            me.stateAction.Add(BlogState.Track, () => {

                float distanceToTarget = (transform.position - targetEntity.position).magnitude;
                if (targetEntity != null)
                {

                    enemyController.GoTo(targetEntity.position + targetEntity.forward * distanceToPlayer);
                    
                    if(distanceToTarget < strikeDistance)
                    {
                        me.Move(Striking);
                    }
                }
            });


            me.stateAction.Add(BlogState.PrepareStrike, () => {


                if (attackTarget == null )
                {
                    me.Move(Enemy.EnemyAction.Disengage);
                    return;
                }

                Vector3 dir = transform.position - attackTarget.position;
                float distanceToTarget = (dir).magnitude;


                if (distanceToTarget > distanceToPlayer)
                {
                    me.Move(Enemy.EnemyAction.Engage);
                }

                float angle = 180 + (180/Mathf.PI) * Mathf.Atan2(dir.x,dir.z);


                t += Time.deltaTime;
                //Vector3 targetPoint = attackTarget.position + distanceToPlayer * (attackTarget.forward * Mathf.Sin(t) + attackTarget.right * Mathf.Cos(t));
                
                //transform.position = Circle(targetPoint);

                transform.localEulerAngles = new Vector3(0,angle,0);

                if (distanceToTarget > distanceToPlayer - eps )
                {
                    TryStrike();
                }

            });


            me.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Idle,Enemy.EnemyAction.Engage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x)=> { TrackTarget(enemyController.seenPlayer); },Blog.BlogState.Track));
            me.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Track, Enemy.EnemyAction.Disengage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => { StopTrackTarget(); }, Blog.BlogState.Idle));
            me.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.PrepareStrike, Enemy.EnemyAction.Disengage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => { StopTrackTarget(); }, Blog.BlogState.Idle));


            me.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Track, Striking), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => {

                attackTarget = targetEntity.transform;
                StopTrackTarget();
                Debug.Log(attackTarget);
                t = 0; TryStrike(); }, BlogState.PrepareStrike));
            me.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.PrepareStrike, Enemy.EnemyAction.Engage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => {
                
                TrackTarget(enemyController.seenPlayer); }, Blog.BlogState.Track));

            enemyController.enemyStateFSM = me;
            SetState(BlogState.Idle);
        }

        Vector3 Circle(Vector3 targetPoint)
        {
            UnityEngine.AI.NavMeshHit myNavHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPoint, out myNavHit, 100, -1))
            {
                return myNavHit.position;
            }
            return targetPoint;
        }


        Transform targetEntity;

        TimerManager.Timer maximumTrackTimer;

        void TrackTarget(Entity target)
        {
            enemyController.SetTrackingAbility(true, reset:true);
            if (target == null)
            {
                me.Move(Enemy.EnemyAction.Disengage);
            }
            targetEntity = target.transform;

            maximumTrackTimer = TimerManager.StartTimer(15f,()=>
            {
                me.Move(Enemy.EnemyAction.Disengage);
                if (TimerManager.isRunning(maximumTrackTimer))
                {
                    TimerManager.StopTimer(maximumTrackTimer);
                }
            });
        }

        void StopTrackTarget()
        {
            enemyController.SetTrackingAbility(false);
            targetEntity = null;
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
                
                strikeTimer = TimerManager.StartTimer((1 + 5 * (float)rand.NextDouble()),()=> { Strike(10); } );
            }
        }


        void Strike(float damage)
        {

            Attack((float)damage);
        }

        TimerManager.Timer finishStrikeTimer;

        void Attack(float damage)
        {
            //me.SetState(Enemy.EnemyState.Attack);
            currentDamage = damage * (1 + 5 * (float)rand.NextDouble());

            attackHitbox.Activate();

            finishStrikeTimer = TimerManager.StartTimer(0.025f, ()=> { FinishStrike(); });
            
        }

        public void FinishStrike()
        {
            attackHitbox.Deactivate();
            me.SetState(Enemy.EnemyState.PrepareStrike);
            Debug.Log("Finished Strike");
        }

        public void TryDamage(GameObject g)
        {
            Entity c = g.GetComponentInParent<Entity>(includeInactive: true);
            if (c != null)
            {
                Debug.Log(c.gameObject.name+" hit with "+currentDamage);
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
