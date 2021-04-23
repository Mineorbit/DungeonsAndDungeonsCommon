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
