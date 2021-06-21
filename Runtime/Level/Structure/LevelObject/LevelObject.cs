using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObject : MonoBehaviour
    {
        public int levelObjectDataType;

        public bool isDynamic;

        public bool ActivateWhenInactive;

        private bool initialized;

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

        public virtual void OnStartRound()
        {
        }

        public virtual void OnEndRound()
        {
        }
    }
}