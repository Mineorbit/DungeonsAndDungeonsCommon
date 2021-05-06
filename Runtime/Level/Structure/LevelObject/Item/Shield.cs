using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Shield : Item
    {
        public override void OnAttach()
        {
            base.OnAttach();
            transform.localPosition = new Vector3(0.002f, 0, 0);
            transform.localEulerAngles = new Vector3(0, 180, 0);
        }
        public void Update()
        {
            //transform.localEulerAngles = new Vector3(0, 180, 0);
        }
    }
}