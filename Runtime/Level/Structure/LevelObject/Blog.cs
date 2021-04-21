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

        }

        void Start()
        {
            enemyController = GetComponent<EnemyController>();
        }

        void OnEnable()
        {
            enemyController.enabled = true;
        }

        void OnDisable()
        {
            enemyController.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
