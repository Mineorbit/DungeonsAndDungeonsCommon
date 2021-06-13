using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class GateBaseAnimator : BaseAnimator
    {
        public GameObject model;
        private bool open = false;
        
        public void Open()
        {
            if (!open)
            {
                open = true;
                Debug.Log("Open Animation "+model.activeSelf);
                Debug.Log("Open Animation f "+model.activeSelf);
            }
        }

        public void Update()
        {
            // Temporary fix
            model.SetActive(!open);
        }

        public void Close()
        {
            if (open)
            {
                open = false;
                Debug.Log("Close Animation"+model.activeSelf);
                Debug.Log("Close Animation f"+model.activeSelf);
            }
        }
    }
}