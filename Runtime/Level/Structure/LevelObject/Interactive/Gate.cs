using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Gate : InteractiveLevelObject
    {
        public Collider gateCollider;
        public SwitchingLevelObjectBaseAnimator gateBaseAnimator;
        public NavMeshObstacle navMeshObstacle;

        public override void OnInit()
        {
            base.OnInit();
            gateBaseAnimator.Set(false);
            gateCollider.enabled = true;
            navMeshObstacle.enabled = true;
        }

        public override void Activate()
        {
            base.Activate();
            Invoke(SetOpen);
            navMeshObstacle.enabled = false;
        }

        public void SetOpen()
        {
            Debug.Log("Opening Gate");
            gateBaseAnimator.Set(true);
            gateCollider.enabled = false;
        }

        public void SetClosed()
        {
            Debug.Log("Closing Gate");
            gateBaseAnimator.Set(false);
            gateCollider.enabled = true;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Invoke(SetClosed);
            navMeshObstacle.enabled = true;
        }
    }
}