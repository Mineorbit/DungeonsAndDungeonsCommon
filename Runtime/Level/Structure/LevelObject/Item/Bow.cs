using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Bow : Item
    {
        public override void OnAttach()
        {
            base.OnAttach();
            transform.localPosition = new Vector3(0, 0.002f, 0);
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}