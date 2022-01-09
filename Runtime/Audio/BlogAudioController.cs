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

		public void Hit()
		{
			Play(3);
		}

		public void Footstep()
		{
			Play(0);
		}
		
		public void Death()
		{
			Play(2);
		}
		
		public void Strike()
		{
			Play(5);
		}

		public void Daze()
		{
			Play(1);
		}

		public void Undaze()
		{
			Stop(1);
		}

		// Update is called once per frame
    		void Update()
   		{
        
    		}
	}
}
