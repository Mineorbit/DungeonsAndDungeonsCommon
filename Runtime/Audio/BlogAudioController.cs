using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    	public class BlogAudioController : EnemyAudioController
    	{
    // Start is called before the first frame update
    		void Start()
    		{
        
    		}
		public void Footstep()
		{
		Play(5);
		}

		public void Hit()
		{
			Play(2);
		}

		public void Strike()
		{
			Play(4);
		}
    // Update is called once per frame
    		void Update()
   		{
        
    		}
	}
}
