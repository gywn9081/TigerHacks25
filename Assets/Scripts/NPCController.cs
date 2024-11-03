using System.Collections;
using System.Collections.Generic;
using static System.Math;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCController : MonoBehaviour
{
    public float walkSpeed = 0.5f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    //float rotationX = 0;
    public bool canMove = true;

    CharacterController characterController;
    public float randCounter = 0f;
    public float pauseCounter = 0f;
    public bool doneMoving = false;
    
void Start()
{
    characterController = GetComponent<CharacterController>();
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

void Update()
{
    #region Handles falling
    while(!characterController.isGrounded)
    {
        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }
    #endregion

    if(pauseCounter > 0f)
    {

        pauseCounter -= Time.deltaTime;
        return;

    }

    if( (randCounter) <= 0f)
    {

            #region New random movement generation

            float random = (Random.Range(0f, 1f)) - Random.Range(0f, 1f);
            randCounter = Random.Range(0f,5f);
            if(pauseCounter <= 0) pauseCounter = Random.Range(0f,3f);

            #endregion

            #region Handles Movement
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            float curSpeedX = canMove ? (walkSpeed * (random)) : 0;
            float curSpeedY = canMove ? (walkSpeed * (random)) : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
            #endregion

            if(canMove)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);    
            }

    }
        #region Moves NPC
        characterController.Move(moveDirection * (Time.deltaTime));
        #endregion
    
    randCounter -= Time.deltaTime;
    
}
}