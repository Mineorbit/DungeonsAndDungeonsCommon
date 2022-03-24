using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class DestructibleLevelObject : NetworkLevelObject
    {
        public bool destroyed = false;
        public virtual void Destroy()
        {
            
        }
        
    }
}