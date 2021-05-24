using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerController : EntityController
    {
        public static bool doSim;
        public PlayerBaseAnimator playerBaseAnimator;
        public CharacterController controller;
        public Transform cam;
        public float Speed = 3f;
        public Vector3 targetDirection;
        public Vector3 movingDirection;
        public float speedY;


        public bool locallyControllable;
        public bool activated = true;
        public bool IsGrounded;
        public bool takeInput = true;
        public bool doInput;
        public bool isMe;
        public bool allowedToMove = true;

        public float heightRay = 0.55f;


        public float currentSpeed;


        public List<float> lastSpeeds = new List<float>();
        public Collider col;
        private float convergenceSpeed = 0.1f;


        //Needs to be refined

        private Vector3 forwardDirection;
        private readonly float gravity = 4f;

        private readonly int k = 10;


        private Vector3 lastPosition;

        //public static PlayerController currentPlayer;
        private Player player;

        private float turnSmoothVel;

        //Setup References for PlayerController and initial values if necessary
        public void Awake()
        {
            if (Camera.main != null)
                cam = Camera.main.transform;


            controller = transform.GetComponent<CharacterController>();
            SetParams();
            lastPosition = controllerPosition;
        }

        public override void Start()
        {
            base.Start();
            player = transform.GetComponent<Player>();
        }


        public override void Update()
        {
            UpdateGround();
            StateUpdate();

            playerBaseAnimator.speed = currentSpeed / 3;
            Move();
            base.Update();
        }

        private void FixedUpdate()
        {
            ComputeCurrentSpeed();
        }

        public void OnDisable()
        {
        }

        private void ComputeCurrentSpeed()
        {
            lastSpeeds.Add((transform.position - lastPosition).magnitude / Time.deltaTime);
            if (lastSpeeds.Count > k) lastSpeeds.RemoveAt(0);
            float sum = 0;
            for (var i = 0; i < lastSpeeds.Count; i++) sum += lastSpeeds[i];
            currentSpeed = sum / k;
            lastPosition = transform.position;
        }

        private void SetParams()
        {
            heightRay = 1.1f;
        }

        public void UpdateGround()
        {
            var mask = 1 << 10;
            var hit = new RaycastHit();
            IsGrounded = controller.isGrounded || Physics.Raycast(transform.position, -Vector3.up, out hit, heightRay,
                mask, QueryTriggerInteraction.Ignore);
            col = hit.collider;
        }

        public void Move()
        {
            if (!IsGrounded && doSim) speedY -= gravity * Time.deltaTime;
            if (IsGrounded || !doSim) speedY = 0;
            targetDirection = new Vector3(0, 0, 0);

            if (doInput && takeInput)
            {
                if (cam != null)
                    targetDirection =
                        Vector3.Normalize(
                            Vector3.ProjectOnPlane(cam.right, transform.up) * Input.GetAxisRaw("Horizontal") +
                            Vector3.ProjectOnPlane(cam.forward, transform.up) * Input.GetAxisRaw("Vertical"));


                // Pickup closest item
                if (Input.GetKeyDown(KeyCode.G)) player.Invoke(player.UpdateEquipItem);

                if (Input.GetKeyDown(KeyCode.G))
                {
                    // INVOKE
                    // INTERACT WITH ENVIRONMENT (READ TEXT, PRESS BUTTON etc)
                }

                if (IsGrounded)
                    if (Input.GetKeyDown(KeyCode.Space))
                        speedY = 1.5f;
                if (Input.GetMouseButton(0))
                    //INVOKE
                    player.Invoke(player.UseLeft, true);
                else if (Input.GetMouseButton(1))
                    //INVOKE
                    player.Invoke(player.UseRight, true);
                if (Input.GetMouseButtonUp(1)) player.Invoke(player.StopUseRight, true);
            }

            targetDirection.y = speedY;
            if (targetDirection.sqrMagnitude >= 0.01f)
                movingDirection = targetDirection;
            else
                movingDirection = new Vector3(0, 0, 0);

            // THIS IS  JANK BUT WORKS
            if ((Level.instantiateType == Level.InstantiateType.Test) || (Level.instantiateType == Level.InstantiateType.Online))
                controller.Move(targetDirection * Speed * Time.deltaTime);


            if (currentSpeed > 0) forwardDirection = (forwardDirection + movingDirection) / 2;

            var angleY = 180 + 180 / Mathf.PI * Mathf.Atan2(forwardDirection.x, forwardDirection.z);

            transform.eulerAngles = new Vector3(0, angleY, 0);
        }

        private void StateUpdate()
        {
            doSim = true;
            doInput = PlayerManager.acceptInput && allowedToMove && activated &&
                      locallyControllable; //&& !player.lockNetUpdate;
        }
    }
}