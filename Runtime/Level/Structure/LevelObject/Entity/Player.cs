using System;
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
        
        public float heightRay;
        


        public string playerName;
        public Color playerColor;


        public bool aiming;

        public bool jumping;

        public virtual void Awake()
        {
            colorChanger = gameObject.GetComponent<ColorChanger>();
        }


        private bool lastGrounded = false;

        public void UpdateGround()
        {
            lastGrounded = isGrounded;
            var mask = 1 << 10 | 1 << 11;
            var hit = new RaycastHit();
            bool raycast =  Physics.Raycast(transform.position, -Vector3.up, out hit, heightRay,
                mask, QueryTriggerInteraction.Ignore);
            bool controllerResult = ((PlayerController) controller).controller.isGrounded;
            isGrounded = controllerResult || raycast;
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


        PlayerController GetController()
        {
            return (PlayerController) controller;
        }
        
        
        
        public bool aimMode = false;
        public void MoveFixed()
        {
            
            if (!isGrounded && GetController().activated)
            {
                if(GetController().inClimbing)
                {
                    GetController().speedY = -GetController().gravity * GetController().climbDampening;
                }
                else
                {
                    GetController().speedY -= GetController().gravity * Time.deltaTime;
                }
            }
            
            
            if (GetController().activated)
            {
                
                
                // change to angle towards ladder later on
                if (GetController().targetDirection.sqrMagnitude >= 0.01f && GetController().inClimbing)
                {
                    GetController().speedY = GetController().climbingSpeed;
                }
                
                
                GetController().targetDirection.y = GetController().speedY;
                if (GetController().targetDirection.sqrMagnitude >= 0.01f)
                    GetController().movingDirection = GetController().targetDirection;
                else
                    GetController().movingDirection = new Vector3(0, 0, 0);
                GetController().controller.Move(GetController().targetDirection * GetController().Speed * Time.deltaTime);

                if ((!lastGrounded && isGrounded) || !GetController().activated)
                {
                    GetController().speedY = 0;
                    jumping = true;
                    //Debug.Break();
                }
                
                if ( speed > 0) GetController().forwardDirection = (GetController().forwardDirection + GetController().movingDirection) / 2;
                
                
                
                //  ROTATION FOR AIMING BOW MAGIC NUMBER YET TO BE REDISTRIBUTED
                if (aimMode && aiming)
                {
                    Vector3 lookDir = -GetController().cam.forward;
                    lookDir.y = 0;
                    transform.rotation = Quaternion.AngleAxis(25, Vector3.up)*Quaternion.LookRotation(lookDir);
                }else
                if (GetController().doInput && GetController().takeInput && GetController().movementInputOnFrame)
                {
                    var angleY = 180 + 180 / Mathf.PI * Mathf.Atan2(GetController().forwardDirection.x, GetController().forwardDirection.z);
                    transform.eulerAngles = new Vector3(0, angleY, 0);
                }
            }
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
            MoveFixed();
        }

        public void Update()
        {
            baseAnimator.speed = speed / 3;
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
