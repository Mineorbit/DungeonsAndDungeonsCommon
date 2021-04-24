
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
        public Player lastSeenPlayer;
        public Enemy seenAlly;



        public float viewDistance = 15;
        public float attackDistance = 4;


        float distToTarget = float.MaxValue;



        Vector3 lastPositionOfPlayer;


        NavMeshAgent navMeshAgent;
        System.Random rand;

        public float currentSpeed;

        public Hitbox attackHitbox;

        public float currentDamage;


        public float forgetTime = 5f;


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

            attackHitbox.Deactivate();

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


            if(CheckLineOfSight(minPlayer))
            {            
                return minPlayer;
            }else
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

        IEnumerator visibilityTimer;
        void UpdateSeenPlayer()
        {
            Player currentViewedPlayer = CheckVisiblePlayer();
            if(currentViewedPlayer == null)
            {
                // start timer for 2 seconds then drop player
                if(visibilityTimer == null && lastSeenPlayer != null)
                {
                    visibilityTimer = Timer(forgetTime,ForgetPlayer,null);
                    StartCoroutine(visibilityTimer);
                }
            }
            else
            {
                if(visibilityTimer != null)
                { 
                    StopCoroutine(visibilityTimer);
                    visibilityTimer = null;
                }
                lastSeenPlayer = currentViewedPlayer;
                seenPlayer = currentViewedPlayer;
            }
        }


        object ForgetPlayer(object input)
        {
            seenPlayer = null;
            return null;
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

        public IEnumerator strikeTimer;

        void UpdateState()
        {

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