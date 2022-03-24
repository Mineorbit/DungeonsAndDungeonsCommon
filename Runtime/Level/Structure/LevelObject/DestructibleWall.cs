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
            destroyed = false;
            wallCollider.enabled = Level.instantiateType == Level.InstantiateType.Edit;
            for(int i = 0; i < wallSegments.Length; i++ )
            {
                wallSegments[i].gameObject.SetActive(true);
                wallSegments[i].isKinematic = true;
                wallSegments[i].useGravity = false;
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

        public void ClearSegments()
        {
            foreach (var segment in wallSegments)
            {
             segment.gameObject.SetActive(false);
            }
        }

        public float clearTime = 10;
        
        public override void Destroy()
        {
            if(!destroyed)
            {
                destroyed = true;
                base.Destroy();
                wallCollider.enabled = false;
                Invoke("ClearSegments",clearTime);
            }
        }
    }

}