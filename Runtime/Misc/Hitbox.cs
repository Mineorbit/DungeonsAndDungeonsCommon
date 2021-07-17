using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Hitbox : MonoBehaviour
    {
        public UnityEvent<GameObject> enterEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> exitEvent = new UnityEvent<GameObject>();

        public Collider hitboxCollider;

        public bool isAttached;

        public string targetTag;


        public int insideCounter = 0;
        
        public void Start()
        {
        
        }
        
        public void OnTriggerEnter(Collider other)
        {
            try
            {
                if(!String.IsNullOrEmpty(targetTag))
                    if (other.gameObject.CompareTag(targetTag))
                    {
                        insideCounter++;
                        enterEvent.Invoke(other.gameObject);
                    }
            }catch (Exception e)
            {
                GameConsole.Log(e.ToString());
            } 
        }

        public void OnTriggerExit(Collider other)
        {
            try
            {
                if(!String.IsNullOrEmpty(targetTag))
                    if (other.gameObject.CompareTag(targetTag))
                    {
                        insideCounter--;
                        exitEvent.Invoke(other.gameObject);
                    }
            }
            catch (Exception e)
            {
                GameConsole.Log(e.ToString());
            }
        }

        public void Attach(string target)
        {
            targetTag = target;
            isAttached = true;
            enterEvent = new UnityEvent<GameObject>();
            exitEvent = new UnityEvent<GameObject>();
        }

        public void Attach(GameObject parent, string target, Vector3 localLocation)
        {
            transform.parent = parent.transform;
            transform.localPosition = localLocation;
            Attach(target);
        }

        public void Activate()
        {
            if (isAttached)
                hitboxCollider.enabled = true;
            else GameConsole.Log("Hitbox " + gameObject.name + " was not attached");
        }

        public void Deactivate()
        {
            if (isAttached) hitboxCollider.enabled = false;
        }
    }
}
