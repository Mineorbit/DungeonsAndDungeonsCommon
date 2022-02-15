using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Bomb : Entity
    {
        // Start is called before the first frame update
        public float explosionTime;
        public float explosionRadius;
        public int explosionDamage = 50;
        public Vector3 throwDirection;
        public float throwForce = 25;
        void Start()
        {
            Invoke("Explode",explosionTime);
            rigidbody.AddForce(throwDirection*throwForce,ForceMode.Impulse);
        }

        public void ExplosionEffect()
        {
            EffectCaster.FX("ExplosionFX",transform.position);
        }
        
        void Explode()
        {
            GameConsole.Log("Explode");
            Invoke(ExplosionEffect,true,true);
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var collider in hitColliders)
            {
                GameConsole.Log($"R {collider}");
                
                Entity enemy = collider.GetComponent<Entity>();
                
                if (enemy != null && enemy != this)
                {
                    enemy.Hit(this,explosionDamage);
                }
            }
            Kill(); 
        }
        
        // Update is called once per frame
        void Update()
        {

        }
    }
}