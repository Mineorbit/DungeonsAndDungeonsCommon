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

        public Transform target;
        Vector3 targetInterpolation;

        public UnityEvent attackEvent;
        public UnityEvent endAttackEvent;

        void Start()
        {
            targetInterpolation = transform.parent.forward;
            attackEvent = new UnityEvent();
            endAttackEvent = new UnityEvent();
        }

        void Update()
        {
            if (animator != null)
                animator.SetFloat("Speed", speed);

            Vector3 targetDirection = new Vector3(0,0,0);

            if(target == null)
            {

                targetDirection = transform.parent.forward;
            }else
            {
                targetDirection = target.transform.position - transform.parent.position;
            }

            targetInterpolation = (targetInterpolation + targetDirection) / 2;

            float angle = 180 + (180 / Mathf.PI) * Mathf.Atan2(targetInterpolation.x, targetInterpolation.z);

            transform.eulerAngles = new Vector3(0, angle, 0);
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
