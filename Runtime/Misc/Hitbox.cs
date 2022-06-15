using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = System.Object;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Hitbox : MonoBehaviour
    {
        public UnityEvent<GameObject> enterEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> exitEvent = new UnityEvent<GameObject>();

        public Collider hitboxCollider;

        public bool isAttached;

        public string targetTag;


        public List<GameObject> insideObjects;
        public int insideCounter = 0;

        public List<MonoBehaviour> storedObjects;
        
        
        public void Start()
        {
        
        }

        public void StoreObjects<T>() where T: MonoBehaviour
        {
            enterEvent.AddListener(x =>
            {
                if (!storedObjects.Contains(x.GetComponent<T>())) storedObjects.Add(x.GetComponent<T>());
            });
            exitEvent.AddListener(x =>
            {
                storedObjects.RemoveAll(p => ((T)p).Equals(x.GetComponent<T>()));
            });
        }
        
        public void OnTriggerEnter(Collider other)
        {
            try
            {
                if(!String.IsNullOrEmpty(targetTag))
                    if (other.gameObject.CompareTag(targetTag))
                    {
                        insideObjects.Add(other.gameObject);
                        insideCounter = insideObjects.Count;
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
                        insideObjects.RemoveAll((x) => x == other.gameObject);
                        insideCounter = Math.Max(insideObjects.Count,0);
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
            if (!isAttached) GameConsole.Log("Hitbox " + gameObject.name + " was not attached, but still activating");
            hitboxCollider.enabled = true;
        }

        public void Deactivate()
        {
            if (!isAttached) GameConsole.Log("Hitbox " + gameObject.name + " was not attached, but still deactivating");
            hitboxCollider.enabled = false;
        }
    }
}
