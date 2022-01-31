using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerController : EntityController
    {
        public PlayerBaseAnimator playerBaseAnimator;
        public Transform cam;
        public float Speed = 3f;
        public float speedY;


        public bool locallyControllable;
        public bool activated;
        public bool takeInput = true;
        public bool doInput;
        public bool isMe;
        public bool allowedToMove = true;


        public float aimDistance = 20;





	public float jumpingSpeed = 1.75f;

        //Needs to be refined



        public Vector3 GetTargetPoint()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            Vector3 start = ray.GetPoint((Camera.main.transform.position-transform.position).magnitude); 
            RaycastHit hit;
            int mask = LayerMask.NameToLayer("HitBox");
            int realmask = ~mask;
            Vector3 target;
            
            // Debug.DrawRay(start,ray.direction*5f,UnityEngine.Color.red,200);
            if (Physics.Raycast(start, ray.direction, out hit, aimDistance, realmask))
            {
                target = hit.point;
            }
            else
            {
                target = start+ray.direction*aimDistance;
            }

            return target;
        }

        
        //public static PlayerController currentPlayer;
        private Player player;

        private float turnSmoothVel;
        
        public bool movementInputOnFrame = false;

        //Setup References for PlayerController and initial values if necessary
        public void Awake()
        {
            if (Camera.main != null)
                cam = Camera.main.transform;


            controller = transform.GetComponent<CharacterController>();
            
        }

        public override void Start()
        {
            base.Start();
            player = transform.GetComponent<Player>();
        }


        public override void Update()
        {
            StateUpdate();
            Move();
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            InputUpdate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            speedY = 0;
        }

        public Quaternion GetAimDirection()
        {
            Vector3 target = GetTargetPoint();
            Vector3 dir = target - transform.position;
            //Debug.DrawLine(transform.position,target,Color.green,200);
            return Quaternion.LookRotation(dir,Vector3.up);
        }

        

        public void OnLeftUse(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                player.Invoke(player.UseLeft, true,true);
                ((Player) entity).aimMode = true;
            }
        }
        
        public void OnRightUse(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                player.Invoke(player.UseRight, true,true);
                ((Player) entity).aimMode = false;
            }
        }
        
        public void OnStopLeftUse(InputAction.CallbackContext context) 
        {
            if (doInput && takeInput && player.usingLeftItem)
            {
                player.Invoke(player.StopUseLeft, true);
            }
        }
                
        public void OnStopRightUse(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && player.usingRightItem)
            { 
                player.Invoke(player.StopUseRight, true);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                if (((Player) entity).isGrounded)
                {
                        player.jumping = true;
                        speedY = jumpingSpeed;
                }
            }
        }

        public void OnSwapLeft(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                GameConsole.Log("Swap Left");
                player.Invoke(player.SwapLeft,true);
            }
        }
        
        public void OnSwapRight(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                GameConsole.Log("Swap Right");
                player.Invoke(player.SwapRight,true);
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                player.Invoke(player.Interact, true, true);
            }
        }

        public void OnJump()
        {
            if (doInput && takeInput && !player.usingLeftItem && !player.usingRightItem)
            {
                if (((Player) entity).isGrounded)
                {
                    player.jumping = true;
                    speedY = jumpingSpeed;
                }
            }
        }
        
        public Vector2 inputDirection;
        public void OnMovementInput(InputAction.CallbackContext context)
        {
            if (doInput && takeInput)
            {
                inputDirection = context.ReadValue<Vector2>();
            }
        }

        public Vector3 cameraForwardDirection;
        public Vector3 targetDirection;
        public Vector3 movingDirection;
        public Vector3 forwardDirection;
        public Quaternion aimRotation;
        
        
        public void Move()
        {   
            if (doInput && takeInput)
            {
                
                cameraForwardDirection = -cam.forward;
                
                aimRotation = GetAimDirection();
                
                if(!player.usingLeftItem && !player.usingRightItem)
                {
                    if (cam != null && allowedToMove)
                        targetDirection =
                            Vector3.Normalize(
                            Vector3.ProjectOnPlane(cam.right, transform.up) * inputDirection.x +
                            Vector3.ProjectOnPlane(cam.forward, transform.up) * inputDirection.y);
                }
                else
                {
                    targetDirection = Vector3.zero;
                }
                
            }

        }
    
        private float eps = 0.05f;
        private void StateUpdate()
        {
            doInput = PlayerManager.acceptInput  && activated &&
                      locallyControllable; //&& !player.lockNetUpdate;
                      
            movementInputOnFrame = (inputDirection.magnitude > eps);
        }

        private void InputUpdate()
        {
            player.movingDirection = movingDirection;
            player.targetDirection = targetDirection;
            player.forwardDirection = forwardDirection;
            player.aimRotation = aimRotation;
            player.cameraForwardDirection = cameraForwardDirection;
        }
    }
}
