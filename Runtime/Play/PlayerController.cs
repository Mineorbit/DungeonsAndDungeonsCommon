using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class PlayerController : MonoBehaviour
    {
        //public static PlayerController currentPlayer;
        Player player;
        public CharacterController controller;
        public Transform cam;
        float convergenceSpeed = 0.1f;
        float turnSmoothVel;
        public float Speed = 3f;
        public Vector3 targetDirection;
        public Vector3 movingDirection;
        public float speedY = 0;
        float gravity = 4f;


        public bool locallyControllable;
        public bool activated = true;
        public bool IsGrounded;
        public bool doInput;
        public bool isMe;
        public static bool doSim;
        public bool allowedToMove = true;

        public float heightRay = 0.55f;

        public Hitbox itemHitbox;

        public List<Item> itemsInProximity;


        public float currentSpeed;

        int k = 15;


        public List<float> lastSpeeds = new List<float>();

        //Setup References for PlayerController and initial values if necessary
        public void Awake()
        {
            if (Camera.main != null)
                cam = Camera.main.transform;


            controller = transform.GetComponent<CharacterController>();
            SetParams();
        }

        void ComputeCurrentSpeed()
        {
            lastSpeeds.Add(controller.velocity.magnitude);
            if(lastSpeeds.Count>k)
            {
                lastSpeeds.RemoveAt(0);
            }
            float sum = 0;
            for(int i = 0;i<lastSpeeds.Count;i++)
            {
                sum += lastSpeeds[i];
            }
            currentSpeed = sum / k;
        }

        void SetParams()
        {
            heightRay = 1.1f;

            itemsInProximity = new List<Item>();
            itemHitbox.Attach("Item");
            itemHitbox.enterEvent.AddListener((x)=> { itemsInProximity.Add(x.GetComponent<Item>());  });
            itemHitbox.exitEvent.AddListener((x) => { itemsInProximity.Remove(x.GetComponent<Item>()); });
        }

        void Start()
        {
            player = transform.GetComponent<Player>();
        }
        public Collider col;

        public void UpdateGround()
        {
            int mask = 1 << 10;
            RaycastHit hit = new RaycastHit();
            IsGrounded = controller.isGrounded || Physics.Raycast(transform.position, -Vector3.up, out hit, heightRay, mask, QueryTriggerInteraction.Ignore);
            col = hit.collider;
        }



        //Needs to be refined

        void UpdateEquipItem()
        {
            if(player.itemHandles.Length > 0)
            {
                if (player.itemHandles[0] != null)
                {
                    List<Item> pickUpItems = itemsInProximity;
                    foreach(Item i in player.items)
                    {
                        pickUpItems.Remove(i);
                    }
                    if(pickUpItems.Count > 0)
                    {
                        Debug.Log("Swapping of Item");
                        player.itemHandles[0].Dettach();
                        player.itemHandles[0].Attach(pickUpItems[0]);

                    }
                    else
                    {
                        Debug.Log("Dettachment of Item");
                        player.itemHandles[0].Dettach();
                    }
                }
            }
        }

        public void Move()
        {

            if (!IsGrounded && doSim)
            {
                speedY -= gravity * Time.deltaTime;
            }
            if (IsGrounded || !doSim)
            {
                speedY = 0;
            }
            if(!activated) targetDirection = new Vector3(0, 0, 0);

            if (doInput)
            {
                if (cam != null)
                    targetDirection = Vector3.Normalize(Vector3.ProjectOnPlane(cam.right, transform.up) * Input.GetAxisRaw("Horizontal") + Vector3.ProjectOnPlane(cam.forward, transform.up) * Input.GetAxisRaw("Vertical"));

            
                // Pickup closest item
                if(Input.GetKeyDown(KeyCode.G))
                {
                    UpdateEquipItem();
                }

                if (IsGrounded)
                {

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        speedY = 1.5f;
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    player.UseLeft();
                }else
                if (Input.GetMouseButtonDown(1))
                {
                    player.UseRight();
                }
            }

            targetDirection.y = speedY;
            if (targetDirection.sqrMagnitude >= 0.01f)
            {
                movingDirection = targetDirection;
            }
            else
            {
                movingDirection = new Vector3(0, 0, 0);
            }
            controller.Move(targetDirection * Speed * Time.deltaTime);
        }

        

       


        void Update()
        {
            UpdateGround();
            StateUpdate();
            Move();
        }
        void StateUpdate()
        {
            doSim = true;
            doInput = PlayerManager.acceptInput && allowedToMove && activated && locallyControllable; //&& !player.lockNetUpdate;
        }
        
        void FixedUpdate()
        {
            ComputeCurrentSpeed();
        }

        public void OnDisable()
        {
        }
    }
}
