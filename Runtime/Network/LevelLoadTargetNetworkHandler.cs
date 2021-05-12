using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetNetworkHandler : LevelObjectNetworkHandler
    {
        public new LevelLoadTarget observed;

        public override void SendAction(string actionName, ActionParam argument)
        {
            Debug.Log("Test "+actionName);
            if(actionName == " ")
            {
                
            }
            else
            {
                base.SendAction(actionName,argument);
            }
        }
    }
}