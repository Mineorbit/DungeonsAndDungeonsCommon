using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkHandler : MonoBehaviour
    {

        public static List<Type> loadedTypes = new List<Type>();
        public static Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

        public void Awake()
        {
            LevelObject levelObject = gameObject.GetComponent<LevelObject>();
            if(!loadedTypes.Contains(levelObject.GetType()))
            {
                loadedTypes.Add(levelObject.GetType());
                foreach (MethodInfo info in levelObject.GetType().GetMethods())
                {
                if (!methods.ContainsKey(info.ToString()))
                    methods.Add(info.ToString(), info);
                }
            }


        }


        public void Marshall(MethodInfo methodInfo)
        {
            Debug.Log("WE DID "+methodInfo.Name);
        }


        public void Marshall(MethodInfo methodInfo, object param)
        {
            Debug.Log("WE DID " + methodInfo.Name+" with "+param);
        }

    }
}