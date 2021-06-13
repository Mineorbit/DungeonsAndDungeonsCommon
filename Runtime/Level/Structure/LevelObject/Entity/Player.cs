﻿using System.Collections;
using UnityEngine;

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

        public int localId;

        public float speed;


        public string name;
        public Color playerColor;

        private readonly bool alive = true;
        private readonly float cooldownTime = 2f;

        

        public virtual void Awake()
        {
            colorChanger = gameObject.GetComponent<ColorChanger>();
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var itemHandle in itemHandles) itemHandle.Dettach();
        }

        private void OnCollisionEnter(Collision collision)
        {
            var col = collision.collider.gameObject;
            if (col.tag == "Enemy")
            {
                //int damage = col.GetComponent<EnemyController>().damage;
                var damage = 10;
                if (damage > 0) Hit(col.GetComponent<Entity>(), damage);
            }
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


        public void UpdateEquipItem()
        {
            var toAttach = itemsInProximity.Find(x => !x.isEquipped);
            if (toAttach != null)
            {
                Debug.Log("Picking up");
                if (toAttach.GetType() == typeof(Sword))
                {
                    Invoke(DettachLeftItem);
                    Invoke(AttachLeftItem, toAttach);
                }

                if (toAttach.GetType() == typeof(Shield))
                {
                    Invoke(DettachRightItem);
                    Invoke(AttachRightItem, toAttach);
                }
            }
            else
            {
                Debug.Log("Trying drop");
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
                    playerColor = Color.Yellow;
                    setColor(UnityEngine.Color.yellow);
                    break;
                case 2:
                    playerColor = Color.Red;
                    setColor(UnityEngine.Color.red);
                    break;
                case 3:
                    playerColor = Color.Green;
                    setColor(UnityEngine.Color.green);
                    break;
            }
        }

        private void setColor(UnityEngine.Color baseC)
        {
            colorChanger.SetColor(7, baseC);
            colorChanger.SetColor(2, colorChanger.comp(baseC));
            colorChanger.SetColor(8, UnityEngine.Color.Lerp(baseC, UnityEngine.Color.white, 0.75f));
        }


        public override void setMovementStatus(bool allowedToMove)
        {
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