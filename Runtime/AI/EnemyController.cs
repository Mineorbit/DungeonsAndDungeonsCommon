
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemyController : MonoBehaviour
    {
        Enemy me;
        public Player seenPlayer;
        public Enemy seenAlly;
        public int health;
        public int damage;


        float viewDistance = 15;
        float attackDistance = 4;


        float distToTarget = float.MaxValue;

        Vector3 lastPositionOfPlayer;


        NavMeshAgent navMeshAgent;
        System.Random rand;

        public float currentSpeed;

        void Start()
        {
            me = GetComponent<Enemy>();

            rand = new System.Random();
            health = 100;

            damage = 2;

            navMeshAgent = GetComponent<NavMeshAgent>();



            me.SetState(Enemy.EnemyState.Idle);
        }

        void Update()
        {
            currentSpeed = navMeshAgent.velocity.magnitude;
        }

        void FixedUpdate()
        {
            UpdateDistance();
            UpdateState();

            UpdateTarget();

        }

        void UpdateTarget()
        {
            var enemyState = me.GetState();
            if (enemyState == Enemy.EnemyState.Track)
            {
                navMeshAgent.SetDestination(lastPositionOfPlayer);
            }
            else if (enemyState == Enemy.EnemyState.Attack)
            {
                navMeshAgent.SetDestination(transform.position);
                transform.LookAt(lastPositionOfPlayer);
            }
        }

        void UpdateDistance()
        {
            seenAlly = CheckClosestAlley();
            seenPlayer = CheckVisiblePlayer();
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

        public IEnumerator strikeTimer;

        void UpdateState()
        {
            var enemyState = me.GetState();
            if (enemyState != Enemy.EnemyState.Strike || enemyState != Enemy.EnemyState.PrepareStrike)
            {

                if (attackDistance <= distToTarget && distToTarget <= viewDistance)
                {
                    if (seenPlayer != null)
                    {
                        me.SetState(Enemy.EnemyState.Track);
                        if (strikeTimer != null)
                        {
                            StopCoroutine(strikeTimer);
                            strikeTimer = null;
                        }
                    }
                }
                else if (distToTarget <= attackDistance)
                {
                    if (seenPlayer != null)
                    {
                        me.SetState(Enemy.EnemyState.Attack);
                        if (seenAlly == null)
                        {
                            TryStrike();
                        }
                        else
                        if (seenAlly.GetState() != Enemy.EnemyState.Strike || seenAlly.GetState() != Enemy.EnemyState.PrepareStrike)
                        {
                            TryStrike();
                        }
                    }
                }
                else
                {
                    me.SetState(Enemy.EnemyState.Idle);
                    if (strikeTimer != null)
                        StopCoroutine(strikeTimer);
                }

            }
        }

        public void TryStrike()
        {
            if (strikeTimer == null)
            {
                me.SetState(Enemy.EnemyState.PrepareStrike);
                strikeTimer = StrikeTimer((1 + 5 * (float)rand.NextDouble()));
                StartCoroutine(strikeTimer);
            }
        }
        IEnumerator StrikeTimer(float strikeTime)
        {
            yield return new WaitForSeconds(strikeTime);
            Strike();
        }

        void Strike()
        {
            if (seenAlly != null)
            {
                if (seenAlly.GetState() != Enemy.EnemyState.Strike || seenAlly.GetState() != Enemy.EnemyState.PrepareStrike)
                {
                    strikeTimer = null;
                    me.SetState(Enemy.EnemyState.Strike);
                }
                else
                {
                    me.SetState(Enemy.EnemyState.Attack);
                }
            }
            else
            {
                strikeTimer = null;
                me.SetState(Enemy.EnemyState.Strike);
            }
        }

        public void FinishStrike()
        {
            me.SetState(Enemy.EnemyState.Attack);
        }



        bool hitCooldown = false;

        IEnumerator HitTimer(float time)
        {
            yield return new WaitForSeconds(time);
            hitCooldown = false;
        }


        void StartHitCooldown()
        {
            hitCooldown = true;
            StartCoroutine(HitTimer(0.5f));
        }


        public virtual void Hit(int damage)
        {
            if (!hitCooldown)
            {

                StartHitCooldown();
                health = health - damage;
                if (health == 0)
                {
                    Kill();
                }
                //FreezeFramer.freeze(0.0075f);
            }

        }

        public void Kill()
        {
            me.SetState(Enemy.EnemyState.Dead);
            //eventually call
            LevelManager.currentLevel.RemoveDynamic(me);
        }

        Enemy CheckClosestAlley()
        {
            float minDist = float.MaxValue;
            Enemy minEnemy = null;
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
            return minEnemy;
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
            return minPlayer;
        }
    }
}