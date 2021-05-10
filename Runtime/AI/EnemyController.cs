﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using com.mineorbit.dungeonsanddungeonscommon;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemyController : EntityController
    {

        public Enemy me;


        public Player seenPlayer;
        public Player lastSeenPlayer;
        public Enemy seenAlly;





        float distToTarget = float.MaxValue;



        Vector3 lastPositionOfPlayer;


        NavMeshAgent navMeshAgent;

        public float currentSpeed;




        public static float forgetTime = 5f;



        //Queue<Vector3> targetPoints = new Queue<Vector3>();

        Vector3 currentTarget = new Vector3(0,0,0);
        public override void Start()
        {
            base.Start();

            me = GetComponent<Enemy>();


            navMeshAgent = GetComponent<NavMeshAgent>();

            currentTarget = transform.position;

            SetTrackingAbility(true);
        }


        public void SetTrackingAbility(bool ability, bool reset = false)
        {
            if(navMeshAgent != null)
            { 
            navMeshAgent.isStopped = !ability;
            if(reset) navMeshAgent.ResetPath();
            }
        }


        public Vector3 GetDirection()
        {
            return navMeshAgent.velocity;
        }

        public void GoTo(Vector3 target)
        {
            currentTarget = target;
        }
        /*
        public void GoTo(Vector3 target)
        {
            targetPoints.Enqueue(target);
        }

        TimerManager.Timer walkTimer;


        public void UpdateLocomotion()
        {
            if ((currentTarget - transform.position).magnitude < 0.05f || !TimerManager.isRunning(walkTimer))
            {
                if (targetPoints.Count > 0)
                {
                    currentTarget = targetPoints.Dequeue();
                    navMeshAgent.SetDestination(currentTarget);
                    walkTimer = TimerManager.StartTimer(5f,()=> { });
                }

            }
        }*/

        public void UpdateLocomotion()
        {
            navMeshAgent.SetDestination(currentTarget);
        }

        public override void Update()
        {
            UpdateLocomotion();

            currentSpeed = navMeshAgent.velocity.magnitude;
            base.Update();
        }


        Player CheckVisiblePlayer()
        {
            float minDist = float.MaxValue;
            Player minPlayer = null;
            for (int i = 0; i < 4; i++)
            {
                Player p = PlayerManager.playerManager.players[i];
                if (p != null)
                {
                    if (p.IsAlive())
                    {
                        Vector3 dir = p.transform.position - transform.position;
                        float dist = dir.magnitude;

                        if (Vector3.Dot(transform.forward, dir) > 0 && dist <= me.viewDistance && dist < minDist)
                        {
                            minPlayer = p;
                            minDist = dist;
                        }
                    }
                }
            }

            bool walls = me.seeThroughWalls;
            if (walls ? true : CheckLineOfSight(minPlayer))
            {
                return minPlayer;
            } else
            {
                return null;
            }

        }

        public bool CheckLineOfSight(Entity target)
        {
            if (target == null) return false;

            int layerMask = 0;

            layerMask = ~layerMask;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject.GetComponentInParent<Player>()) return true;

                return false;
            }
            else
            {
                return false;
            }
        }

        TimerManager.Timer visibilityTimer;
        void UpdateSeenPlayer()
        {
            Player currentViewedPlayer = CheckVisiblePlayer();
            if(currentViewedPlayer == null)
            {
                // start timer for 2 seconds then drop player
                if(lastSeenPlayer != null)
                {
                    if(me.forgetPlayer)
                    visibilityTimer = TimerManager.StartTimer(forgetTime, () => { this.ForgetPlayer(); });
                }
            }
            else
            {
                TimerManager.StopTimer(visibilityTimer);

                lastSeenPlayer = currentViewedPlayer;
                seenPlayer = currentViewedPlayer;
            }
        }


        void ForgetPlayer()
        {
            seenPlayer = null;
        }

        void UpdateVariables()
        {
            UpdateSeenPlayer();
            UpdateDistance();
        }


        void FixedUpdate()
        {
            UpdateVariables();
            UpdateState();
        }

        

        void UpdateDistance()
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


        void UpdateState()
        {
            Debug.Log(me);
            me.FSM.ExecuteState();
        }

        




        

        Enemy CheckClosestAlley()
        {
            float minDist = float.MaxValue;
            Enemy minEnemy = null;
            /*
            foreach (GameObject g in Level.GetAllEnemies())
            {
                Enemy e = g.GetComponent<Enemy>();
                if (e != null && e != me && e.GetState() != Enemy.EnemyState.Dead)
                {

                    Vector3 dir = e.transform.position - transform.position;
                    float dist = dir.magnitude;

                    if (dist <= attackDistance && dist < minDist)
                    {
                        minEnemy = e;
                        minDist = dist;
                    }
                }
            }
            */
            return minEnemy;
        }

        
    }
}