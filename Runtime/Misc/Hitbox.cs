using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Hitbox : MonoBehaviour
    {
        public UnityEvent<GameObject> enterEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> exitEvent = new UnityEvent<GameObject>();

        public Collider collider;

        private bool isAttached;

        private string targetTag;

        private void Start()
        {
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == targetTag)
            {
                Debug.Log(other.gameObject.name + " inside");
                enterEvent.Invoke(other.gameObject);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == targetTag) exitEvent.Invoke(other.gameObject);
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
                collider.enabled = true;
            else Debug.Log("Hitbox " + gameObject.name + " was not attached");
        }

        public void Deactivate()
        {
            if (isAttached) collider.enabled = false;
        }
    }
}