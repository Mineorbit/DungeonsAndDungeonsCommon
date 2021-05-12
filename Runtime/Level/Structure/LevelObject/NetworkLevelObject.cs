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
            //if(levelObjectNetworkHandler == null) 
                //levelObjectNetworkHandler = gameObject.AddComponent<LevelObjectNetworkHandler>();
            

        }

        Queue<Action> todo = new Queue<Action>();

        //This marks a message for transport through network
        public void Invoke<T>(Action<T> a, T argument)
        {
            if (this.enabled) a.DynamicInvoke(argument);
            if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled)
                if( levelObjectNetworkHandler.identified)
                {
                levelObjectNetworkHandler.SendAction(a.Method.Name, LevelObjectNetworkHandler.ActionParam.From(argument));
                }
                else
                {
                    todo.Enqueue(()=> { Invoke(a, argument); });
                }
            
        }

        public void Invoke(Action a)
        {
            if (this.enabled) a.DynamicInvoke();
            if (levelObjectNetworkHandler != null && levelObjectNetworkHandler.enabled)
            if( levelObjectNetworkHandler.identified)
            {
                levelObjectNetworkHandler.SendAction(a.Method.Name);
            }else
            {
                    todo.Enqueue(()=> { Invoke(a); });
            }
        }


        public virtual void FixedUpdate()
        {
            if(todo.Count>0)
            todo.Dequeue().Invoke();
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