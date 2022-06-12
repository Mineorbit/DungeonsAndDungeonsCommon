
using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ElementType", order = 1)]
    public class ElementType : ScriptableObject
    {
        public new string name = "Default";

        public static ElementType defaultElementType;

        public void OnEnable()
        {
            if (defaultElementType == null)
            {
                defaultElementType = (ElementType) Resources.Load("DamageTypes/Default");
            }
        }

        [Serializable]
        public struct DamageMultiplier
        {
            public ElementType hitObject;
            public float multiplier;
        }
        

        public DamageMultiplier[] damageMultipliers = new DamageMultiplier[] { };

        public float GetMultiplier(ElementType type)
        {
            if (defaultElementType == null)
            {
                defaultElementType = (ElementType) Resources.Load("DamageTypes/Default");
            }

            foreach (DamageMultiplier multiplier in damageMultipliers)
            {
                if (type == multiplier.hitObject)
                {
                    return multiplier.multiplier;
                }
            }

            if (this == defaultElementType)
            {
                // if default Multiplier and no match found
                return 1f;
            }

            return defaultElementType.GetMultiplier(type);
        }
    }
}