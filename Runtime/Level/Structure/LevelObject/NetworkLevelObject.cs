using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkLevelObject : LevelObject
    {
        public LevelObjectNetworkHandler levelObjectNetworkHandler;

        public static bool enableNetworking = false;

        public override void OnInit()
        {
            base.OnInit();
            if(enableNetworking)
            {
                
                levelObjectNetworkHandler = GetComponent<LevelObjectNetworkHandler>();
                //
                if(levelObjectNetworkHandler == null)
                {
                    levelObjectNetworkHandler = gameObject.AddComponent<LevelObjectNetworkHandler>();
                }
            }
            else
            {
                GetComponent<LevelObjectNetworkHandler>().enabled = false;
            }
        }



        //This marks a message for transport through network
        public void Invoke(Action<Dictionary<string, object>> a, Dictionary<string, object> argument)
        {
            if (levelObjectNetworkHandler != null) levelObjectNetworkHandler.SendAction(a.Method.Name, argument);
            if (this.enabled) a.DynamicInvoke(argument);
        }

        public void Invoke(Action a)
        {
            if (levelObjectNetworkHandler != null) levelObjectNetworkHandler.SendAction(a.Method.Name);
            if (this.enabled) a.DynamicInvoke();
        }

        /*

        public void Invoke(Action a)
        {
            MethodInfo methodInfo = a.Method;
            if (this.enabled) a.Invoke();
            if (h != null) h.Marshall(a);
        }*/

    }
}