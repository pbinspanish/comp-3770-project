using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera; //used to move player forward in the direction facing the camera
    private Rigidbody player; //player rigidbody
    private CapsuleCollider playerCollider; //player collider used to check if the player is grounded

    //Input
    private float verticalInput, horizontalInput; //axes
    private bool Run, Jump; //keys

    [Header("Movement")]
    public bool canMove = true;
    public float walkSpeed = 100f;
    public float runSpeed = 200f;
    private float currentSpeed;
    private float rotationSpeed = 10f;
    public Vector3 movementDirection; //controls movement
    Quaternion desiredRotation; //controls rotation

    //Physics
    public bool isGrounded;
    private bool isOnSlope;
    private RaycastHit slopeHit;
    private float gravity = -9.81f;
    private float gravityRate = 40f;
    private float jumpHeight = 200f;
    private float jumpCap = 2f; //double jump
    private float jumpCount = 0f;
    private bool jumpAgain;
    private bool isJumping;
    private float upVelocity; //controls jumping and gravity
    
    //debugging
    private UnityEngine.Color color;

    public static Vector3 mouseHit;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Rigidbody>(); //reference
        playerCollider = GetComponentInParent<CapsuleCollider>();
        Cursor.visible = true; //make sure mouse cursor is visible
    }

    void Update()
    {
        if (canMove) { getInput(); } //get input if canMove is true
        //debug();
    }

    void FixedUpdate()
    {
        if (canMove) { move(); } //move player if canMove is true
    }

    void move() //change velocity and rotation
    {
        player.velocity = movementDirection * Time.fixedDeltaTime;
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, desiredRotation, rotationSpeed * Time.fixedDeltaTime); //rotate player
    }

    void getInput() //handle all input configuration
    {
        //movement axes
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        //movement keys
        Run = Input.GetKey(KeyCode.LeftShift);
        Jump = Input.GetKey(KeyCode.Space);

        //move facing camera
        Quaternion faceCamera = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0); //get camera forward facing angle on Y axis
        Vector3 movementInput = faceCamera * new Vector3(horizontalInput, 0, verticalInput); //set player movement facing camera
        movementDirection = movementInput.normalized; //normalizing sets the magnitude to 1

        setRotation();
        setMoveSpeed();

        isOnSlope = checkSlope();
        isGrounded = checkGround(); //is the player grounded?
        jump();
        updateGravity();

        //final movement direction
        movementDirection.x *= currentSpeed;
        movementDirection.y = upVelocity;
        movementDirection.z *= currentSpeed;

        if (isOnSlope && !isJumping && isGrounded)
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, slopeHit.normal);
        }
    }

    void setRotation() //player rotates to face mouse cursor
    {
        Ray mouse = playerCamera.ScreenPointToRay(Input.mousePosition); //get mouse position by casting a ray in the world
        if (Physics.Raycast(mouse, out var hit))
        {
            mouseHit = hit.point; //get the point where the ray hit something
            Vector3 direction = mouseHit - player.transform.position; //direction to rotate is the point where the mouse hit offsetted by the player's position
            desiredRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)); //set player's desired rotation ignoring the up axis to avoid tilting
        }
    }

    void setMoveSpeed() //set walking and running speed
    {
        if (!Run)
        {
            currentSpeed = walkSpeed;
        }
        if (Run && isGrounded)
        {
            currentSpeed = runSpeed;
        }
    }

    void jump() //handle jumping height and times allowed to jump
    {
        if (Jump && isGrounded)
        {
            upVelocity = jumpHeight;
            jumpCount = 1f;
            isJumping = true;
        }
        if (!isGrounded && !Jump && (jumpCount < jumpCap))
        {
            jumpAgain = true;
        }
        if (Jump && jumpAgain)
        {
            upVelocity = jumpHeight;
            jumpCount++;
            jumpAgain = false;
        }
        if (isGrounded && !Jump)
        {
            jumpCount = 0f;
            jumpAgain = false;
            isJumping = false;
        }
    }

    void updateGravity() //handle manual gravity because RigidBody gravity is disabled when using velocity
    {
        if (!isGrounded)
        {
            upVelocity += gravity * gravityRate * Time.deltaTime;
        }
        if (isGrounded && upVelocity < 0f)
        {
            upVelocity = 0f;
        }
    }

    bool checkGround() //return if the player is grounded
    {
        Vector3 origin = playerCollider.bounds.center; //origin of the ray is the center of the player collider
        float distance = playerCollider.bounds.extents.y + 0.01f; //cast the ray to a distance of half the player collider + 0.01f error handler
        if (isOnSlope)
        {
            return Physics.Raycast(origin, -slopeHit.normal, distance);
        }
        return Physics.Raycast(origin, Vector3.down, distance); //cast ray from center of player collider to its bottom
    }

    bool checkSlope() //return if the player is on a sloped surface
    {
        if (Physics.Raycast(playerCollider.bounds.center, Vector3.down, out slopeHit, playerCollider.bounds.extents.y + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    void debug()
    {
        color = isGrounded ? UnityEngine.Color.red : UnityEngine.Color.green; //red ray if grounded, green ray if not grounded
        Debug.DrawRay(playerCollider.bounds.center, Vector3.down * playerCollider.bounds.extents.y, color); //dray casted ray
        Debug.Log(checkSlope());
    }

    // slow debuff - Projectile Launcher
    float speedMultiple = 100;
    float tSlowEnd;
    public void Slow(float _speedMultiple, float sec)
    {
        if (_speedMultiple < speedMultiple) //only apply the stronger slow
        {
            speedMultiple = Mathf.Clamp(_speedMultiple, 0, 100);
            tSlowEnd = Time.time + sec;
        }
    }
}
