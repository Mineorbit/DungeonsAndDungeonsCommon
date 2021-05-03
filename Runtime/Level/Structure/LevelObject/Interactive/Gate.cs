using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Gate : InteractiveLevelObject
    {
        public Collider collider;
        public GateBaseAnimator gateBaseAnimator;
        public UnityEngine.AI.NavMeshObstacle navMeshObstacle;
        
        public override void OnInit()
        {
            base.OnInit();
            gateBaseAnimator.Close();
            collider.enabled = true;
            navMeshObstacle.enabled = true;
        }

        public override void Activate()
        {
            base.Activate();
            SetOpen();
            navMeshObstacle.enabled = false;
        }

        public void SetOpen()
        {

            gateBaseAnimator.Open();
            collider.enabled = false;
        }

        public void SetClosed()
        {
            gateBaseAnimator.Close();
            collider.enabled = true;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            SetClosed();
            navMeshObstacle.enabled = true;
        }

    }
}