using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObject : MonoBehaviour
    {

        public string levelObjectDataType;

		bool initialized = false;

        public virtual void OnInit()
        {
			initialized = true;
        }

        public virtual void OnDeInit()
        {
			initialized = false;
        }

        public virtual void Reset()
        {
            Debug.Log("Resetting "+levelObjectDataType);
            OnDeInit();
            OnInit();
        }
        

        public void OnDestroy()
        {
            Debug.Log("LevelObject destroyed: "+gameObject.name);
        }
 

        public virtual void  OnStartRound()
        {

        }

        public virtual void OnEndRound()
        {

        }


        void Start()
        {

        }

        void Update()
        {

        }
    }
}
