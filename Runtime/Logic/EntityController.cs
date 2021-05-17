using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityController : MonoBehaviour
    {
        public Vector3 controllerPosition;
        private bool dirty;
        private Vector3 positionUpdate;

        public virtual void Start()
        {
            controllerPosition = transform.position;
        }

        public virtual void Update()
        {
            if (dirty)
            {
                controllerPosition = positionUpdate;
                dirty = false;
            }
        }

        public void OnSpawn(Vector3 location)
        {
            controllerPosition = location;
        }

        public void UpdatePosition(Vector3 position)
        {
            dirty = true;
            positionUpdate = position;
        }
    }
}