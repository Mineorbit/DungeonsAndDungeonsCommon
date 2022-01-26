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
        void Start()
        {
            canThrowBomb = true;
        }

        public override void Use()
        {
            base.Use();
            if(canThrowBomb)
            {
                canThrowBomb = false;
                Invoke(CreateBomb,false,true);
                Invoke("AllowThrow",bombThrowTime);
            }
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