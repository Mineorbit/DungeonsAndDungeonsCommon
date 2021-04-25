using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObject : MonoBehaviour
    {

        public string levelObjectDataType;

		bool initialized = false;

        public bool isDynamic = false;


        // Initializes a LevelObject to its original state
        // It must be assured that this also resets a LevelObject to its Initial State in the Level (If Static that is  self explanatory, might cause
        // destruction of some dynamic LevelObjects aswell, if Dynamic may not cause the distruction of static LevelObjects (but may involve other dynamic GameObjects ))
        public virtual void OnInit()
        {
			initialized = true;
        }

        public virtual void OnDeInit()
        {
			initialized = false;
        }

        public virtual void  OnStartRound()
        {

        }

        public virtual void OnEndRound()
        {

        }


        
    }
}
