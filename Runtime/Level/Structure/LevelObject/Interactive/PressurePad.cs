using System.Collections;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PressurePad : InteractiveLevelObject
    {
        public Hitbox playerStandinghitbox;
        public Collider buildCollider;


        public float unpressTime = 5f;

        public ButtonBaseAnimator buttonBaseAnimator;

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
            playerStandinghitbox.exitEvent.AddListener(x => { StartCoroutine(UnpressAfterTime()); });
        }

        private IEnumerator UnpressAfterTime()
        {
            GameConsole.Log("Starting Unpressing");
            yield return new WaitForSeconds(unpressTime);
            GameConsole.Log("Check: "+playerStandinghitbox.insideCounter);
            if (playerStandinghitbox.insideCounter == 0)
            {
                GameConsole.Log("Deactivating");
                Deactivate();
            }
        }

        public override void ResetState()
        {
            base.ResetState();
            Deactivate();
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