using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Hitbox : MonoBehaviour
    {

        public UnityEvent<GameObject> enterEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> exitEvent = new UnityEvent<GameObject>();
        Collider collider;

        string targetTag;

        void Start()
        {
        }

        public void Attach(string target)
        {
            targetTag = target;
            collider = GetComponent<Collider>();
        }

        public void Activate()
        {
            collider.enabled = true;
        }

        public void Deactivate()
        {
            collider.enabled = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == targetTag)
            {
                enterEvent.Invoke(other.gameObject);
            }
        }
        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == targetTag)
            {
                exitEvent.Invoke(other.gameObject);
            }
        }
    }
}