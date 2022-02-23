using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class BombGlove : Item
    {
        public LevelObjectData bomb;
        public float bombThrowTime;
        public bool canThrowBomb;
        public GameObject sampleBomb;
        private bool set = false;
        void Start()
        {
            canThrowBomb = true;
            sampleBomb.SetActive(false);
        }

        public void ThrowEffect()
        {
            sampleBomb.SetActive(true);
            ((PlayerBaseAnimator)owner.baseAnimator).Throw();
        }
        
        public override void Use()
        {
            if (!set)
            {
                set = true;    
                ((PlayerBaseAnimator)owner.baseAnimator).bombThrowReleaseEvent.AddListener(ThrowRelease);
            }
            base.Use();
            if(canThrowBomb)
            {
                canThrowBomb = false;
                Invoke(ThrowEffect);
            }
        }

        public void ThrowRelease()
        {
            sampleBomb.SetActive(false);
            Invoke(CreateBomb,false,true);
            Invoke("AllowThrow",bombThrowTime);
        }

        public void AllowThrow()
        {
            canThrowBomb = true;
        }
        
        public void CreateBomb()
        {
                Vector3 forward = -owner.transform.forward;
                forward.Normalize();
                forward.y = 1;
                forward.Normalize();
                GameObject arrowObject = LevelManager.currentLevel.AddDynamic(bomb,transform.position + 0.5f*forward, Quaternion.LookRotation(Vector3.forward), new Util.Optional<int>());
                arrowObject.GetComponent<Bomb>().throwDirection = forward;

        }
    }
}