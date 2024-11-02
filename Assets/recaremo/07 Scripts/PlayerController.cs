using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace recaremo
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Put here the rigidbody of the player")]
        [SerializeField] private Rigidbody playerRb;
        [SerializeField] private Transform playerHead;
        [SerializeField] private Camera playerCamera;

        [Header("Configurations")]
        [SerializeField] private float playerWalkSpeed;
        [SerializeField] private float playerRunSpeed;
        [SerializeField] private float playerJumpSpeed;
        [SerializeField] private float impactThreshold;
        [SerializeField] private float playerNewHeight;

        [Header("Camera Effects")]
        [SerializeField] private float baseCameraFOV = 60f;
        [SerializeField] private float baseCameraHight = 0.85f;

        [SerializeField] private float walkBobbingRate = 0.75f;
        [SerializeField] private float runBobbingRate = 1f;
        [SerializeField] private float maxWalkBobbingOffset = 0.2f;
        [SerializeField] private float maxRunBobbingOffset = 0.3f;

        [SerializeField] private float cameraShakeTreshold = 10f;
        [SerializeField][Range(0f, 0.03f)] private float cameraShakeRate = 0.015f;
        [SerializeField] private float maxVerticalFallShakeAngle = 40f;
        [SerializeField] private float maxHorizonzalFallShakeAngle = 40f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioWalk;

        [Header("Runtime")]
        bool playerIsGrounded = false;
        bool playerIsJumping = false;
        float vyCache;
        Vector3 newVelocity;



        // Start is called before the first frame update
        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Vector3 playerHeigth = transform.localScale;
            transform.localScale = new Vector3(playerHeigth.x, playerHeigth.y * playerNewHeight / 2f, playerHeigth.z);
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(2f * Input.GetAxis("Mouse X") * Vector3.up);

            newVelocity = Vector3.up * playerRb.linearVelocity.y;
            float speed = Input.GetKey(KeyCode.LeftShift) ? playerRunSpeed : playerWalkSpeed;
            newVelocity.x = Input.GetAxis("Horizontal") * speed;
            newVelocity.z = Input.GetAxis("Vertical") * speed;

            if (playerIsGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space) && !playerIsJumping)
                {
                    newVelocity.y = playerJumpSpeed;
                    playerIsJumping = true;
                }
            }

            bool isMovingOnGround = (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f) && playerIsGrounded;

            if (isMovingOnGround)
            {
                float bobbingRate = Input.GetKey(KeyCode.LeftShift) ? runBobbingRate : walkBobbingRate;
                float bobbingOffset = Input.GetKey(KeyCode.LeftShift) ? maxRunBobbingOffset : maxWalkBobbingOffset;
                Vector3 targetHeadPosition = Vector3.up * baseCameraHight + Vector3.up * (Mathf.PingPong(Time.time * bobbingRate, bobbingOffset) - bobbingOffset * 0.5f);
                playerHead.localPosition = Vector3.Lerp(playerHead.localPosition, targetHeadPosition, 0.1f);
            }

            playerRb.linearVelocity = transform.TransformDirection(newVelocity);

            // Audio
            audioWalk.enabled = isMovingOnGround;
            audioWalk.pitch = Input.GetKey(KeyCode.LeftShift) ? 1.75f : 1f;
        }


        void FixedUpdate()
        {
            //if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f))
            //{
            //    playerIsGrounded = true;
            //}
            //else playerIsGrounded = false;

            vyCache = playerRb.linearVelocity.y;
        }

        void LateUpdate()
        {
            Vector3 e = playerHead.eulerAngles;
            e.x -= Input.GetAxis("Mouse Y") * 2f;
            e.x = RestrictAngle(e.x, -85f, 85f);
            playerHead.eulerAngles = e;

            // FOV
            float fovOffset = (playerRb.linearVelocity.y < 0f) ? Mathf.Sqrt(Mathf.Abs(playerRb.linearVelocity.y)) : 0f;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, baseCameraFOV + fovOffset, 0.25f);

            // Fall Effects
            if (!playerIsGrounded && Mathf.Abs(playerRb.linearVelocity.y) >= cameraShakeTreshold)
            {
                Vector3 newAngle = playerHead.localEulerAngles;
                newAngle += Vector3.right * Random.Range(-maxVerticalFallShakeAngle, maxVerticalFallShakeAngle);
                newAngle += Vector3.up * Random.Range(-maxHorizonzalFallShakeAngle, maxHorizonzalFallShakeAngle);
                playerHead.localEulerAngles = Vector3.Lerp(playerHead.localEulerAngles, newAngle, cameraShakeRate);

            }
            else
            {
                e = playerHead.eulerAngles;
                e.y = 0f;
                playerHead.localEulerAngles = e;
            }
        }

        public static float RestrictAngle(float angle, float angleMin, float angleMax)
        {
            if (angle > 180) angle -= 360;
            else if (angle < -180) angle += 360;

            if (angle > angleMax) angle = angleMax;
            if (angle < angleMin) angle = angleMin;

            return angle;
        }

        void OnCollisionStay(Collision col)
        {
            playerIsGrounded = true;
            playerIsJumping = false;
        }

        void OnCollisionExit(Collision col)
        {
            playerIsGrounded = false;
        }

        void OnCollisionEnter(Collision col)
        {
            if (Vector3.Dot(col.GetContact(0).normal, Vector3.up) < 0.5f)
            {
                if (playerRb.linearVelocity.y < -5f)
                {
                    playerRb.linearVelocity = Vector3.up * playerRb.linearVelocity.y;
                    return;
                }
            }

            float acceleration = (playerRb.linearVelocity.y - vyCache) / Time.fixedDeltaTime;
            float impactForce = playerRb.mass * Mathf.Abs(acceleration);

            if (impactForce >= impactThreshold)
            {
                Debug.Log("Fall Damage!");
            }
        }
    }
}
