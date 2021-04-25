using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class PressurePad : InteractiveLevelObject
    {
        public Hitbox playerStandinghitbox;
        public override void OnInit()
        {
            base.OnInit();
            playerStandinghitbox.Attach("Player");
            playerStandinghitbox.enterEvent.AddListener((x)=> { Activate(); });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
