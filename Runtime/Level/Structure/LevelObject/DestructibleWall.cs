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
            
            
        }

        public override void DestroyState(bool d)
        {
            for(int i = 0; i < wallSegments.Length; i++ )
            {
                wallSegments[i].gameObject.SetActive(!d);
                wallSegments[i].isKinematic = !d;
                wallSegments[i].useGravity = d;
                if(!d)
                {
                    wallSegments[i].transform.localPosition = positions[i];
                    wallSegments[i].transform.localRotation = Quaternion.Euler(-90,0,0);
                }
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

        public void ClearSegments()
        {
            foreach (var segment in wallSegments)
            { 
                segment.gameObject.SetActive(false);
            }
        }

        public float clearTime = 10;
        
        

        public override void DestroyEffect(Vector3 origin)
        {
            wallCollider.enabled = false;
            foreach (Rigidbody rigidbody in wallSegments)
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                rigidbody.AddExplosionForce(25,origin,25);
                
            }
            Invoke("ClearSegments",clearTime);
        }
    }

}