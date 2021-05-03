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
        public string Identity;

        public Component observed;

        public static List<Type> loadedTypes = new List<Type>();
        public static Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

        //Fetch Methods
        public void Awake()
        {
            if(!loadedTypes.Contains(observed.GetType()))
            {
                loadedTypes.Add(observed.GetType());
                foreach (MethodInfo info in observed.GetType().GetMethods())
                {
                if (!methods.ContainsKey(info.ToString()))
                    methods.Add(info.ToString(), info);
                }
            }
            NetworkManager.instance.networkHandlers.Add(this);

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