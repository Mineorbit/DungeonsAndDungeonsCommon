using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityBaseAnimator : MonoBehaviour
    {
        public Entity me;
        public Animator animator;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Strike()
        {
            animator.SetTrigger("Strike");
        }
    }
}
