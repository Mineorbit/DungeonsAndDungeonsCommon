using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class GateBaseAnimator : BaseAnimator
    {
        public GameObject model;

        public void Open()
        {
            Debug.Log("Open Animation");
            model.SetActive(false);
        }

        public void Close()
        {
            Debug.Log("Close Animation");
            model.SetActive(true);
        }
    }
}