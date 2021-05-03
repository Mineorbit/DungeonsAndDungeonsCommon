using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class MainCaller : MonoBehaviour
    {
        public static Queue<Action> todo = new Queue<Action>();

        public static void Do(Action a)
        {
            todo.Enqueue(a);
        }

        void Update()
        {
            if (todo.Count > 0)
            {
                todo.Dequeue().Invoke();
            }
        }
    }
}