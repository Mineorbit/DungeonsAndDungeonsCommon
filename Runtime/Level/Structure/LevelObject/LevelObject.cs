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
		
		public virtual void OnEnable()
		{
			if(!initialized)
			OnInit();	
		}
		
		public virtual void OnDisable()
		{
			if(initialized)
			OnDeInit();
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

        // Update is called once per frame
        void Update()
        {

        }
    }
}
