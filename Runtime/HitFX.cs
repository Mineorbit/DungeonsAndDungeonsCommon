using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
	public class HitFX : MonoBehaviour
	{
		public ParticleSystem particleSystem;
		public HitFXAudioController hitFXAudioController;
		private static float lastTime = 1f;
		private Queue<HitFX> queue;
		public void Setup(Queue<HitFX> q)
		{
			gameObject.SetActive(false);
			//particleSystem.Play();
			queue = q;
			Invoke("Despawn",lastTime);
		}
            public void Spawn(Vector3 position)
    		{
    			gameObject.SetActive(true);
    			gameObject.transform.position = position;
                //PLAY SOUND OF HIT
                hitFXAudioController.Cast();
    			particleSystem.Play();
                Invoke("Despawn",lastTime);
    		}
    		public void  Despawn()
    		{
    			gameObject.SetActive(false);
                queue.Enqueue(this);
    		}
	}
}
