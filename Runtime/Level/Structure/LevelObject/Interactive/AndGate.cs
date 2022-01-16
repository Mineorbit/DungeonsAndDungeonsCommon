using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class AndGate : InteractiveLevelObject
    {
        // Start is called before the first frame update
        public Collider buildCollider;
        public GameObject model;
        void Start()
        {

        }
        public int activeInOr = 0;
        // WHAT ABOUT NAV MESH BUILD?
        public override void OnInit()
        {
            base.OnInit();
            bool b = Level.instantiateType == Level.InstantiateType.Edit;
            buildCollider.enabled = b;
            activeInOr = 0;
            model.SetActive(b);
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            buildCollider.enabled = false;
            activeInOr = 0;
            model.SetActive(false);
        }

        public override void ResetState()
        {
            base.ResetState();
            
            buildCollider.enabled = true;
            activeInOr = 0;
            model.SetActive(true);
        }


        public override void Activate()
        {
            activeInOr++;
            if(activeInOr == inBoundWires.Count)
                base.Activate();
        }

        public override void Deactivate()
        {
            activeInOr--;
            if(activeInOr != inBoundWires.Count)
                base.Deactivate();
        }
    }
}