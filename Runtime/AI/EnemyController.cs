using UnityEngine;
using UnityEngine.AI;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemyController : EntityController
    {
        public static float forgetTime = 5f;

        public Enemy me;


        public Player seenPlayer;
        public Player lastSeenPlayer;
        public Enemy seenAlly;
        public Vector3 lastPositionOfPlayer;
        

        //Queue<Vector3> targetPoints = new Queue<Vector3>();

        private Transform currentWalkTarget;


        private float distToTarget = float.MaxValue;




        public NavMeshAgent navMeshAgent;

        private TimerManager.Timer visibilityTimer;

        public override void Start()
        {
            base.Start();

            me = GetComponent<Enemy>();


            navMeshAgent = GetComponent<NavMeshAgent>();

            Stop();

            //SetTrackingAbility(true);
        }

        public override void Update()
        {
            

            currentSpeed = navMeshAgent.velocity.magnitude;

            base.Update();
        }

        public bool isGrounded()
        {
            return (navMeshAgent.enabled && !navMeshAgent.isOnNavMesh) || controller.isGrounded;
        }

        private float speedY = 0;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateLocomotion();
            UpdateVariables();
            if (!isGrounded())
            {
                speedY += gravity * Time.deltaTime;
                controller.Move(Vector3.down*speedY);
            }
            else
            {
                speedY = 0;
            }
        }

        public Vector3 GetDirection()
        {
            return navMeshAgent.velocity;
        }

        public bool targetIsTransform;

        public Vector3 currentWalkTargetPosition;
        public void GoTo(Transform target)
        {
            targetIsTransform = true;
            currentWalkTarget = target;
        }

        public void SetAgent(bool available)
        {
            if(navMeshAgent != null)
                navMeshAgent.enabled = available;

            if (!isGrounded())
                navMeshAgent.enabled = false;
        }
        
        
        public void GoTo(Vector3 t)
        {
            targetIsTransform = false;
            currentWalkTargetPosition = t;
            GameConsole.Log($"Going to {t}");
        }

        public void Stop()
        {
            GameConsole.Log("Stopping");
            currentWalkTarget = transform;
            navMeshAgent.ResetPath();
        }

        public void UpdateLocomotion()
        {
            if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh && me.allowedToMove)
            {
                if(targetIsTransform)
                    currentWalkTargetPosition = currentWalkTarget.position;
                
                GameConsole.Log("Nav Mesh target set");
                navMeshAgent.SetDestination(currentWalkTargetPosition);
                
            }
            if (!me.allowedToMove && navMeshAgent.enabled)
            {
                navMeshAgent.ResetPath();
            }
        }


        public Player CheckVisiblePlayer()
        {
            Player minPlayer = null;
            if(PlayerManager.playerManager != null)
            {
            var minDist = float.MaxValue;
            for (var i = 0; i < 4; i++)
            {
                var p = PlayerManager.playerManager.players[i];
                if (p != null)
                    if (p.IsAlive())
                    {
                        var dir = p.transform.position - transform.position;
                        var dist = dir.magnitude;

                        if (Vector3.Dot(transform.forward, dir) > 0 && dist <= me.viewDistance && dist < minDist)
                        {
                            minPlayer = p;
                            minDist = dist;
                        }
                    }
            }
            }
            var walls = me.seeThroughWalls;
            if (walls ? true : CheckLineOfSight(minPlayer))
                return minPlayer;
            return null;
        }

        public bool CheckLineOfSight(Entity target)
        {
            if (target == null) return false;

            var layerMask = 0;

            layerMask = ~layerMask;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit,
                Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject.GetComponentInParent<Player>()) return true;

                return false;
            }

            return false;
        }

        private void UpdateSeenPlayer()
        {
            var currentViewedPlayer = CheckVisiblePlayer();
            if (currentViewedPlayer == null)
            {
                // start timer for 2 seconds then drop player
                if (lastSeenPlayer != null)
                    if (me.forgetPlayer)
                        Invoke("ForgetPlayer", forgetTime);
            }
            else
            {
                TimerManager.StopTimer(visibilityTimer);

                lastSeenPlayer = currentViewedPlayer;
                lastPositionOfPlayer = currentViewedPlayer.transform.position;
                seenPlayer = currentViewedPlayer;
            }
        }


        private void ForgetPlayer()
        {
            seenPlayer = null;
        }

        private void UpdateVariables()
        {
            UpdateSeenPlayer();
            UpdateDistance();
        }


        private void UpdateDistance()
        {
            seenAlly = CheckClosestAlley();
            if (seenPlayer != null)
            {
                distToTarget = (seenPlayer.transform.position - transform.position).magnitude;
                lastPositionOfPlayer = seenPlayer.transform.position;
            }
            else
            {
                distToTarget = float.MaxValue;
            }
        }


        private Enemy CheckClosestAlley()
        {
            Enemy minEnemy = null;
            return minEnemy;
        }
    }
}