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
            GetObservedItem().rigidBody.isKinematic = !isOnServer;
            GetObservedItem().rigidBody.useGravity = isOnServer;
            GetObservedItem().rigidBody.detectCollisions = isOnServer;
            
            GetObservedItem().onAttachEvent.AddListener(() => { owner = ((Player) GetObservedItem().owner).localId; 
                transmitPosition = false;
                interpolatePosition = false;
                GetObservedItem().rigidBody.isKinematic = !isOnServer;
                GetObservedItem().rigidBody.useGravity = isOnServer;
                GetObservedItem().rigidBody.detectCollisions = isOnServer;
            });
            
            GetObservedItem().onDettachEvent.AddListener(() => { owner = -1; 
                transmitPosition = true;
                interpolatePosition = true;
                GetObservedItem().rigidBody.isKinematic = !isOnServer;
                GetObservedItem().rigidBody.useGravity = isOnServer;
                GetObservedItem().rigidBody.detectCollisions = isOnServer;
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