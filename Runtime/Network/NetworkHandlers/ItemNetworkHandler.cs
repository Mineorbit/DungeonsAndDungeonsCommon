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
            transmitPosition = true;
            interpolatePosition = true;

            bool server = NetworkManager.instance.localId == -1;
            GetObservedItem().rigidbody.isKinematic = !server;
            GetObservedItem().rigidbody.useGravity = server;
            
            GetObservedItem().onAttachEvent.AddListener(() => { owner = ((Player) GetObservedItem().owner).localId; 
                transmitPosition = false;
                interpolatePosition = false;
            });
            
            GetObservedItem().onDettachEvent.AddListener(() => { owner = -1; 
                transmitPosition = true;
                interpolatePosition = true;
                GetObservedItem().rigidbody.isKinematic = !server;
                GetObservedItem().rigidbody.useGravity = server;
            });

        }

        public override bool WantsToTransmit()
        {
            return owner == NetworkManager.instance.localId;
        }
        
        public Item GetObservedItem()
        {
            return (Item) GetObservedEntity();
        }
       
    }
}