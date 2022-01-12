using System.Collections;
using System.Collections.Generic;
using com.mineorbit.dungeonsanddungeonscommon;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemAudioController : AudioController
    {
        // Start is called before the first frame update
        public Item item;
        public virtual void Start()
        {
            item.onCollideEvent.AddListener(Collide);
        }
        
        public void Collide()
        {
            Play(2);
        }
    }
}