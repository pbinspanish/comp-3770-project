using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))] //automatically adds CharacterController when attaching this script to an object
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    private CharacterController characterController;

    [Header("Mouse Cursor")]
    public bool showMouse = true;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    private float currentSpeed;
    private float rotationSpeed = 10f;
    
    [Header("Physics")]
    public float fallRate = 1f;
    public float jumpHeight = 7f;
    private float gravity = -9.81f;
    private bool isGrounded; //is the player on the ground?
    private float upVelocity; //controls the up vector of player movement (positive to jump, negative to fall)

    //animation variables
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isJumping;

    private Quaternion desiredRotation;
    [HideInInspector] public Vector3 movementDirection = Vector3.zero; //vector that controls movement

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>(); //reference
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.visible = showMouse ? true : false;
        Movement();
    }

    void Movement()
    {
        //movement variables
        bool Run = Input.GetKey(KeyCode.LeftShift);
        bool Jump = Input.GetKey(KeyCode.Space);

        //get movement input axes
        float verticalInput = Input.GetAxisRaw("Vertical"); //"W" & "S" keys
        float horizontalInput = Input.GetAxisRaw("Horizontal"); //"A" & "D" keys

        //calculate movement direction to move forward facing the camera (isometric)
        Vector3 movementInput = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0) * new Vector3(horizontalInput, 0, verticalInput);
        movementDirection = movementInput.normalized; //normalizing ensures a magnitude of 1

        if (showMouse)
        {
            Ray mouse = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(mouse, out var hit))
            {
                Vector3 mouseHit = hit.point;
                Vector3 offset = transform.position + 1.5f * Vector3.up;
                Vector3 direction = mouseHit - offset;
                desiredRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }
        }
        else
        {
            desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up); //calculate desired rotation to face movement direction
        }

        isMoving = (movementDirection != Vector3.zero) ? true : false; //is the player moving?
        if (isMoving)
        {
            if (Run)
            {
                currentSpeed = runSpeed;
                isRunning = true;
                isWalking = false;
            }
            else
            {
                currentSpeed = walkSpeed;
                isWalking = true;
                isRunning = false;
            }
        }

        isGrounded = characterController.isGrounded; //is the player on the ground?

        if (Jump && isGrounded) //jump
        {
            upVelocity = jumpHeight;
            isJumping = true;
        }
        //handle gravity physics
        if (isGrounded && upVelocity < 0)
        {
            upVelocity = 0f;
        }
        if (!isGrounded)
        {
            upVelocity += gravity * fallRate * Time.deltaTime;
        }

        movementDirection.y = upVelocity;
        movementDirection.x *= currentSpeed;
        movementDirection.z *= currentSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime); //rotate player towards movement direction (desired rotation)
        characterController.Move(movementDirection * Time.deltaTime); //move player
    }
}
