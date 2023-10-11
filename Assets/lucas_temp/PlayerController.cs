using Unity.Netcode;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
     // get input and control PlayerChara

     // public
     public static Vector3 mouseHit { get; private set; }
     public bool enableMoveInput = true; //lose control when eg. knocked away / stunned

     // private
     Rigidbody rb { get => NetworkChara.myChara.rb; }
     Camera cam;


     void Start()
     {
          cam = Camera.main;
     }
     void Update()
     {
          if (NetworkChara.myChara != null)
          {
               if (Input.GetKeyDown(KeyCode.Alpha3)) Blink(); //TEST

               UpdateInput();
          }
     }
     void FixedUpdate()
     {
          if (NetworkChara.myChara != null)
          {
               UpdateRotation();
               UpdatePosition();
          }
     }



     // TEST ---------------------------------------------------------------------

     float blinkDist = 20;
     void Blink()
     {
          //var rot = PlayerChara.me.transform.rotation.eulerAngles;
          //rot.y = 0; //blink in XZ plane
          //var dir = Quaternion.Euler(rot) * Vector3.forward;
          //PlayerChara.me.transform.position += dir.normalized * blinkDist;

          NetworkChara.myChara.transform.position += NetworkChara.myChara.transform.rotation * Vector3.forward * blinkDist;

          Debug.Log("BLINK ");
     }


     // public ---------------------------------------------------------------------------------

     public float tCapSpeed;
     public float capSpeed;

     public void CapSpeed(float speed, float sec)
     {
          capSpeed = speed;
          tCapSpeed = Time.time + sec;
     }


     // input ---------------------------------------------------------------------

     float inputX; //left right
     float inputZ; //forward
     float inputY; //jump
     bool isRunning = false;

     void UpdateInput()
     {
          if (enableMoveInput)
          {
               inputX = Input.GetAxisRaw("Horizontal");
               inputZ = Input.GetAxisRaw("Vertical");
               isRunning = Input.GetKey(KeyCode.LeftShift);
               if (inputY == 0) inputY = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;
          }
          else
          {
               inputX = 0;
               inputZ = 0;
               isRunning = false;
               inputY = 0;
          }
     }


     // move ---------------------------------------------------------------------
     CharaStatus setting { get => CharaStatus.singleton; }
     float acc { get => setting.acc; }
     float speedCap { get => setting.speedCap; }
     float speedCapRun { get => setting.speedCapRun; }
     float speedCapAirborne { get => setting.speedCapAirborne; }
     float jumpForce { get => setting.jumpForce; }
     float rotateSpeed { get => setting.rotateSpeed; }

     float gravity = 9.8f;

     bool isGrounded { get => true; set { } } //TODO


     void UpdatePosition()
     {
          // XZ move
          var vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

          if (cam != null)
          {
               var h = inputX * cam.transform.right;
               var v = inputZ * cam.transform.forward;
               var dir = v + h;

               vel += dir.normalized * acc * Time.fixedDeltaTime;
          }
          else
          {
               vel += new Vector3(inputX, 0, inputZ).normalized * acc * Time.fixedDeltaTime;
          }


          // cap XZ speed
          float clamp = (Time.time < tCapSpeed) ? capSpeed : isGrounded ? (isRunning ? speedCapRun : speedCap) : speedCapAirborne;
          vel = Vector3.ClampMagnitude(vel, clamp);


          // Y: verticle velocity, it's not cap by max speed
          if (inputY != 0 && isGrounded)
          {
               rb.velocity = new Vector3(vel.x, 0, vel.z);
               rb.AddForce(inputY * jumpForce * Vector3.up, UnityEngine.ForceMode.Impulse);

               inputY = 0; //comsume input
               isGrounded = false;
          }
          else
          {
               rb.velocity = new Vector3(vel.x, rb.velocity.y - gravity * Time.fixedDeltaTime, vel.z);
          }
     }

     void UpdateRotation()
     {
          var player = NetworkChara.myChara;
          var ray = cam.ScreenPointToRay(Input.mousePosition);

          if (Physics.Raycast(ray, out var hit))
          {
               mouseHit = hit.point;

               var me = player.transform.position + 1.5f * Vector3.up;
               var dir = hit.point - me;
               var rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)); //remove up/down tilt
               //player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, rot, rotateSpeed * Time.deltaTime);
               player.transform.rotation = Quaternion.Slerp(player.transform.rotation, rot, rotateSpeed * Time.deltaTime);
          }
          else
          {
               //Debug.Log("no hit"); no rotation
          }
     }



}