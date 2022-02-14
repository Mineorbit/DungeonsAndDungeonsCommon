using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerController : EntityController
    {
        public PlayerBaseAnimator playerBaseAnimator;
        public Transform cam;
        [FormerlySerializedAs("Speed")] public float speed = 3f;
        public float speedY;


        public bool locallyControllable;
        public bool activated;
        public bool takeInput = true;
        public bool isMe;
        public bool allowedToMove = true;


        public float aimDistance = 20;






        //Needs to be refined



        public Vector3 GetTargetPoint()
        {
            Debug.Assert(Camera.main != null, "Camera.main != null");
            Camera main;
            Ray ray = (main = Camera.main).ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            Vector3 start = ray.GetPoint((main.transform.position-transform.position).magnitude); 
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
        private Player _player;

        private float _turnSmoothVel;
        
        public bool movementInputOnFrame;

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
            _player = transform.GetComponent<Player>();
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
            return Quaternion.LookRotation(dir,Vector3.up);
        }

        

        public void OnLeftUse(InputAction.CallbackContext context)
        {
            if (takeInput && !_player.usingLeftItem && !_player.usingRightItem)
            {
                _player.Invoke(_player.UseLeft, false);
            }
        }
        
        public void OnRightUse(InputAction.CallbackContext context)
        {
            if (takeInput && !_player.usingLeftItem && !_player.usingRightItem)
            {
                _player.Invoke(_player.UseRight, false);
            }
        }
        
        public void OnStopLeftUse(InputAction.CallbackContext context) 
        {
            if (takeInput && _player.usingLeftItem)
            {
                _player.Invoke(_player.StopUseLeft, false);
            }
        }
                
        public void OnStopRightUse(InputAction.CallbackContext context)
        {
            if (takeInput && _player.usingRightItem)
            { 
                _player.Invoke(_player.StopUseRight, false);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (takeInput && !_player.usingLeftItem && !_player.usingRightItem)
            {
                _player.Invoke(_player.Jump,false);
            }
        }

        public void OnSwapLeft(InputAction.CallbackContext context)
        {
            if (takeInput && !_player.usingLeftItem && !_player.usingRightItem)
            {
                _player.Invoke(_player.SwapLeft,false);
            }
        }
        
        public void OnSwapRight(InputAction.CallbackContext context)
        {
            if (takeInput && !_player.usingLeftItem && !_player.usingRightItem)
            {
                _player.Invoke(_player.SwapRight,false);
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (takeInput && !_player.usingLeftItem && !_player.usingRightItem)
            {
                _player.Invoke(_player.Interact, false);
            }
        }

       
        public Vector2 inputDirection;
        public void OnMovementInput(InputAction.CallbackContext context)
        {
            if (takeInput)
            {
                inputDirection = context.ReadValue<Vector2>();
            }

            if (!context.performed)
            {
                inputDirection = Vector2.zero;
            }
        }

        public Vector3 cameraForwardDirection;
        public Vector3 targetDirection;
        public Quaternion aimRotation;


        private void Move()
        {
                cameraForwardDirection = -cam.forward;
                
                aimRotation = GetAimDirection();
                
                if(!_player.usingLeftItem && !_player.usingRightItem)
                {
                    if (cam != null && allowedToMove)
                    {
                        var up = transform.up;
                        targetDirection =
                            Vector3.Normalize(
                                Vector3.ProjectOnPlane(cam.right, up) * inputDirection.x +
                                Vector3.ProjectOnPlane(cam.forward, up) * inputDirection.y);
                    }
                }
                else
                {
                    targetDirection = Vector3.zero;
                }
                
            

        }
    
        private float eps = 0.05f;
        private void StateUpdate()
        {
                      
            movementInputOnFrame = (inputDirection.magnitude > eps);
            //GameConsole.Log($"Test: {movementInputOnFrame} {takeInput}");
            if (!movementInputOnFrame || ! takeInput)
            {
                _player.targetDirection = Vector3.zero;
            }
        }

        private void InputUpdate()
        {
            if(Level.instantiateType == Level.InstantiateType.Test)
            {
                _player.targetDirection = targetDirection;
                _player.aimRotation = aimRotation;
                _player.cameraForwardDirection = cameraForwardDirection;
                _player.movementInputOnFrame = movementInputOnFrame;
                _player.takeInput = takeInput;
            }

            if (Level.instantiateType == Level.InstantiateType.Online)
            {
               ( (PlayerNetworkHandler) _player.GetNetworkHandler()).UpdateInputData();
            }
        }
    }
}
