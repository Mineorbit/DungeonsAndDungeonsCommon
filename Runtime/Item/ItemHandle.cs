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

        void Start()
        {
            p = GetComponentInParent<PlayerController>();
        }


        public void Attach(Item item)
        {
            item.transform.position = transform.position;
            item.transform.parent = transform;
            slot = item;
            slot.OnAttach();
        }
        public void Dettach()
        {
            slot.OnDettach();
        }

        public void Use()
        {
            slot.Use();
            p.allowedToMove = false;
            UseWait();
        }

        IEnumerator useWaitTime(float t)
        {
            yield return new WaitForSeconds(t);
            StopUse();
            p.allowedToMove = true;
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