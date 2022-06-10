using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class InteractableLevelObject : InteractiveLevelObject
    {
        private Hitbox playerNearHitbox;

        public override void OnInit()
        {
            base.OnInit();
            gameObject.tag = "Interactable";
        }

        public virtual void Interact()
        {
            
        }
    }
}