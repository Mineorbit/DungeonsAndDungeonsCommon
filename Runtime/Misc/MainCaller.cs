using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class MainCaller : MonoBehaviour
    {

        public static MainCaller instance;

        public void Awake()
        {
            if(instance != null)
            {
                Destroy(this);
            }
            instance = this;
        }

        public static Queue<Action> todo = new Queue<Action>();

        public static void Do(Action a)
        {
            todo.Enqueue(a);
        }

        public static void startCoroutine(IEnumerator c)
        {
            Debug.Log(instance);
            instance.StartCoroutine(c);
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