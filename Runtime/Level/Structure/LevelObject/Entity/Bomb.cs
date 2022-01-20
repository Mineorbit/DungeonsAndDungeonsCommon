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
        void Start()
        {
            Invoke("Explode",explosionTime);
        }

        void Explode()
        {
            GameConsole.Log("Explode");
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var collider in hitColliders)
            {
                GameConsole.Log($"R {collider}");
                
                Entity enemy = collider.GetComponent<Entity>();
                
                if (enemy != null && enemy != this)
                {
                    enemy.Hit(this,100);
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