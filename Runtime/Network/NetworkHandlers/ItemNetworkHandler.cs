using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemNetworkHandler : EntityNetworkHandler
    {
        public override void Start()
        {
            base.Start();
            transmitPosition = false;
            GetObservedItem().onAttachEvent.AddListener(() => { owner = ((Player) GetObservedItem().owner).localId; 
                transmitPosition = false;});
            GetObservedItem().onDettachEvent.AddListener(() => { owner = -1; 
                transmitPosition = true;});

        }

        public Item GetObservedItem()
        {
            return (Item) GetObservedEntity();
        }
       
    }
}