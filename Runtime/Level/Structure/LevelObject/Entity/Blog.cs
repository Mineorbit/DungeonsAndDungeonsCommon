using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
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
        
        public override void OnInit()
        {
            base.OnInit();
            onHitEvent.AddListener(x => { if(x.GetType().IsInstanceOfType(typeof(Entity)))  Counter((Entity)x); }); 
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
        public void TryStrike()
        {
                GetController().Stop();
               //GetController().GoTo(attackTarget.transform);
                Invoke("Strike",1 + 2 * (float) rand.NextDouble());
            
        }


        public void StrikeEffect()
        {
            baseAnimator.Strike();
        }
        
        private void Strike()
        {
            Invoke(StrikeEffect);
            
            currentDamage = baseDamage * (1 + damageMultiplier * (float) rand.NextDouble());

            attackHitbox.Activate();

            Invoke("FinishStrike" ,strikeDuration);
        }

        public void FinishStrike()
        {
            attackHitbox.Deactivate();
            targetEntity = attackTarget;
        }
        
    }
}