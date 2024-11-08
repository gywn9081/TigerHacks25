using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCController : MonoBehaviour
{
    //public GameObject gameObject;
    public float walkSpeed = 0.75f;
    public float gravity = 10f;
    public float rotationSpeed = 5f; // Speed of rotation

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private float randCounter = 0f;
    private float pauseCounter = 0f;

    private float health = 200f;
    public GameObject gore;
    bool Dead = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Dead) return;

        if(health <= 0)
        {
            Die();
        }

        // Handle gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = 0; // Reset vertical movement when grounded
        }

        // Handle pause
        if (pauseCounter > 0f)
        {
            pauseCounter -= Time.deltaTime;
            return;
        }

        // Handle random movement generation
        if (randCounter <= 0f)
        {
            float randomX = Random.Range(-1f, 1f);
            float randomZ = Random.Range(-1f, 1f);
            moveDirection.x = randomX * walkSpeed;
            moveDirection.z = randomZ * walkSpeed;

            randCounter = Random.Range(1f, 5f); // New random time for next movement
            pauseCounter = Random.Range(0f, 3f); // Pause time after movement
        }

        // Move the NPC
        characterController.Move(moveDirection * Time.deltaTime);

        // Smoothly rotate towards movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Update the random counter
        randCounter -= Time.deltaTime;
    }

    public void TakeDamage(float damageTaken)
    {
        walkSpeed = walkSpeed * (damageTaken/health);
        health -= damageTaken;

    }

    void Die()
    {
        moveDirection.y = 10000;
        moveDirection.x = -10000;
        characterController.Move(moveDirection);
        Dead = true;
        //innocent animals die --> increase in uprising bar
    }
}