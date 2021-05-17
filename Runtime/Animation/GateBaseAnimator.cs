using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class GateBaseAnimator : BaseAnimator
    {
        public GameObject model;

        public void Open()
        {
            model.SetActive(false);
        }

        public void Close()
        {
            model.SetActive(true);
        }
    }
}