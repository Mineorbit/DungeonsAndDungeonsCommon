using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class FSM<T, U>
    where U : CustomEnum
    where T : CustomEnum
    {


        public string name;

        public T state;
        public Dictionary<Tuple<T, U>, Tuple<Action<U>, T>> transitions;
        public Dictionary<T, Action> stateAction;

        public FSM()
        {
            transitions = new Dictionary<Tuple<T, U>, Tuple<Action<U>, T>>();
            stateAction = new Dictionary<T, Action>();
        }

        public void SetState(T s)
        {
            state = s;
        }

        public void ExecuteState()
        {
            if(stateAction.ContainsKey(state))
            stateAction[state]();
        }

        public void Move(U inputValue)
        {
            T oldState = state;
            Tuple<Action<U>, T> value;
            if (transitions.TryGetValue(new Tuple<T, U>(state, inputValue), out value))
            {
                state = value.Item2;

                UnityEngine.Debug.Log($"[{name}] {oldState} -> {inputValue} -> {state}");
                value.Item1(inputValue);

            }
            else
            {
                UnityEngine.Debug.Log($"[{name}] In State {state} existiert kein Übergang für {inputValue}");
            }
        }
    }
}
