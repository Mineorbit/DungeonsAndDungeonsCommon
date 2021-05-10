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


        public override void OnInit()
        {
            base.OnInit();

            levelObjectNetworkHandler = GetComponent<LevelObjectNetworkHandler>();
            //
            if(levelObjectNetworkHandler == null) 
                levelObjectNetworkHandler = gameObject.AddComponent<LevelObjectNetworkHandler>();
            

        }



        //This marks a message for transport through network
        public void Invoke<T>(Action<T> a, T argument)
        {
            if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled) levelObjectNetworkHandler.SendAction(a.Method.Name, LevelObjectNetworkHandler.ActionParam.From(argument));
            if (this.enabled) a.DynamicInvoke(argument);
        }

        public void Invoke(Action a)
        {
            if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled) levelObjectNetworkHandler.SendAction(a.Method.Name);
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