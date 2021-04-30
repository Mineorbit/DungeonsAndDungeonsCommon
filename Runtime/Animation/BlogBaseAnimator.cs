using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class BlogBaseAnimator : EntityBaseAnimator
    {
        Animator animator;
        public float speed;

        public Vector3 target;
        Vector3 targetInterpolation;

        public UnityEvent attackEvent;
        public UnityEvent endAttackEvent;

        void Start()
        {
            target = transform.forward;
            targetInterpolation = target;
            attackEvent = new UnityEvent();
            endAttackEvent = new UnityEvent();
        }

        void Update()
        {
            if (animator != null)
                animator.SetFloat("Speed", speed);

            targetInterpolation = (targetInterpolation + target) / 2;

            float angle = 180 + (180 / Mathf.PI) * Mathf.Atan2(targetInterpolation.x, targetInterpolation.z);

            transform.eulerAngles = new Vector3(0, angle, 0);
        }

        public void Attack()
        {
            animator.SetTrigger("Strike");
            attackEvent.Invoke();
        }
        public void Hit()
        {
            animator.SetTrigger("Hit");
        }

        public void EndAttack()
        {
            endAttackEvent.Invoke();
        }
    }
}
