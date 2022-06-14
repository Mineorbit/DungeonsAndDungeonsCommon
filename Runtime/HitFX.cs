using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace com.mineorbit.dungeonsanddungeonscommon
{
	public class HitFX : FX
	{
		public VisualEffect effect;
		public HitFXAudioController hitFXAudioController;
		private static float lastTime = 1f;
		private Queue<FX> queue;
		public override void Setup(Queue<FX> q)
		{
			gameObject.SetActive(false);
			effect.Stop();
			//particleSystem.Play();
			queue = q;
			Invoke("Despawn",lastTime);
		}
            public override void Spawn(Vector3 position)
    		{
    			gameObject.SetActive(true);
    			gameObject.transform.position = position;
                //PLAY SOUND OF HIT
                hitFXAudioController.Cast();
    			effect.Play();
                Invoke("Despawn",lastTime);
    		}
    		public override void  Despawn()
    		{
    			gameObject.SetActive(false);
                queue.Enqueue(this);
    		}
	}
}
