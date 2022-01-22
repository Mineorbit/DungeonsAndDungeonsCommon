using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ExplosionFX : FX
    {
        public ParticleSystem particleSystem;
        public AudioController explosionFXAudioController;
        private static float lastTime = 1f;
        private Queue<FX> queue;
        

        public override void Setup(Queue<FX> q)
        {
            gameObject.SetActive(false);
            //particleSystem.Play();
            queue = q;
            Invoke("Despawn",lastTime);
        }
        
        public override void Spawn(Vector3 position)
        {
            gameObject.SetActive(true);
            gameObject.transform.position = position;
            //PLAY SOUND OF HIT
            explosionFXAudioController.Play(0);
            particleSystem.Play();
            Invoke("Despawn",lastTime);
        }
        
        public override void  Despawn()
        {
            gameObject.SetActive(false);
            queue.Enqueue(this);
        }
    }
}