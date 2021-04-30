using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class BaseAnimator : MonoBehaviour
    {
        public static Dictionary<string, MethodInfo> animations = new Dictionary<string, MethodInfo>();
        void Awake()
        {
            Debug.Log("Initializing"+ this.GetType());
            foreach(MethodInfo info in this.GetType().GetMethods())
            {
                Debug.Log(info);
                if(!animations.ContainsKey(info.ToString()))
                animations.Add(info.ToString(),info);
            }

        }

        public void InvokeAnimation(string methodName)
        {
            MethodInfo m;
            if(animations.TryGetValue(name, out m))
            {
                m.Invoke(this, null);
            }
        }
    }
}