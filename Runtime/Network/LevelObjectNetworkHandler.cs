using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using Game;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {
        public new LevelObject observed;

        List<string> availableActions = new List<string>();
        public virtual void Awake()
        {
            observed = GetComponent<LevelObject>();
            base.Awake();
        }


        public void ProcessAction(Packet p)
        {

        }

        public void SendAction(string actionName)
        {
            if(availableActions.Contains(actionName))
            {

            }
        }

        // Not yet needed
        /*
        public void SendAction(string actionName, object param)
        {
            if (availableActions.Contains(actionName))
            {

            }
        }
        */
    }
}
