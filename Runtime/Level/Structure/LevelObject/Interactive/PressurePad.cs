using System.Collections;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PressurePad : InteractiveLevelObject
    {
        public Hitbox playerStandinghitbox;
        public Collider buildCollider;

        public bool returnToUnpress;

        public float unpressTime = 5f;

        public ButtonBaseAnimator buttonBaseAnimator;

        public bool pressed;
        public TimerManager.Timer unpressTimer;


        public override void OnInit()
        {
            base.OnInit();
            buildCollider.enabled = Level.instantiateType == Level.InstantiateType.Edit;
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            buildCollider.enabled = false;
            playerStandinghitbox.Attach("Entity");

            playerStandinghitbox.enterEvent.AddListener(x =>
            {
                TimerManager.StopTimer(unpressTimer);
                Activate();
            });
            playerStandinghitbox.exitEvent.AddListener(x => { StartCoroutine(Unpress()); });
        }

        IEnumerator Unpress()
        {
            yield return new WaitForSeconds(unpressTime);
            if (playerStandinghitbox.insideCounter == 0)
            {
                Deactivate();
            }
        }

        public override void ResetState()
        {
            base.ResetState();
            bool retToUnp = returnToUnpress;
            returnToUnpress = true;
            Deactivate();
            returnToUnpress = retToUnp;
            // Factor out into parent eventually
            buildCollider.enabled = true;
        }
        
        public override void OnActivated()
        {
            buttonBaseAnimator.Press();
        }
        
        public override void OnDeactivated()
        {
            buttonBaseAnimator.Unpress();
        }
    }
}