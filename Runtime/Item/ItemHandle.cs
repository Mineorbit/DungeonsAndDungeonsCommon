using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{


    public class ItemHandle : MonoBehaviour
    {
        public enum HandleType { LeftHand, RightHand };
        public Item slot;
        public HandleType handleType;

        PlayerController p;
        Entity player;

        void Start()
        {
            p = GetComponentInParent<PlayerController>();
            player = GetComponentInParent<Player>();
        }


        public void Attach(Item item)
        {
            item.transform.position = transform.position;
            item.transform.localRotation = new Quaternion(0,0,0,0);
            item.transform.parent = transform;
            slot = item;
            player.items[0] = item;
            slot.OnAttach();
        }
        public void Dettach()
        {
            if(slot != null)
            {
                LevelManager.currentLevel.AddToDynamic(slot.gameObject,slot.gameObject.transform.position, new Quaternion(0,0,0,0));
                player.items[0] = null;
                slot.OnDettach();
                slot = null;
            }
        }

        public void Use()
        {
            if(slot != null)
            { 
                slot.Use();
                player.setMovementStatus(false);
                UseWait();
            }else
            {
                player.setMovementStatus(true);
            }
        }

        IEnumerator useWaitTime(float t)
        {
            yield return new WaitForSeconds(t);
            StopUse();
            player.setMovementStatus(true);
        }


        public virtual void UseWait()
        {
            StartCoroutine(useWaitTime(slot.useTime));
        }



        public void StopUse()
        {
            slot.StopUse();
        }
    }
}