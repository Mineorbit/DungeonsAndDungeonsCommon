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

        

        //This marks a message for transport through network

        public void Invoke(Action<Dictionary<string,object>> a, Dictionary<string, object> argument)
        {
            if(h!=null) h.Marshall(a.Method.Name, argument);
            if (this.enabled) a.DynamicInvoke(argument);
        }


        public void Invoke(Action a)
        {
            if (h != null) h.Marshall(a.Method.Name);
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