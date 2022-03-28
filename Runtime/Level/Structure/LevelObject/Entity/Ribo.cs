using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Ribo : Enemy
    {
        
        public bool circlePlayer = true;

        public new BlogBaseAnimator baseAnimator;

        public Hitbox attackHitbox;

        public Entity attackTarget;

        private float maxTrackTime;

        // FSM is shared between all components of any enemy but is  defined in the enemy monobehaviour

        private readonly float randomWalkTime;
        
        public float strikeDuration;

        public Random rand = new Random();

        public override void DazeEffect(Vector3 hitPosition)
        {
            ((BlogBaseAnimator) baseAnimator).Daze();
            base.DazeEffect(hitPosition);
        }
        
        public override void UndazeEffect()
        {
            base.UndazeEffect();
            ((BlogBaseAnimator) baseAnimator).Undaze();
        }

        public bool randomWalking = false;
        public Vector3 lastRandomTarget;
        void StartRandomWalk()
        {
            randomWalking = true;
            float angle =(float) rand.NextDouble()*360;
            Vector3 randomPoint = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 15f;
            lastRandomTarget = transform.position + randomPoint;
            GetController().GoTo(lastRandomTarget);
        }

        public override void OnInit()
        {
            base.OnInit();
            // override behavior tree
            // create default behavior  tree
            BehaviorTree baseBehaviorTree = new BehaviorTree();
            BehaviorTree.Node root = new BehaviorTree.SequenceNode();
            BehaviorTree.Node playerSeenNode = new BehaviorTree.SelectorNode();
            BehaviorTree.Node playerSeenCheckNode = new BehaviorTree.ActionNode(() =>
            {
                
                //GameConsole.Log("Checking if seeing Player",false,"AI");
                return  (GetController().seenPlayer == null) ? BehaviorTree.Response.Failure : BehaviorTree.Response.Success;
            });


            BehaviorTree.Node atLastSeenNode = new BehaviorTree.ActionNode(() =>
            {
                return ((GetController().lastPositionOfPlayer - transform.position).magnitude < 0.05f) ? BehaviorTree.Response.Failure : BehaviorTree.Response.Success;
            });

            BehaviorTree.Node playerGotoLastSeenNode = new BehaviorTree.ActionNode(() =>
            {
                GetController().GoTo(GetController().lastPositionOfPlayer);
                return BehaviorTree.Response.Success;
            });

            BehaviorTree.Node playerLastSeenNode = new BehaviorTree.SelectorNode();
            playerLastSeenNode.children = new[] {atLastSeenNode, playerGotoLastSeenNode};
            
            BehaviorTree.Node playerEverSeenNode = new BehaviorTree.ActionNode(() =>
            {
                return (GetController().lastSeenPlayer == null)
                    ? BehaviorTree.Response.Success
                    : BehaviorTree.Response.Failure;
            });
            
            BehaviorTree.Node gotoToLastNode = new BehaviorTree.SelectorNode();
            gotoToLastNode.children = new[] {playerEverSeenNode,playerLastSeenNode};

            BehaviorTree.Node playerRandomWalkNode = new BehaviorTree.ActionNode(() =>
            {
                // this should be a subtree
                if (!randomWalking)
                {
                    //GameConsole.Log("Starting Random Walking",false,"AI");
                    StartRandomWalk();
                    return BehaviorTree.Response.Failure;
                } else
                {
                    if ((transform.position - lastRandomTarget).magnitude < 0.05f)
                    {
                        randomWalking = false;
                        return BehaviorTree.Response.Failure;
                    }
                    return BehaviorTree.Response.Running;
                }

            });



            BehaviorTree.Node playerSearchNode = new BehaviorTree.SequenceNode();
            
            playerSearchNode.children = new[] {gotoToLastNode,playerRandomWalkNode};

            playerSeenNode.children = new[] {playerSeenCheckNode, playerSearchNode};
            
            
            
            
            BehaviorTree.Node closeInNode = new BehaviorTree.ActionNode(() =>
            {
                GetController().GoTo(GetController().seenPlayer.transform);
                return BehaviorTree.Response.Success;
            });

            BehaviorTree.Node playerAttackRangeNode = new BehaviorTree.ActionNode(() =>
            {
                
                return  ( ( GetController().seenPlayer.transform.position - transform.position).magnitude < attackDistance) ? BehaviorTree.Response.Failure : BehaviorTree.Response.Success;
            });
            
            BehaviorTree.Node playerStrikeNode = new BehaviorTree.ActionNode(() =>
            {
                
                //GameConsole.Log("Calling for Strike",false,"AI");
                Strike();
                return BehaviorTree.Response.Running;
            });
            
            
            BehaviorTree.Node playerAttackNode = new BehaviorTree.SelectorNode();
            
            playerAttackNode.children = new[] {playerAttackRangeNode, playerStrikeNode};
            
            root.children = new[] {playerSeenNode,closeInNode, playerAttackNode};
            baseBehaviorTree.root = root;

            behaviorTree = baseBehaviorTree;

            
            onHitEvent.AddListener(x => { if(x.GetType().IsInstanceOfType(typeof(Entity)))  Counter((Entity)x); }); 
        }


        public override void setMovementStatus(bool allowedToMove)
        {
            base.setMovementStatus(allowedToMove);
            if(GetController() != null)
                GetController().SetAgent(allowedToMove);
        }

        private Entity lastVisualTrack = null;

        
        public void SetTrackedEffect(Entity t)
        {
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
        


        public override void OnStartRound()
        {
            base.OnStartRound();

            attackHitbox.Attach("Entity");

            attackHitbox.enterEvent.AddListener(x => { TryDamage(x, currentDamage); });

            attackHitbox.Deactivate();
        }


        // This Needs Telegraphing before attack instead
        private bool striking = false;
        
        /*
        public void TryStrike()
        {
            
               // GetController().GoTo(attackTarget.transform);
               // Invoke("Strike",1 + 2 * (float) rand.NextDouble());
               Strike();
            
        }
        */

        public void StrikeEffect()
        {
            baseAnimator.Strike();
        }

        private float hittingDuration = 0.5f;
        
        public void Strike()
        {
            if (!striking)
            {
                striking = true;
                GetController().Stop();
                Invoke(StrikeEffect);
                // Movement status?
                currentDamage = baseDamage* (1 + damageMultiplier* (float) rand.NextDouble());
                attackHitbox.Activate();
                Invoke("FinishHitting",hittingDuration);
                Invoke("FinishStrike",strikeDuration);
            }
        }

        public void FinishHitting()
        {
            attackHitbox.Deactivate();
        }
        
        public void FinishStrike()
        {
            striking = false;
        }

        public override void Kill()
        {
            base.Kill();
            GetController().Stop();
        }
    }
}