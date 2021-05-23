using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetMover : MonoBehaviour
    {

        public Transform target;
        public void Update()
        {
            if (target != null) transform.position = target.position;
        }
    }
}