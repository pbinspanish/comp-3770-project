using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//automatically add CharacterController when attaching this script to an object
[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    //references
    private CharacterController characterController;
    private Animator playerAnimator;

    [Header("Camera & Mouse Cursor")]
    public Camera playerCamera;
    public bool showMouseCursor = true;

    //movement speed
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    private float currentSpeed;
    private float rotationSpeed = 10f;
    
    //gravity
    [Header("Physics")]
    public float gravity = -9.81f;
    public float gravityMultiplier = 3f;
    //jump height
    public float jumpHeight = 7f;

    //used for gravity(falling) and jumping
    private float upVelocity;

    //vector that controls movement
    private Vector3 movementDirection = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //references
        characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponent<Animator>();
        
        //lock cursor and set invisible
        if (!showMouseCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        //get movement input axes
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        //checks if the player is touching the ground
        bool isGrounded = characterController.isGrounded;

        //is the player moving? use for animation triggers
        bool playerMoving = false;

        //calculate movement input to move forward facing the camera
        Vector3 movementInput = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0) * new Vector3(horizontalInput, 0, verticalInput);
        //normalize movement vector to ensure it has a magnitude of 1
        movementDirection = movementInput.normalized;

        //check if the player is moving
        if (movementDirection != Vector3.zero)
        {
            playerMoving = true;

            //rotate the player forward in the direction he is moving
            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            
            //activate walk animation
            playerAnimator.SetBool("isWalking", true);
        }
        else
        {
            playerMoving = false;

            //deactivate walk animation
            playerAnimator.SetBool("isWalking", false);
        }

        //input actions
        bool Run = Input.GetKey(KeyCode.LeftShift);
        bool isRunning = false;
        bool Jump = Input.GetKey(KeyCode.Space);
        //bool isJumping = false;

        //handle walking or running speed
        if (Run && !isRunning && playerMoving)
        {
            currentSpeed = runSpeed;
            isRunning = true;
            
            //activate run animation
            playerAnimator.SetBool("isRunning", true);
        }
        else
        {
            currentSpeed = walkSpeed;
            isRunning = false;
            
            //deactivate run animation
            playerAnimator.SetBool("isRunning", false);
        }

        //handle gravity physics
        if (isGrounded && upVelocity < 0)
        {
            upVelocity = -1f;
        }
        if (!isGrounded)
        {
            upVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        
        //calculate up vector (gravity or jumping)
        movementDirection.y = upVelocity;

        //movement speed only affects moving forward or right. should not affect up vector
        movementDirection.x *= currentSpeed;
        movementDirection.z *= currentSpeed;
        
        //move player
        characterController.Move(movementDirection * Time.deltaTime);
    }
}
