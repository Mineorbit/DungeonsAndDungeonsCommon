using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;
        public int localId;
        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
