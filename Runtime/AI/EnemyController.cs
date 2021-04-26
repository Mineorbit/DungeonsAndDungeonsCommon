
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using com.mineorbit.dungeonsanddungeonscommon;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemyController : MonoBehaviour
    {

        public Enemy me;


        public FSM<Enemy.EnemyState, Enemy.EnemyAction> enemyStateFSM;

        public Player seenPlayer;
        public Player lastSeenPlayer;
        public Enemy seenAlly;



        public float viewDistance = 15;
        public float attackDistance = 4;


        float distToTarget = float.MaxValue;



        Vector3 lastPositionOfPlayer;


        NavMeshAgent navMeshAgent;

        public float currentSpeed;




        public static float forgetTime = 5f;





        void Start()
        {
            me = GetComponent<Enemy>();


            navMeshAgent = GetComponent<NavMeshAgent>();

            

            me.SetState(Enemy.Idle);
        }



        void Update()
        {
            currentSpeed = navMeshAgent.velocity.magnitude;
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

                        if (Vector3.Dot(transform.forward, dir) > 0 && dist <= viewDistance && dist < minDist)
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

        bool CheckLineOfSight(Player target)
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
            enemyStateFSM.ExecuteState();
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