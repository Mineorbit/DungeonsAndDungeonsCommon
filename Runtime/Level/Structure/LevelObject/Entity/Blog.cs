using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
    {

        public override void OnInit()
        {
			base.OnInit();
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            health = 100;
        }


        public override void OnEnable()
        {
			base.OnEnable();
        }

        public override void OnDisable()
        {
			base.OnDisable();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
