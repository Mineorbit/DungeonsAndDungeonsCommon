﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Player : Entity
    {
        public enum Color
        {
            Blue,
            Red,
            Green,
            Yellow
        }

        public ColorChanger colorChanger;

	int local_Id;

        public int localId
        {
        get {
        return local_Id;
        }
        
        set {
        local_Id = value;
        changeColor(value);
        }
       
        }

        public bool isGrounded;
        
        public float heightRay = 1.1f;
        
        public float speed;


        public string playerName;
        public Color playerColor;


        public bool aiming;
        

        public virtual void Awake()
        {
            colorChanger = gameObject.GetComponent<ColorChanger>();
        }

        public void UpdateGround()
        {
            var mask = 1 << 10 | 1 << 11;
            var hit = new RaycastHit();
            isGrounded =  Physics.Raycast(transform.position, -Vector3.up, out hit, heightRay,
                mask, QueryTriggerInteraction.Ignore);
        }
        
        public bool IsGrounded()
        {
            return isGrounded;
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var itemHandle in itemHandles) itemHandle.Dettach();
        }


        public override void OnStartRound()
        {
            base.OnStartRound();
            health = 100;
        }


        public override void Spawn(Vector3 location, Quaternion rotation, bool allowedToMove)
        {
            /*
            Thread t = new Thread(new ThreadStart(() => { base.Spawn(location, rotation, allowedToMove); }));
            t.IsBackground = true;
            t.Start();
            */
            //if (loadTarget != null) { loadTarget.WaitForChunkLoaded(location); }  
            base.Spawn(location, rotation, allowedToMove);
        }


        public void DettachLeftItem()
        {
            GetLeftHandle().Dettach();
        }

        public void DettachRightItem()
        {
            GetRightHandle().Dettach();
        }

        public void AttachLeftItem(Item item)
        {
            GetLeftHandle().Attach(item);
        }

        public void AttachRightItem(Item item)
        {
            GetRightHandle().Attach(item);
        }

        public void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateGround();
        }

        public void Update()
        {
            baseAnimator.speed = currentSpeed / 3;
        }

        public void UpdateEquipItem()
        {
            var toAttach = itemsInProximity.Find(x => !x.isEquipped);
            if (toAttach != null)
            {
                if (toAttach.equipSide == Item.Side.Left)
                {
                    Invoke(DettachLeftItem);
                    Invoke(AttachLeftItem, toAttach);
                }else
                if (toAttach.equipSide == Item.Side.Right)
                {
                    Invoke(DettachRightItem);
                    Invoke(AttachRightItem, toAttach);
                }
                else
                {
                    GameConsole.Log("Item cannot be equipped due to side setting");
                }
            }
            else
            {
                Invoke(DettachLeftItem);
                Invoke(DettachRightItem);
            }
        }


        public ItemHandle GetLeftHandle()
        {
            foreach (var i in itemHandles)
                if (i.handleType == ItemHandle.HandleType.LeftHand)
                    return i;
            return null;
        }

        public ItemHandle GetRightHandle()
        {
            foreach (var i in itemHandles)
                if (i.handleType == ItemHandle.HandleType.RightHand)
                    return i;
            return null;
        }


        public virtual bool IsAlive()
        {
            return alive;
        }

        public void changeColor(int id)
        {
            switch (id)
            {
                case 0:
                    playerColor = Color.Blue;
                    setColor(UnityEngine.Color.blue);
                    break;
                case 1:
                    playerColor = Color.Red;
                    setColor(UnityEngine.Color.red);
                    break;
                case 2:
                    playerColor = Color.Green;
                    setColor(UnityEngine.Color.green);
                    break;
                case 3:
                    playerColor = Color.Yellow;
                    setColor(UnityEngine.Color.yellow);
                    break;
            }
        }

        private void setColor(UnityEngine.Color baseC)
        {
            colorChanger.SetColor(5, baseC);
            colorChanger.SetColor(0, colorChanger.comp(baseC));
            colorChanger.SetColor(6, UnityEngine.Color.Lerp(baseC, UnityEngine.Color.white, 0.75f));
        }


        public override void setMovementStatus(bool allowedToMove)
        {
            base.setMovementStatus(allowedToMove);
            PlayerManager.playerManager.playerControllers[localId].allowedToMove = allowedToMove;
        }


        public void UseLeft()
        {
            var h = GetLeftHandle();
            if (h != null) UseHandle(h);
        }

        public void UseRight()
        {
            var h = GetRightHandle();
            if (h != null) UseHandle(h);
        }

        public void StopUseLeft()
        {
            var h = GetLeftHandle();
            if (h != null)
            {
                StopUseHandle(h);
            }
        }
        
        public void StopUseRight()
        {
            var h = GetRightHandle();
            if (h != null)
            {
                StopUseHandle(h);
            }
        }
        
    }
}
