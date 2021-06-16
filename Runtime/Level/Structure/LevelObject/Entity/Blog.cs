using System;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
    {
        private static readonly EnemyAction BackToAttack = new EnemyAction("BackToAttack", 6);


        public bool circlePlayer = true;


        public new BlogBaseAnimator baseAnimator;

        public Hitbox attackHitbox;

        public Entity attackTarget;
        private readonly float circleTime = 5;

        private TimerManager.Timer circleTimer;

        private readonly float eps = 1f;

        private TimerManager.Timer finishStrikeTimer;

        private float maxTrackTime = 10f;

        private TimerManager.Timer maximumTrackTimer;

        private Random rand;

        // FSM is shared between all components of any enemy but is  defined in the enemy monobehaviour


        private readonly float randomWalkTime = 8f;

        private TimerManager.Timer randomWalkTimer;


        public TimerManager.Timer strikeTimer;

        private float t;


        private Entity targetEntity;
        
        
        public float damageMultiplier = 2;
        public float baseDamage = 10;

        public float strikeDuration = 0.025f;
        


        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }


        public void OnDestroy()
        {
            TimerManager.StopTimer(strikeTimer);
            TimerManager.StopTimer(finishStrikeTimer);
            TimerManager.StopTimer(circleTimer);
            TimerManager.StopTimer(randomWalkTimer);
            TimerManager.StopTimer(maximumTrackTimer);
        }

        private void RandomWalk()
        {
            controller.SetTrackingAbility(true);
            if (!TimerManager.isRunning(randomWalkTimer))
            {
                var randomAngle = UnityEngine.Random.Range(0, Mathf.PI);
                var randomDistance = 6f;
                var randomGoal = transform.position + randomDistance * (transform.forward * Mathf.Sin(randomAngle) +
                                                                        transform.right * Mathf.Cos(randomAngle));
                controller.GoTo(randomGoal);
                randomWalkTimer = TimerManager.StartTimer(randomWalkTime, () => { });
            }
        }


        public override void OnInit()
        {
            base.OnInit();


            onHitEvent.AddListener(x => { Counter(x); });
            rand = new Random();
            viewDistance = 5;

            SetupBlogFSM();


            FSM.SetState(EnemyState.Idle);
        }

        public override void setMovementStatus(bool allowedToMove)
        {
            base.setMovementStatus(allowedToMove);
            Debug.Log("Stopping");
            controller.SetTrackingAbility(allowedToMove);
        }

        private void Counter(Entity attacker)
        {
            targetEntity = attacker;
            FSM.Move(EnemyAction.Attack);
        }


        private Entity lastVisualTrack = null;

        
        public void SetTrackedEffect(Entity t)
        {
            Debug.Log("Changing Tracked to: "+t);
            if (t == null)
            {
                baseAnimator.target = null;
                lastVisualTrack = null;
            }else
            {
                baseAnimator.target = t.transform;
                lastVisualTrack = t;
            }
        }
        public void DropTrackedEffect()
        {
            SetTrackedEffect(null);
        }
        
        public void TrackingEffect(Entity target)
        {
            if (lastVisualTrack != target)
                Invoke(SetTrackedEffect, target);
        }

        
        
        public void TrackingEffect()
        {
            if(lastVisualTrack != null)
                Invoke(DropTrackedEffect);
        }
        
        
        private void SetupBlogFSM()
        {
            Debug.Log("Setup FSM");
            FSM = new FSM<EnemyState, EnemyAction>();


            FSM.stateAction.Add(EnemyState.Idle, () =>
            {
                RandomWalk();
                TrackingEffect();
                if (controller.seenPlayer != null) FSM.Move(EnemyAction.Engage);
            });

            FSM.stateAction.Add(BlogState.Track, () =>
            {
                if (controller.seenPlayer == null)
                {
                    FSM.Move(EnemyAction.Disengage);
                    return;
                }

                var distanceToTarget = (transform.position - targetEntity.transform.position).magnitude;
                if (targetEntity != null)
                {
                    controller.GoTo(targetEntity.transform.position + targetEntity.transform.forward * attackDistance);


                    TrackingEffect();

                    if (distanceToTarget < attackDistance + eps) FSM.Move(EnemyAction.Attack);
                }
            });


            FSM.stateAction.Add(EnemyState.Attack, () =>
            {
                if (attackTarget == null || controller.seenPlayer == null)
                {
                    FSM.Move(EnemyAction.Disengage);
                    return;
                }


                var dir = transform.position - attackTarget.transform.position;
                var distanceToTarget = dir.magnitude;

                if (!controller.CheckLineOfSight(attackTarget)) FSM.Move(EnemyAction.Engage);


                TrackingEffect(attackTarget);

                if (TimerManager.isRunning(circleTimer))
                {
                    if (circlePlayer)
                    {
                        t += Time.deltaTime;
                        var targetPoint = attackTarget.transform.position + attackDistance *
                            (attackTarget.transform.forward * Mathf.Sin(t) +
                             attackTarget.transform.right * Mathf.Cos(t));

                        controller.GoTo(Circle(targetPoint));
                    }
                }
                else
                {
                    Debug.Log("Restarting Timer");
                    circleTimer = TimerManager.StartTimer(circleTime, () => { TryStrike(); });
                }
            });


            FSM.stateAction.Add(BlogState.Strike, () =>
            {
                var dir = transform.position - attackTarget.transform.position;
                TrackingEffect(attackTarget);
                controller.GoTo(attackTarget.transform.position);
            });


            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(EnemyState.Idle, EnemyAction.Engage),
                new Tuple<Action<EnemyAction>, EnemyState>(x =>
                {
                    forgetPlayer = true;
                    TrackTarget(controller.seenPlayer);
                }, BlogState.Track));
            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(BlogState.Track, EnemyAction.Disengage),
                new Tuple<Action<EnemyAction>, EnemyState>(x => { StopTrackTarget(); }, EnemyState.Idle));
            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(EnemyState.Attack, EnemyAction.Disengage),
                new Tuple<Action<EnemyAction>, EnemyState>(x => { StopTrackTarget(); }, EnemyState.Idle));

            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(EnemyState.Idle, EnemyAction.Attack),
                new Tuple<Action<EnemyAction>, EnemyState>(x =>
                {
                    forgetPlayer = false;
                    attackTarget = targetEntity;
                    StopTrackTarget(false);
                    t = 0;
                    TryStrike();
                }, EnemyState.Attack));

            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(BlogState.Track, EnemyAction.Attack),
                new Tuple<Action<EnemyAction>, EnemyState>(x =>
                {
                    forgetPlayer = false;
                    attackTarget = targetEntity;
                    StopTrackTarget(false);
                    t = 0;
                    TryStrike();
                }, EnemyState.Attack));


            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(BlogState.Strike, BackToAttack),
                new Tuple<Action<EnemyAction>, EnemyState>(x =>
                {
                    forgetPlayer = false;
                    attackTarget = targetEntity;
                    StopTrackTarget(false);
                    t = 0;
                    TryStrike();
                }, EnemyState.Attack));

            FSM.transitions.Add(new Tuple<EnemyState, EnemyAction>(EnemyState.Attack, EnemyAction.Engage),
                new Tuple<Action<EnemyAction>, EnemyState>(x =>
                {
                    forgetPlayer = true;
                    TrackTarget(controller.seenPlayer);
                }, BlogState.Track));

            SetState(EnemyState.Idle);
        }

        private Vector3 Circle(Vector3 targetPoint)
        {
            NavMeshHit myNavHit;
            if (NavMesh.SamplePosition(targetPoint, out myNavHit, 100, -1)) return myNavHit.position;
            return targetPoint;
        }

        public void TrackTarget(Entity target)
        {
            controller.SetTrackingAbility(true, true);
            if (target == null) FSM.Move(EnemyAction.Disengage);
            targetEntity = target;

            maximumTrackTimer = TimerManager.StartTimer(maxTrackTime, () =>
            {
                FSM.Move(EnemyAction.Disengage);
                if (TimerManager.isRunning(maximumTrackTimer)) TimerManager.StopTimer(maximumTrackTimer);
            });
        }

        private void StopTrackTarget(bool disableTracking = true)
        {
            controller.SetTrackingAbility(!disableTracking);
            targetEntity = null;
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            health = maxHealth;


            attackHitbox.Attach("Player");

            attackHitbox.enterEvent.AddListener(x => { TryDamage(x, currentDamage); });

            attackHitbox.Deactivate();
        }


        public void TryStrike()
        {
            if (!TimerManager.isRunning(strikeTimer) && !TimerManager.isRunning(finishStrikeTimer))
            {
                StopTrackTarget(false);
                controller.GoTo(attackTarget.transform.position);
                Debug.Log("Blog trying to Strike");
                FSM.SetState(BlogState.Strike);
                strikeTimer = TimerManager.StartTimer(1 + 5 * (float) rand.NextDouble(), () => { Strike(); });
            }
        }


        public void StrikeEffect()
        {
            baseAnimator.Strike();
        }
        
        private void Strike()
        {
            Invoke(StrikeEffect);
            Attack();
        }

        
        private void Attack()
        {
            currentDamage = baseDamage * (1 + damageMultiplier * (float) rand.NextDouble());

            attackHitbox.Activate();


            finishStrikeTimer = TimerManager.StartTimer(strikeDuration, () => { FinishStrike(); });
        }

        public void FinishStrike()
        {
            attackHitbox.Deactivate();
            Debug.Log("Finished Strike");
            targetEntity = attackTarget;
            FSM.Move(BackToAttack);
        }

        public class BlogState : EnemyState
        {
            public static BlogState Track = new BlogState("Track", 5);
            public static BlogState Strike = new BlogState("Strike", 6);


            public BlogState(string val, int card) : base(val, card)
            {
                Value = val;
                cardinal = card;
            }
        }
    }
}