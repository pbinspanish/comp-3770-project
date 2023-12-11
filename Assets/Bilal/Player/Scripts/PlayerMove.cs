using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera; //used to move player forward in the direction facing the camera
    private static Rigidbody player; //player rigidbody
    public GameObject prefab;
    private CapsuleCollider playerCollider; //player collider used to check if the player is grounded

    //Input
    private float verticalInput, horizontalInput; //axes
    private bool Run, Jump; //keys
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 30f;
    private float DashingTime = 0.2f;
    private float DashingCooldown = 1f; 
    [SerializeField] private TrailRenderer tr;

    private bool respawning = false;

    [Header("Movement")]
    public bool canMove = false;
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
    private static Vector3 spawnPosition;

    public bool body;
    public bool zombie;
    GameObject deadBody;

    public bool starterDialogue = true;
    public bool hasSword = false;

    public int zombieKill = 0;
    public bool demonDead;

    // Start is called before the first frame update
    void Start()
    {
        hasSword = true;
        canMove = false;
        isDashing = false;
        tr = GetComponentInParent<TrailRenderer>();
        player = GetComponentInParent<Rigidbody>(); //reference
        playerCollider = GetComponentInParent<CapsuleCollider>();
        prefab = transform.parent.gameObject;
        spawnPosition = transform.position;
        Cursor.visible = true; //make sure mouse cursor is visible
    }

    void Update()
    {
        if(isDashing){
            return;
        }
        Debug.Log(isGrounded);
        if (!starterDialogue && !body && !zombie && !respawning)
        {
            GetComponent<Animator>().SetBool("Sleep", false);
            canMove = !GetComponent<DialogueInitiator>().isInConversation;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            respawn();
        }
        

        if(zombieKill == 8)
        {
            Destroy(GameObject.FindGameObjectWithTag("CampWall"));
        }

        if (demonDead)
        {
            GameObject.FindGameObjectWithTag("BossWall").GetComponent<BoxCollider>().enabled = false;
            GameObject.FindGameObjectWithTag("BossWall").GetComponent<MeshRenderer>().enabled = false;
            GetComponent<HP>().health = GetComponent<HP>().maxHealth;
        }

        if (canMove) { getInput(); } //get input if canMove is true
        else{
            currentSpeed = 0;
            player.velocity = Vector3.zero;
        }
        //debug();
    }

    void FixedUpdate()
    {
        if(isDashing){
            return;
        }
        if (canMove) { move(); } //move player if canMove is true
        if (deadBody.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Death") && deadBody.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && body)
        {
            body = false;
            player.transform.position = spawnPosition;
            foreach (var mesh in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                mesh.enabled = true;
            }
            GetComponentInChildren<MeshRenderer>().enabled = true;
            canMove = true;
            respawning = false;
        }
    }

    void move() //change velocity and rotation
    {
        player.velocity = movementDirection * Time.fixedDeltaTime;
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, desiredRotation, rotationSpeed * Time.fixedDeltaTime); //rotate player
    }

    void getInput() //handle all input configuration
    {
        if(isDashing)
        {
            return;
        }
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
        if(!isDashing){
            setRotation();
            setMoveSpeed();
        }
        

        isOnSlope = checkSlope();
        isGrounded = checkGround(); //is the player grounded?
        jump();
        updateGravity();

        //final movement direction
        if(!isDashing){
            movementDirection.x *= currentSpeed;
            movementDirection.y = upVelocity;
            movementDirection.z *= currentSpeed;
        }
       

        if (isOnSlope && !isJumping && isGrounded)
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, slopeHit.normal);
        }
        if(Input.GetKeyDown(KeyCode.Tab) && canDash)
        {
            StartCoroutine(Dash());
        }
        Debug.Log("yup");
    }
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = gravity;
        gravity = 0f;
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        Quaternion faceCamera = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0); //get camera forward facing angle on Y axis
        Vector3 movementInput = faceCamera * new Vector3(horizontalInput, 0, verticalInput); //set player movement facing camera
        movementDirection = movementInput.normalized; //normalizing sets the magnitude to 1
        movementDirection.x *= dashingPower;
        movementDirection.y = 0;
        movementDirection.z *= dashingPower;
        player.velocity = movementDirection;
        tr.emitting = true;
        GetComponent<HP>().ChangeDamage();
        yield return new WaitForSeconds(DashingTime);
        GetComponent<HP>().ChangeDamage();
        tr.emitting = false;
        gravity = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(DashingCooldown);
        canDash = true;

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

    public bool checkGround() //return if the player is grounded
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

    public void respawn()
    {
        respawning = true;
        body = true;
        canMove = false;
        deadBody = Instantiate(gameObject, transform.position, transform.rotation);
        foreach(var mesh in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            mesh.enabled = false;
        }
        GetComponentInChildren<MeshRenderer>().enabled = false;
        deadBody.GetComponent<Animator>().SetBool("isGrounded", true);
        deadBody.GetComponent<Animator>().SetBool("DieBitch", true);
        Destroy(deadBody.GetComponent<PlayerMove>());
        Destroy(deadBody.GetComponent<Rigidbody>());
        Destroy(deadBody.GetComponent<ProjectileLauncher>());
        Destroy(deadBody.GetComponent<Collider>());
        Destroy(deadBody.GetComponent<HP>());
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            spawnPosition = other.transform.position;
        }

        if (other.gameObject.GetComponent<BossWallCheckpoint>() != null)
        {
            GameObject.FindGameObjectWithTag("BossWall").GetComponent<BoxCollider>().enabled = true;
            GameObject.FindGameObjectWithTag("BossWall").GetComponent<MeshRenderer>().enabled = true;
        }

        if (other.gameObject.CompareTag("Respawn"))
        {
            respawn();
        }
        if (other.gameObject.CompareTag("ZombieTrigger"))
        {
            zombie = true;
            canMove = false;
            other.GetComponent<AudioSource>().Play();
            foreach (var zombie in GameObject.FindObjectsOfType(typeof(ZombieAnimator)) as ZombieAnimator[])
            {
                zombie.GetComponent<ZombieAnimator>().WakeZombie();
            }
        }

    }
   

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            GetComponent<HP>().DealDamage(5f);
        }
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
