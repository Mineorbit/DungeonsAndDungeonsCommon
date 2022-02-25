using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class BlogBaseAnimator : EntityBaseAnimator
    {

        public Transform target;

        public UnityEvent attackEvent;
        public UnityEvent endAttackEvent;
        private Vector3 targetInterpolation;
        public BlogAudioController blogAudioController;

        [FormerlySerializedAs("enemyController")] public new EnemyController entityController;
        

        public override void Start()
        {
            base.Start();
            targetInterpolation = transform.parent.forward;
            attackEvent = new UnityEvent();
            endAttackEvent = new UnityEvent();
        }

        public override void Update()
        {
            base.Update();

            var targetDirection = new Vector3(0, 0, 0);

            if (target == null)
                targetDirection = transform.parent.forward;
            else
                targetDirection = target.transform.position - transform.parent.position;

            LookInDirection(targetDirection);
        }


        public void LookInDirection(Vector3 dir)
        {
            targetInterpolation = (targetInterpolation + dir) / 2;

            var angle = 180 / Mathf.PI * Mathf.Atan2(targetInterpolation.x, targetInterpolation.z);

            transform.eulerAngles = new Vector3(0, angle, 0);
        }

        private IEnumerator _strikeArch;
        public float strikeSpeed = 1f;
        public float jumpHeight;
        IEnumerator StrikeArch()
        {
            Vector3 startPosition = transform.localPosition;
            Vector3 dir = transform.up;
            dir.Normalize();
            float t = 0;
            while (t < 2f)
            {
                transform.localPosition = startPosition + (-(1 - t) * (1 - t) + 1f) * dir*jumpHeight;
                t += Time.deltaTime*strikeSpeed;
                yield return new WaitForEndOfFrame();
            }

            transform.localPosition = startPosition;
            _strikeArch = null;
        }

        public override void Strike()
        {
            base.Strike();
            blogAudioController.Strike();
            if(_strikeArch == null)
            {
                _strikeArch = StrikeArch();
                StartCoroutine(_strikeArch);
            }
        }

        public override void Daze()
        {
            base.Daze();
            blogAudioController.Daze();
        }

        public override void Undaze()
        {
            base.Undaze();
            blogAudioController.Undaze();
        }

        public override void Death()
        {
            base.Death();
            blogAudioController.Death();
        }

        public override void Hit()
        {
            base.Hit();
            blogAudioController.Hit();
        }
        /*
        public void Attack()
        {
            animator.SetTrigger("Strike");
            attackEvent.Invoke();
        }
        
        public void EndAttack()
        {
            endAttackEvent.Invoke();
        }
        */
    }
}