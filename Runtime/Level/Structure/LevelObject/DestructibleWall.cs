using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class DestructibleWall : DestructibleLevelObject
    {
        public Collider wallCollider;
        public Rigidbody[] wallSegments;
        public Vector3[] positions;
        public override void OnInit()
        {
            base.OnInit();
            wallCollider.enabled = Level.instantiateType == Level.InstantiateType.Edit;
            for(int i = 0; i < wallSegments.Length; i++ )
            {
                wallSegments[i].transform.localPosition = positions[i];
                wallSegments[i].transform.localRotation = Quaternion.Euler(-90,0,0);
            }
            
        }

        public void OnValidate()
        {
            positions = new Vector3[wallSegments.Length];
            for(int i = 0; i < wallSegments.Length; i++ )
            {
                positions[i] = wallSegments[i].transform.localPosition;
            }
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            wallCollider.enabled = false;
        }
        
        public override void Destroy(Vector3 origin, float realDamage)
        {
            wallCollider.enabled = false;
            foreach (Rigidbody rigidbody in wallSegments)
            {
                rigidbody.AddExplosionForce(realDamage,origin,realDamage);
            }
        }
    }

}