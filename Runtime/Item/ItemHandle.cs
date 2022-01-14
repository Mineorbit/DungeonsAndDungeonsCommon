using System;
using System.Collections;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemHandle : MonoBehaviour
    {
        public enum HandleType
        {
            LeftHand,
            RightHand
        }

        public bool Back = false;
        
        public Item slot;
        public HandleType handleType;
        
        private PlayerController p;
        private Entity player;



        [Serializable]
        public struct ItemOffset
        {
            public string itemType;
            public Vector3 offset;
        }

        public ItemOffset[] itemOffsets;
        
        public Vector3 GetLocalOffset()
        {
            foreach (ItemOffset itemOffset in itemOffsets)
            {
                if (itemOffset.itemType == slot.GetType().Name)
                {
                    return itemOffset.offset;
                }
            }
            return itemOffsets.Length > 0 ? itemOffsets[0].offset : Vector3.zero;
            
        }
        private void Start()
        {
            p = GetComponentInParent<PlayerController>();
            player = GetComponentInParent<Player>();
        }


        public void Attach(Item item)
        {
            item.transform.position = transform.position;
            item.transform.localRotation = Quaternion.identity;
            item.transform.parent = transform;
            slot = item;
            if (!player.items.Contains(item)) player.items.Add(item);
            item.itemHandle = this;
            slot.OnAttach();
            slot.transform.localPosition = slot.GetAttachmentPoint() + GetLocalOffset();
        }


        public void Dettach()
        {
            if (slot != null)
            {
                LevelManager.currentLevel.AddToDynamic(slot.gameObject, slot.gameObject.transform.position,
                    new Quaternion(0, 0, 0, 0));
                player.items.Remove(slot);
                slot.OnDettach();
                slot.itemHandle = null;
                slot = null;
            }
        }

        public void Use()
        {
            if (slot != null)
            {
                slot.Use();

                var waitingStarted = false;
                if (slot.GetType() == typeof(Sword))
                {
                    waitingStarted = true;
                    UseWait();
                }

                if (waitingStarted)
                    player.setMovementStatus(false);
            }
            else
            {
                player.setMovementStatus(true);
            }
        }


        public virtual void UseWait()
        {
            StartCoroutine(useWaitTime(slot.useTime));
        }

        private IEnumerator useWaitTime(float t)
        {
            yield return new WaitForSeconds(t);
            StopUse();
            player.setMovementStatus(true);
        }


        public bool Empty()
        {
            return (slot == null);
        }
        
        public void StopUse()
        {
            if (slot != null)
                slot.StopUse();
            player.setMovementStatus(true);
        }
        //currently for bow
        public void StopUse(Quaternion arg)
        {
            if (slot != null)
                slot.StopUse(arg);
            player.setMovementStatus(true);
        }
    }
}