using System.Collections.Generic;
using NetLevel;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObject : MonoBehaviour
    {
        public int levelObjectDataType;

        // This defines the behaviour of a LevelObject and the damage effects

        public Constants.Color color;
        
        public ElementType elementType;
        
        public bool isDynamic;

        public bool ActivateWhenInactive;

        public List<LevelObjectData.Property> levelObjectProperties;

        // Initializes a LevelObject to its original state
        // It must be assured that this also resets a LevelObject to its Initial State in the Level (If Static that is  self explanatory, might cause
        // destruction of some dynamic LevelObjects aswell, if Dynamic may not cause the distruction of static LevelObjects (but may involve other dynamic GameObjects ))
        public virtual void OnInit()
        {
        }

        public virtual void OnDeInit()
        {
        }

        public virtual void OnStartRound()
        {
        }

        public virtual void OnEndRound()
        {
        }
    }
}
