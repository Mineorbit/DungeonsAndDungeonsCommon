using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {
        public new LevelObject observed;
        public virtual void Awake()
        {
            observed = GetComponent<LevelObject>();
            base.Awake();
        }

    }
}
