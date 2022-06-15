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

        public override void OnActivated()
        {
            SetOpen();
            navMeshObstacle.enabled = false;
        }
        public override void OnDeactivated()
        {
            SetClosed();
            navMeshObstacle.enabled = true;
        }
        public void SetOpen()
        {
            gateBaseAnimator.Set(true);
            gateCollider.enabled = false;
        }

        public void SetClosed()
        {
            gateBaseAnimator.Set(false);
            gateCollider.enabled = true;
        }

        
    }
}