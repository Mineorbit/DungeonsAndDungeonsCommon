using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ButtonAudioController : AudioController
    {

        public void OnPress()
        {
            Play(0);
        }

        public void OnUnpress()
        {
            Play(0);
        }
        
    }
}