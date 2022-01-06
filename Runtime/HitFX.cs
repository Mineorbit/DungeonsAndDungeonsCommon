using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
	public class HitFX : MonoBehaviour
	{
		public ParticleSystem particleSystem;
		public HitFXAudioController hitFXAudioController;
		public void Setup()
		{
			gameObject.SetActive(true);
			particleSystem.Play();
		}
            public void Spawn(Vector3 position)
    		{
    			gameObject.SetActive(true);
    			gameObject.transform.position = position;
                //PLAY SOUND OF HIT
                hitFXAudioController.Cast();
    			particleSystem.Play();
    		}
    		public void  Despawn()
    		{
    			gameObject.SetActive(false);
    		}
	}
}
