using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CommieController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 0.75f;
    public float runSpeed = 5f;
    public float jumpPower = 3f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    public bool canMove = true;
    bool Dead = false;
    public float health = 300;
    public Animator animator;
    CharacterController characterController;

    public int returnSpeed = 200;

void CheckPlayer()
{
    if (transform.position.y < -100) {
        characterController.enabled = false;
        //characterController.transform.position = new Vector3(10f, 10f, 35f);
        characterController.enabled = true;
    }
}

void MoveTowards(Vector3 location) {
    var cc = GetComponent<CharacterController>();
    var offset = location - transform.position;
    //Get the difference.
    if(offset.magnitude > 1f) {
    //If we're further away than .1 unit, move towards the target.
    //The minimum allowable tolerance varies with the speed of the object and the framerate. 
    // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
        offset = offset.normalized * returnSpeed;
        //normalize it and account for movement speed.
        cc.Move(offset * Time.deltaTime);
        //actually move the character.
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
    if(Dead) return;

    #region Handles Movement
    Vector3 forward = transform.TransformDirection(Vector3.forward);
    Vector3 right = transform.TransformDirection(Vector3.right);

    //Press left shift to run
    bool isRunning = Input.GetKey(KeyCode.LeftShift);
    float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
    float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
    float movementDirectionY = moveDirection.y;
    moveDirection = (forward * curSpeedX) + (right * curSpeedY);
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

    void TakeDamage(float damageTaken)
    {
        walkSpeed = walkSpeed * (health/(health-damageTaken));
        health -= damageTaken;

    }

    void Die()
    {
        Dead = true;
        //Method to either decrease uprising meter or farmer win (based on player count)
    }

    CheckPlayer();
}
}