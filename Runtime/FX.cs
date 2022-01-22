using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class FX : MonoBehaviour
    {

        public virtual void Setup(Queue<FX> q)
        {

        }

        // Start is called before the first frame update
        public virtual void Spawn(Vector3 position)
        {
        }

        public virtual void Despawn()
        {
            
        }
    }
}