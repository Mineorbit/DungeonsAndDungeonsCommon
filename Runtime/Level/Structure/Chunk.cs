using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Chunk : MonoBehaviour
    {
        public int chunkId;
        void Start()
        {
            gameObject.name = "Chunk " + chunkId;
        }

    }
}
