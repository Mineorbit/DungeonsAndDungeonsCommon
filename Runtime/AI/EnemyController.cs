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

        // STOLEN
        //https://stackoverflow.com/questions/45416515/check-if-disabled-navmesh-agent-player-is-on-navmesh

        float onMeshThreshold = 3;

        public bool IsAgentOnNavMesh(GameObject agentObject)
        {
            Vector3 agentPosition = agentObject.transform.position;
            NavMeshHit hit;

            // Check for nearest point on navmesh to agent, within onMeshThreshold
            if (NavMesh.SamplePosition(agentPosition, out hit, onMeshThreshold, NavMesh.AllAreas))
            {
                // Check if the positions are vertically aligned
                if (Mathf.Approximately(agentPosition.x, hit.position.x)
                    && Mathf.Approximately(agentPosition.z, hit.position.z))
                {
                    // Lastly, check if object is below navmesh
                    return agentPosition.y >= hit.position.y;
                }
            }

            return false;
        }
        
        private float speedY = 0;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateLocomotion();
            UpdateVariables();
            isGrounded = me.isGrounded;
            if (!isGrounded)
            {
                speedY += gravity * Time.deltaTime;
                controller.Move(Vector3.down*speedY);
            }
            else
            {
                speedY = 0;
            }

            navMeshAgent.enabled = isGrounded && IsAgentOnNavMesh(this.gameObject) && navAllowed;
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

        private bool navAllowed = true;

        public void SetAgent(bool available)
        {
            navAllowed = available;
        }
        
        
        public void GoTo(Vector3 t)
        {
            targetIsTransform = false;
            currentWalkTargetPosition = t;
        }

        public void Stop()
        {
            currentWalkTarget = transform;
            navMeshAgent.ResetPath();
        }

        public void UpdateLocomotion()
        {
            if (me.allowedToMove)
            {
                if(targetIsTransform)
                    currentWalkTargetPosition = currentWalkTarget.position;
                if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.SetDestination(currentWalkTargetPosition);
                }
                else
                {
                    Vector3 dir = currentWalkTargetPosition - transform.position;
                    dir.Normalize();
                    float speed = 2f;
                    controller.Move(dir*speed*Time.deltaTime);
                }
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