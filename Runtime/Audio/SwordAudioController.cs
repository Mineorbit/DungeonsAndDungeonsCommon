using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SwordAudioController : AudioController
    {
        public Sword sword;
        void Start()
        {
            sword.onUseEvent.AddListener(Swoosh);
            sword.onHitEvent.AddListener(Hit);
        }
        void Swoosh()
        {
            Play(0);
        }

        void Hit()
        {
            Play(1);
        }
    }
}