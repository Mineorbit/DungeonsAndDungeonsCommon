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
	public Queue<HitFX> hitEffects = new Queue<HitFX>();
        public void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
            }

            instance = this;
            for(int i = 0;i<4;i++)
            {
            	Create();
            }
        }
        
        public void Create()
        {
        HitFX effect = (Instantiate(instance.hitFX ,Vector3.zero,Quaternion.identity) as GameObject).GetComponent<HitFX>();
        effect.Despawn();
        hitEffects.Enqueue(effect);
        }
	
        public static void HitFX(Vector3 position)
        {
        	HitFX effect = instance.hitEffects.Dequeue();
        	effect.Spawn(position);
            	TimerManager.StartTimer(2f, () => { effect.Despawn(); instance.hitEffects.Enqueue(effect);});
        }
    }
}
