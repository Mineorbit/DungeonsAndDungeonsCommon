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
                model.SetActive(false);
                Debug.Log("Open Animation f "+model.activeSelf);
            }
        }

        public void Close()
        {
            if (open)
            {
                open = false;
                Debug.Log("Close Animation"+model.activeSelf);
                model.SetActive(true);
                Debug.Log("Close Animation f"+model.activeSelf);
            }
        }
    }
}