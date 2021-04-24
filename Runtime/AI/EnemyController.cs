
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



        float viewDistance = 15;
        float attackDistance = 4;


        float distToTarget = float.MaxValue;

        Vector3 lastPositionOfPlayer;


        NavMeshAgent navMeshAgent;
        System.Random rand;

        public float currentSpeed;

        public Hitbox attackHitbox;

        public float currentDamage;



        IEnumerator Timer(float strikeTime, Func<object, object> act, object input)
        {
            yield return new WaitForSeconds(strikeTime);
            act(input);
        }


        void Start()
        {
            me = GetComponent<Enemy>();

            rand = new System.Random();

            navMeshAgent = GetComponent<NavMeshAgent>();

            attackHitbox.Attach("Player");

            attackHitbox.enterEvent.AddListener((x)=> { me.TryDamage(x,currentDamage); });

            me.SetState(Enemy.EnemyState.Idle);
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
            return minPlayer;
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
            if (me.GetState() != Enemy.EnemyState.Attack) attackHitbox.Deactivate(); 
            TryStrike();
        }

        public void TryStrike()
        {
            if (strikeTimer == null)
            {
                me.SetState(Enemy.EnemyState.PrepareStrike);
                strikeTimer = Timer((1 + 5 * (float)rand.NextDouble()),Strike, (float) me.damage);
                StartCoroutine(strikeTimer);
            }
        }
        

        object Strike(object damage)
        {
            Debug.Log("Test");

            me.SetState(Enemy.EnemyState.Attack);
            Attack((float)damage);
            return null;
        }

        void Attack(float damage)
        {
            Debug.Log("Attacking");
            currentDamage = damage * (1 + 5 * (float)rand.NextDouble());
            attackHitbox.Activate();

            var attackTimer = Timer(0.025f, FinishStrike, null);
            StartCoroutine(attackTimer);
        }

        public object FinishStrike(object val)
        {
            me.SetState(Enemy.EnemyState.Attack);
            attackHitbox.Deactivate();
            Debug.Log("Finished Strike");
            strikeTimer = null;
            return null;
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