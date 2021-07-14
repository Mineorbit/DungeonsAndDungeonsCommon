using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EffectCaster : MonoBehaviour
    {
        public static EffectCaster instance;
        public UnityEngine.Object hitFX;

        public void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
            }

            instance = this;
        }

        public static void HitFX(Vector3 position)
        {
            GameObject effect = Instantiate(instance.hitFX ,position,Quaternion.identity) as GameObject;
            TimerManager.StartTimer(2f, () => { Destroy(effect); });
        }
    }
}