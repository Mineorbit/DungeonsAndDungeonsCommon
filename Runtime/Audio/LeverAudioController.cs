using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LeverAudioController : AudioController
    {
        public void PlaySwitch()
        {
            Play(0);
        }

        public void PlayerLock()
        {
            Play(1);
        }
    }
}