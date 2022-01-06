using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
	public class HitFX : MonoBehaviour
	{
		public ParticleSystem particleSystem;

		public void Setup()
		{
			particleSystem.Play();
		}
            public void Spawn(Vector3 position)
    		{
    			gameObject.SetActive(true);
    			gameObject.transform.position = position;
    			particleSystem.Play();
    		}
    		public void  Despawn()
    		{
    			gameObject.SetActive(false);
    		}
	}
}
