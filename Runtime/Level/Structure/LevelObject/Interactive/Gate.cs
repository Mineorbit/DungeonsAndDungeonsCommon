using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Gate : InteractiveLevelObject
    {
        public Collider collider;
        public GameObject model;
        
        public override void OnInit()
        {
            base.OnInit();

            model.SetActive(true);
            collider.enabled = true;
        }

        public override void Activate()
        {
            base.Activate();
            model.SetActive(false);
            collider.enabled = false;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}