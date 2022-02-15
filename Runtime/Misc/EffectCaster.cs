using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EffectCaster : MonoBehaviour
    {
        public static Dictionary<string, EffectCaster> dictionary = new Dictionary<string, EffectCaster>();

        public string effectName;
        [FormerlySerializedAs("hitFX")] public UnityEngine.Object effectTemplate;
	    public Queue<FX> effects = new Queue<FX>();
        public void Awake()
        {
            dictionary.Add(effectName,this);
            for(int i = 0;i<4;i++)
            {
            	Create();
            }
        }

        public void Create()
        {
        FX effect = (Instantiate(effectTemplate ,new Vector3(0,-8,0),Quaternion.identity) as GameObject).GetComponent<FX>();
        effect.Setup(effects);
        effects.Enqueue(effect);
        }
	
        public static void FX(string type,Vector3 position)
        {
            EffectCaster caster = dictionary[type];
            if(caster != null)
            {
        	    FX effect = caster.effects.Dequeue();
        	    effect.Spawn(position);
            }
            else
            {
                GameConsole.Log($"There is no caster of type {type}");
            }
        }
    }
}
