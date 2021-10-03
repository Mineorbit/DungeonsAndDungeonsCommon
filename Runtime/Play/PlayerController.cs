using System.Collections.Generic;
using PlasticGui.WorkspaceWindow;
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
        public bool activated = true;
        public bool takeInput = true;
        public bool doInput;
        public bool isMe;
        public bool allowedToMove = true;


        private static float distance = 5;
        private static float aimDistance = 20;





	public float jumpingSpeed = 1.75f;

        //Needs to be refined

        private Vector3 forwardDirection;
        private readonly float gravity = 4f;


        public static Vector3 GetTargetPoint()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            Vector3 start = ray.GetPoint((Camera.main.transform.position-PlayerManager.currentPlayer.transform.position).magnitude); 
            RaycastHit hit;
            int mask = LayerMask.NameToLayer("HitBox");
            int realmask = ~mask;
            Vector3 target;
            
            // Debug.DrawRay(start,ray.direction*5f,UnityEngine.Color.red,200);
            if (Physics.Raycast(start, ray.direction, out hit, aimDistance, realmask))
            {
                target = hit.point;
                GameConsole.Log(hit.collider.gameObject.name);
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
            MoveFixed();
        }


        Quaternion GetAimDirection()
        {
            Vector3 target = GetTargetPoint();
            Vector3 dir = target - transform.position;
            Debug.DrawLine(transform.position,target,Color.green,200);
            return Quaternion.LookRotation(dir,Vector3.up);
        }


        private bool aimMode = false;
        public void MoveFixed()
        {
            
            if (!((Player) entity).isGrounded && activated)
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
            
            if (((Player) entity).isGrounded || !activated) speedY = 0;
            
            if (activated)
            {
                
                if (((Player) entity).isGrounded)
                    if (Input.GetKeyDown(KeyCode.Space))
                        speedY = jumpingSpeed;
                
                
                // change to angle towards ladder later on
                if (targetDirection.sqrMagnitude >= 0.01f && inClimbing)
                {
                    speedY = climbingSpeed;
                }
                
                player.aimRotation = GetAimDirection();
                
                targetDirection.y = speedY;
                if (targetDirection.sqrMagnitude >= 0.01f)
                    movingDirection = targetDirection;
                else
                    movingDirection = new Vector3(0, 0, 0);
                controller.Move(targetDirection * Speed * Time.deltaTime);
            
                if ( entity.speed > 0) forwardDirection = (forwardDirection + movingDirection) / 2;
                
                
                
                //  ROTATION FOR AIMING BOW MAGIC NUMBER YET TO BE REDISTRIBUTED
                if (aimMode && player.aiming)
                {
                    Vector3 lookDir = -cam.forward;
                    lookDir.y = 0;
                    transform.rotation = Quaternion.AngleAxis(25, Vector3.up)*Quaternion.LookRotation(lookDir);
                }else
                if (doInput && takeInput && movementInputOnFrame)
                {
                    var angleY = 180 + 180 / Mathf.PI * Mathf.Atan2(forwardDirection.x, forwardDirection.z);
                    transform.eulerAngles = new Vector3(0, angleY, 0);
                }
            }
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

                
                
                aimMode = Input.GetMouseButton(0);
                
                // Pickup closest item
                if (Input.GetKeyDown(KeyCode.G)) player.Invoke(player.UpdateEquipItem);

                if (Input.GetKeyDown(KeyCode.G))
                {
                    // INVOKE
                    // INTERACT WITH ENVIRONMENT (READ TEXT, PRESS BUTTON etc)
                }

                
                
                
                if (Input.GetMouseButtonUp(0)) player.Invoke(player.StopUseLeft, true);
                else
                if (Input.GetMouseButtonUp(1)) player.Invoke(player.StopUseRight, true);
                else
                if (Input.GetMouseButton(0))
                    //INVOKE
                    player.Invoke(player.UseLeft, true);
                else 
                if (Input.GetMouseButton(1))
                    //INVOKE
                    player.Invoke(player.UseRight, true);

                
                
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
