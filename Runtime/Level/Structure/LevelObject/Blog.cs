using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Blog : Enemy
    {
        EnemyController enemyController;

        public override void OnInit()
        {
			base.OnInit();
            enemyController = GetComponent<EnemyController>();
        }

        public override void OnEnable()
        {
			base.OnEnable();
            enemyController.enabled = true;
        }

        public override void OnDisable()
        {
			base.OnDisable();
            enemyController.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
