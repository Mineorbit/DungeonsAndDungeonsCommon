using System;
using System.Collections;
using System.Linq;
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

        
        public bool usingLeftItem = false;

        
        public bool usingRightItem = false;
        
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
        
        


        public string playerName;
        public Color playerColor;

        
        public bool aimMode = false;

        public bool aiming;

        public bool jumping;

        public virtual void Awake()
        {
            colorChanger = gameObject.GetComponent<ColorChanger>();
            climbableHitbox.Attach("Climbable");
            // POSSIBLE PROBLEM WITH INTERSECTIONS
            climbableHitbox.enterEvent.AddListener((x) =>
            {
                inClimbing = true;
            });
            climbableHitbox.exitEvent.AddListener((x) =>
            {
                if(climbableHitbox.insideCounter == 0)
                    inClimbing = false;
            });
        }


        public bool HasShieldInHand()
        {
            if (GetRightHandHandle().slot != null)
            {

                if (GetRightHandHandle().slot.GetType() == typeof(Shield))
                {
                    return true;
                }
                
            }

            return false;
        }

        
        
        public override bool HitBlocked(Vector3 hitDirection)
        {
            if (IsUsingShield())
            {
                    Shield shield = (Shield) GetRightHandHandle().slot;
                
                    Vector3 dir = hitDirection;
                    dir.y = 0;
                    dir.Normalize();

                    Vector3 forwardDir = -transform.forward;
                    forwardDir.y = 0;
                    forwardDir.Normalize();
                    float angle = Vector3.Angle(forwardDir, dir);
                    if (angle < shield.blockAngle)
                    {
                        return true;
                    }
            }
            return false;
        }

        public override void BlockHit()
        {
            if (HasShieldInHand())
            {
                Shield shield = (Shield) GetRightHandHandle().slot;
                shield.Invoke(shield.BlockAttackEffect);
            }
        }


        public override void FinishKickback()
        {
            base.FinishKickback();
            // HOT FIX
            bool raycast;
            RaycastHit hit;
            (raycast,hit) = GroundCheck();
            Vector3 pos = transform.position + (Vector3.up * (heightRay - hit.distance + 0.125f) );
            rigidbody.position = pos;
            transform.position = pos;
            transform.rotation = Quaternion.identity;
            GameConsole.Log($"Height Increase: {(heightRay - hit.distance) }");
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var itemHandle in itemHandles) itemHandle.Dettach();
        }


        public override void OnStartRound()
        {
            base.OnStartRound();
            bool b = (Level.instantiateType == Level.InstantiateType.Online ||
                      Level.instantiateType == Level.InstantiateType.Test);
            GetController().enabled = b;
        }


        PlayerController GetController()
        {
            return (PlayerController) controller;
        }

        
        public Hitbox climbableHitbox;

        public bool inClimbing;
          
        public float climbDampening = 0.5f;

        public float climbingSpeed = 1f;

        public float Speed = 1;
        
        
        
        public Vector3 targetDirection;
        public Vector3 movingDirection;
        public Vector3 forwardDirection;
        public Vector3 cameraForwardDirection;
        
        
        public bool movementInputOnFrame;
        public bool doInput;
        public bool takeInput;

        
        
        
        public float jumpingSpeed = 1.75f;
        public void Jump()
        {
            if (isGrounded)
            { 
                GameConsole.Log("JUMP PLAYER");
                jumping = true;
                speedY = jumpingSpeed;
            }
        }
        
        public void MoveFixed()
        {
            if (!isGrounded)
            {
                if(inClimbing)
                {
                    speedY = -gravity * climbDampening;
                }
                else
                {
                    speedY -= gravity * Time.deltaTime;
                }
            }


            
            if (ApplyMovement)
            {
                
                
                // change to angle towards ladder later on
                if (targetDirection.sqrMagnitude >= 0.01f && inClimbing)
                {
                    speedY = climbingSpeed;
                }
                
                
                targetDirection.y = speedY;
                if (targetDirection.sqrMagnitude >= 0.01f)
                    movingDirection = targetDirection;
                else
                    movingDirection = new Vector3(0, 0, 0);
                
                
                
                GetController().controller.Move(targetDirection * Speed * Time.deltaTime);

                if ((!lastGrounded && isGrounded))
                {
                    speedY = 0;
                    jumping = false;
                    //Debug.Break();
                }
                
                if ( speed > 0) forwardDirection = (forwardDirection + movingDirection) / 2;
                
                
                
                //  ROTATION FOR AIMING BOW MAGIC NUMBER YET TO BE REDISTRIBUTED
                if (aimMode && aiming)
                {
                    Vector3 lookDir = cameraForwardDirection;
                    lookDir.y = 0;
                    transform.rotation = Quaternion.AngleAxis(25, Vector3.up)*Quaternion.LookRotation(lookDir);
                }else if (IsUsingShield())
                {
                    Vector3 lookDir = cameraForwardDirection;
                    lookDir.y = 0;
                    transform.rotation = Quaternion.AngleAxis(25, Vector3.up)*Quaternion.LookRotation(lookDir);
                }else if (doInput && takeInput && movementInputOnFrame)
                {
                    var angleY = 180 + 180 / Mathf.PI * Mathf.Atan2(forwardDirection.x, forwardDirection.z);
                    transform.eulerAngles = new Vector3(0, angleY, 0);
                }
            }
        }

        public bool IsUsingShield()
        {
            if (HasShieldInHand())
            {
                Shield shield = (Shield) GetRightHandHandle().slot;
                return shield.raised;
            }

            return false;
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



        public void FixedUpdate()
        {
            base.FixedUpdate();
            MoveFixed();
        }

        public void Update()
        {
            baseAnimator.speed = speed / 3;
        }

        public void Interact()
        {
            UpdateEquipItem();
        }
        
        public void UpdateEquipItem()
        {
            var toAttach = itemsInProximity.Find(x => !x.isEquipped);
            if (toAttach != null && !toAttach.isEquipped)
            {
                if (toAttach.equipSide == Item.Side.Left)
                {
                    AttachLeftItem(toAttach);
                }else
                if (toAttach.equipSide == Item.Side.Right)
                {
                    AttachRightItem(toAttach);
                }
                else
                {
                    GameConsole.Log("Item cannot be equipped due to side setting");
                }
            }
            else
            {
                DettachLeftHandItem();
                DettachRightHandItem();
                DettachLeftBackItem();
                DettachRightBackItem();
            }
        }

        public void DettachLeftHandItem()
        {
            GetLeftHandHandle().Dettach();
        }

        public void DettachRightHandItem()
        {
            GetRightHandHandle().Dettach();
        }
        public void DettachLeftBackItem()
        {
            GetLeftBackHandle().Dettach();
        }

        public void DettachRightBackItem()
        {
            GetRightBackHandle().Dettach();
        }

        public void AttachLeftItem(Item item)
        {
            GameConsole.Log($"Player {localId} calling AttachLeft {GetLeftHandHandle().Empty()}");
            if(GetLeftHandHandle().Empty())
            {
                GetLeftHandHandle().Attach(item);
            }
            else if(GetLeftBackHandle().Empty())
            {
                GetLeftBackHandle().Attach(item);
            }
            else
            {
                GetLeftBackHandle().Dettach();
                GetLeftBackHandle().Attach(item);
            }
        }

        public void AttachRightItem(Item item)
        {
            GameConsole.Log($"Player {localId} calling AttachRight {GetLeftHandHandle().Empty()}");
            if(GetRightHandHandle().Empty())
            {
                GetRightHandHandle().Attach(item);
            }
            else if(GetRightBackHandle().Empty())
            {
                GetRightBackHandle().Attach(item);
            }
            else
            {
                GetRightBackHandle().Dettach();
                GetRightBackHandle().Attach(item);
            }
        }
        
        public void SwapEffect()
        {
            ((PlayerBaseAnimator)baseAnimator).SwapItem();
        }
        

        public void SwapLeft()
        {

            bool c = false;
                Item a = GetLeftBackHandle().slot;
                Item b = GetLeftHandHandle().slot;
                GetLeftBackHandle().Dettach();
                GetLeftHandHandle().Dettach();
                if(a != null)
                {
                    GetLeftHandHandle().Attach(a);
                    c = true;
                }
                if(b != null)
                {
                    GetLeftBackHandle().Attach(b);
                    c = true;
                }
                if(c) Invoke(SwapEffect);
        }

        
        
        public void SwapRight()
        {
            
            bool c = false;
                Item a = GetRightBackHandle().slot;
                Item b = GetRightHandHandle().slot;
                GetRightBackHandle().Dettach();
                GetRightHandHandle().Dettach();
                if(a != null)
                {
                    GetRightHandHandle().Attach(a);
                    c = true;
                }
                if(b != null)
                {
                    GetRightBackHandle().Attach(b);
                    c = true;
                }
                if(c) Invoke(SwapEffect);
        }
        
        public ItemHandle GetLeftHandHandle()
        {
            return itemHandles[0];
        }

        public ItemHandle GetRightHandHandle()
        {
            return itemHandles[1];
        }
        
        public ItemHandle GetLeftBackHandle()
        {
            return itemHandles[2];
        }

        public ItemHandle GetRightBackHandle()
        {
            return itemHandles[3];
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
            
            aimMode = true;
            var h = GetLeftHandHandle();
            if (h != null) UseHandle(h);
            if (h.slot != null) usingLeftItem = true;
        }

        public void UseRight()
        {
            aimMode = false;
            var h = GetRightHandHandle();
            if (h != null) UseHandle(h);
            if (h.slot != null) usingRightItem = true;
        }

        public void StopUseLeft()
        {
            var h = GetLeftHandHandle();
            if (h != null)
            {
                StopUseHandle(h);
                if (h.slot != null) usingLeftItem = false;
            }
        }
        
        public void StopUseRight()
        {
            var h = GetRightHandHandle();
            if (h != null)
            {
                StopUseHandle(h);
                if (h.slot != null) usingRightItem = false;
            }
        }
        
    }
}
