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
                Debug.Log("Open Animation "+model);
                model.SetActive(false);
            }
        }

        public void Close()
        {
            if (open)
            {
                open = false;
                Debug.Log("Close Animation");
                model.SetActive(true);
            }
        }
    }
}