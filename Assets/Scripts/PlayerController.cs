using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    public bool canMove = true;
    public Animator animator;
    CharacterController characterController;

    public int returnSpeed = 200;

void CheckPlayer()
{
    if (transform.position.y < -100) {
        characterController.enabled = false;
        characterController.transform.position = new Vector3(10f, 10f, 35f);
        characterController.enabled = true;
    }
}


void Start()
{
    animator = GetComponent<Animator>();
    animator.SetBool("isMoving", false);
    characterController = GetComponent<CharacterController>();
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

void Update()
{
    #region Handles Movement
    Vector3 forward = transform.TransformDirection(Vector3.forward);
    Vector3 right = transform.TransformDirection(Vector3.right);

    //Press left shift to run
    bool isRunning = Input.GetKey(KeyCode.LeftShift);
    float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
    float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
    float movementDirectionY = moveDirection.y;
    moveDirection = (forward * curSpeedX) + (right * curSpeedY);
    if (moveDirection != Vector3.zero)
    {
        animator.SetBool("isMoving", true);
        if (isRunning == true)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
    else
    {
        animator.SetBool("isMoving", false);
    }

    #endregion

    #region Handles jumping

    if(Input.GetButton("Jump") && canMove && characterController.isGrounded)
    {
        moveDirection.y = jumpPower; 
    }
    else
    {
        moveDirection.y = movementDirectionY;
    }

    if(characterController.isGrounded == true)
    {
        animator.SetBool("isJumping", false);
    }
    else
    {
        if(isRunning == true)
        {
            animator.SetBool("isRunJumping", true);
        }
        else
        {
            animator.SetBool("isRunJumping", false);
            animator.SetBool("isJumping", true);
        }
    }

    if(!characterController.isGrounded)
    {
        moveDirection.y -= gravity * Time.deltaTime;
    }

    #endregion

    #region handles rotation
    characterController.Move(moveDirection * Time.deltaTime);

    if(canMove)
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        #endregion    
    }
    CheckPlayer();
}
}