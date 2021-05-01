using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkLevelObject : LevelObject
    {
        NetworkHandler h;

        public override void OnInit()
        {
            base.OnInit();
            if (typeof(Player).IsInstanceOfType(this))
            {
                h = gameObject.AddComponent<PlayerNetworkHandler>();
            }
            else
            if (typeof(Entity).IsInstanceOfType(this))
            {
                h = gameObject.AddComponent<EntityNetworkHandler>();
            }
            else
            h =  gameObject.AddComponent<NetworkHandler>();
        }

        public delegate void ParamsAction(params object[] arguments);


        //This marks a message for transport through network

        public void Invoke(ParamsAction a, object[] argument)
        {
            MethodInfo methodInfo = a.Method;
            if(h!=null)h.Marshall(methodInfo, argument);
            a.Invoke(argument);
        }

        public void Invoke(ParamsAction a)
        {
            MethodInfo methodInfo = a.Method;
            if (h != null) h.Marshall(methodInfo);
            a.Invoke();
        }

        public void Invoke(Action a)
        {
            MethodInfo methodInfo = a.Method;
            if (h != null) h.Marshall(methodInfo);
            a.Invoke();
        }

    }
}