﻿using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerController : EntityController
    {
        public PlayerBaseAnimator playerBaseAnimator;
        public Transform cam;
        public float Speed = 3f;
        public Vector3 targetDirection;
        public Vector3 movingDirection;
        public float speedY;


        public bool locallyControllable;
        public bool activated;
        public bool takeInput = true;
        public bool doInput;
        public bool isMe;
        public bool allowedToMove = true;


        public static float distance = 5;
        public static float aimDistance = 20;





	public float jumpingSpeed = 1.75f;

        //Needs to be refined

        public Vector3 forwardDirection;
        public readonly float gravity = 4f;


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
                target = start+ray.direction*distance;
            }

            return target;
        }

        public Hitbox climbableHitbox;


        public bool inClimbing;
        
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
        }


        public Quaternion GetAimDirection()
        {
            Vector3 target = GetTargetPoint();
            Vector3 dir = target - transform.position;
            Debug.DrawLine(transform.position,target,Color.green,200);
            return Quaternion.LookRotation(dir,Vector3.up);
        }


        
        
        public float climbDampening = 0.5f;

        public float climbingSpeed = 1f;
        public void Move()
        {
            targetDirection = new Vector3(0, 0, 0);
            
            if (doInput && takeInput)
            {
                if (cam != null && allowedToMove)
                    targetDirection =
                        Vector3.Normalize(
                            Vector3.ProjectOnPlane(cam.right, transform.up) * Input.GetAxisRaw("Horizontal") +
                            Vector3.ProjectOnPlane(cam.forward, transform.up) * Input.GetAxisRaw("Vertical"));

                
                
                if (((Player) entity).isGrounded)
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        player.jumping = true;
                        speedY = jumpingSpeed;
                    }
                ((Player) entity).aimMode = Input.GetMouseButton(0);
                
                ((Player) entity).aimRotation = GetAimDirection();
                // Pickup closest item
                if (Input.GetKeyDown(KeyCode.G)) player.Invoke(player.UpdateEquipItem);

                if (Input.GetKeyDown(KeyCode.G))
                {
                    // INVOKE
                    // INTERACT WITH ENVIRONMENT (READ TEXT, PRESS BUTTON etc)
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    player.Invoke(player.SwapLeft,true);
                }
                
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    player.Invoke(player.SwapRight,true);
                }
                
                if (Input.GetMouseButtonUp(0)) player.Invoke(player.StopUseLeft, true);
                else
                if (Input.GetMouseButtonUp(1)) player.Invoke(player.StopUseRight, true);
                else 
                if (Input.GetMouseButton(1))
                    //INVOKE
                    player.Invoke(player.UseRight, true);
                else
                if (Input.GetMouseButton(0))
                    //INVOKE
                    player.Invoke(player.UseLeft, true);

                
                
            }

        }

        private void StateUpdate()
        {
            doInput = PlayerManager.acceptInput  && activated &&
                      locallyControllable; //&& !player.lockNetUpdate;
                      
            movementInputOnFrame = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) ||
                                  Input.GetKey(KeyCode.D));
        }
    }
}
