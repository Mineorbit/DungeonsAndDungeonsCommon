using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
    {

        // FSM is shared between all components of any enemy but is  defined in the enemy monobehaviour


        float randomWalkTime = 8f;

        public class BlogState : EnemyState
        {

            public static BlogState Track = new BlogState("Track",5);
            public static BlogState Strike = new BlogState("Strike", 6);


            public BlogState(string val, int card) : base(val,  card)
            {
                Value = val;
                cardinal = card;
            }
        }


        static EnemyAction BackToAttack = new EnemyAction("BackToAttack",6);


        public bool circlePlayer = true;



        public new BlogBaseAnimator baseAnimator;

        public Hitbox attackHitbox;

        System.Random rand;

        TimerManager.Timer randomWalkTimer;

        void RandomWalk()
        {
            controller.SetTrackingAbility(true);
            if (!TimerManager.isRunning(randomWalkTimer))
            {
                float randomAngle = UnityEngine.Random.Range(0,Mathf.PI);
                float randomDistance = 6f;
                Vector3 randomGoal = transform.position+randomDistance*(transform.forward * Mathf.Sin(randomAngle) + transform.right * Mathf.Cos(randomAngle));
                controller.GoTo(randomGoal);
                randomWalkTimer = TimerManager.StartTimer(randomWalkTime,()=> { });
            }
        }



        public override void OnInit()
        {
            base.OnInit();

            Debug.Log("TEST1");

            onAttackEvent.AddListener((x)=> { Counter(x); });
            rand = new System.Random();
            viewDistance = 5;

            SetupBlogFSM();


            FSM.SetState(Enemy.EnemyState.Idle);

        }

        public override void setMovementStatus(bool allowedToMove)
        {
            base.setMovementStatus(allowedToMove);
            Debug.Log("Stoppinng");
            controller.SetTrackingAbility(allowedToMove);
        }

        void Counter(Entity attacker)
        {
            targetEntity = attacker;
            FSM.Move(Enemy.EnemyAction.Attack);
        }

        float eps = 1f;
        
        float t = 0;

        public Entity attackTarget;

        TimerManager.Timer circleTimer;
        float circleTime = 5;

        void SetupBlogFSM()
        {
            Debug.Log("Setup FSM");
            FSM = new FSM<EnemyState, EnemyAction>();


            FSM.stateAction.Add(BlogState.Idle,()=> {
                RandomWalk();
                baseAnimator.target = -controller.GetDirection();

                if (controller.seenPlayer !=  null)
                {
                    FSM.Move(Enemy.EnemyAction.Engage);
                }
            });

            FSM.stateAction.Add(BlogState.Track, () => {
                float distanceToTarget = (transform.position - targetEntity.transform.position).magnitude;
                if (targetEntity != null)
                {

                    controller.GoTo(targetEntity.transform.position + targetEntity.transform.forward * attackDistance);


                    baseAnimator.target = -controller.GetDirection();

                    if (distanceToTarget < attackDistance+eps)
                    {
                        FSM.Move(Enemy.EnemyAction.Attack);
                    }
                }
            });


            FSM.stateAction.Add(BlogState.Attack, () => {


                if (attackTarget == null )
                {
                    FSM.Move(Enemy.EnemyAction.Disengage);
                    return;
                }


                Vector3 dir = transform.position - attackTarget.transform.position;
                float distanceToTarget = (dir).magnitude;

                if (!controller.CheckLineOfSight(attackTarget))
                {
                    FSM.Move(Enemy.EnemyAction.Engage);
                }


                baseAnimator.target = dir;

                if (TimerManager.isRunning(circleTimer))
                {
                    if (circlePlayer)
                    {

                        t += Time.deltaTime;
                        Vector3 targetPoint = attackTarget.transform.position + attackDistance * (attackTarget.transform.forward * Mathf.Sin(t) + attackTarget.transform.right * Mathf.Cos(t));

                        controller.GoTo(Circle(targetPoint));


                        
                    }
                }else
                {
                    Debug.Log("Restarting Timer");
                    circleTimer = TimerManager.StartTimer(circleTime, () => { TryStrike(); });
                }

            });


            FSM.stateAction.Add(BlogState.Strike, () => {

                Vector3 dir = transform.position - attackTarget.transform.position;
                baseAnimator.target = dir;
                controller.GoTo(attackTarget.transform.position);

            });



            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Idle,Enemy.EnemyAction.Engage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x)=> { forgetPlayer = true; TrackTarget(controller.seenPlayer); },Blog.BlogState.Track));
            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Track, Enemy.EnemyAction.Disengage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => { StopTrackTarget(); }, Blog.BlogState.Idle));
            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Attack, Enemy.EnemyAction.Disengage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => { StopTrackTarget(); }, Blog.BlogState.Idle));

            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Idle, Enemy.EnemyAction.Attack), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => {

                forgetPlayer = false;
                attackTarget = targetEntity;
                StopTrackTarget(disableTracking: false);
                Debug.Log("Attacking " + attackTarget.gameObject.name);
                t = 0; TryStrike();
            }, BlogState.Attack));

            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Track, Enemy.EnemyAction.Attack), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => {

                forgetPlayer = false;
                attackTarget = targetEntity;
                StopTrackTarget(disableTracking: false);

                Debug.Log("Attacking "+attackTarget.gameObject.name);
                t = 0; TryStrike(); }, BlogState.Attack));


            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Strike, BackToAttack), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => {

                forgetPlayer = false;
                attackTarget = targetEntity;
                StopTrackTarget(disableTracking: false);

                Debug.Log("Attacking " + attackTarget.gameObject.name);
                t = 0; TryStrike();
            }, BlogState.Attack));

            FSM.transitions.Add(new Tuple<Enemy.EnemyState, Enemy.EnemyAction>(BlogState.Attack, Enemy.EnemyAction.Engage), new Tuple<Action<Enemy.EnemyAction>, Enemy.EnemyState>((x) => {
            
                    forgetPlayer = true;
                TrackTarget(controller.seenPlayer); }, Blog.BlogState.Track));

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


         Entity targetEntity;

        TimerManager.Timer maximumTrackTimer;

        void TrackTarget(Entity target)
        {
            controller.SetTrackingAbility(true, reset:true);
            if (target == null)
            {
                FSM.Move(Enemy.EnemyAction.Disengage);
            }
            targetEntity = target;

            maximumTrackTimer = TimerManager.StartTimer(15f,()=>
            {
                FSM.Move(Enemy.EnemyAction.Disengage);
                if (TimerManager.isRunning(maximumTrackTimer))
                {
                    TimerManager.StopTimer(maximumTrackTimer);
                }
            });
        }

        void StopTrackTarget(bool disableTracking = true)
        {
            controller.SetTrackingAbility(!disableTracking);
            targetEntity = null;
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            health = 100; 
            

            attackHitbox.Attach("Player");

            attackHitbox.enterEvent.AddListener((x) => { TryDamage(x,currentDamage); });

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
            if (!TimerManager.isRunning(strikeTimer) && !TimerManager.isRunning(finishStrikeTimer))
            {
                StopTrackTarget(disableTracking: false);
                controller.GoTo(attackTarget.transform.position);
                Debug.Log("Blog trying to Strike");
                FSM.SetState(BlogState.Strike);
                strikeTimer = TimerManager.StartTimer((1 + 5 * (float)rand.NextDouble()),()=> { Strike(10); } );
            }
        }


        void Strike(float damage)
        {

            baseAnimator.Strike();
            Attack((float)damage);
        }

        TimerManager.Timer finishStrikeTimer;

        void Attack(float damage)
        {
            currentDamage = damage * (1 + 5 * (float)rand.NextDouble());

            attackHitbox.Activate();


            finishStrikeTimer = TimerManager.StartTimer(0.025f, ()=> { FinishStrike(); });
            
        }

        public void FinishStrike()
        {
            attackHitbox.Deactivate();
            Debug.Log("Finished Strike");
            targetEntity = attackTarget;
            FSM.Move(BackToAttack);
        }

        
        public void OnDestroy()
        {
            TimerManager.StopTimer(strikeTimer);
            TimerManager.StopTimer(finishStrikeTimer);
            TimerManager.StopTimer(circleTimer);
            TimerManager.StopTimer(randomWalkTimer);
            TimerManager.StopTimer(maximumTrackTimer);
        }

    }
}
