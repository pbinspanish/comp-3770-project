using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;


public class PlayerController : MonoBehaviour
{

     // get input and control PlayerChara

     bool enableMoveInput = true; //lose control when eg. knocked away / stunned
     Camera cam;


     void Start()
     {
          cam = Camera.main;
     }
     void Update()
     {
          if (PlayerChara.mine != null && !specMode)
          {
               UpdateInput();
          }
     }
     void FixedUpdate()
     {
          if (PlayerChara.mine != null && !specMode)
          {
               UpdatePosition();
               UpdateRotation();
          }
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

     Vector3 vel;
     Rigidbody rb { get => PlayerChara.mine.rb; }

     float acc { get => PlayerStatus.inst.acc; }
     float speedCap { get => PlayerStatus.inst.speedCap; }
     float speedCapRun { get => PlayerStatus.inst.speedCapRun; }
     float speedCapAirborne { get => PlayerStatus.inst.speedCapAirborne; }
     float jumpForce { get => PlayerStatus.inst.jumpForce; }
     bool isGrounded { get => true; set { } } //TODO
     float gravity { get => PlayerStatus.inst.gravity; }
     float G { get => PlayerStatus.inst.G; }


     void UpdatePosition()
     {
          // XZ: vertical verticle
          vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

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

          // clamp XZ
          float _clamp = isGrounded ? (isRunning ? speedCapRun : speedCap) : speedCapAirborne;
          vel = Vector3.ClampMagnitude(vel, _clamp);


          // Y: verticle velocity, it's not cap by max speed
          if (inputY != 0 && isGrounded)
          {
               rb.velocity = new Vector3(vel.x, 0, vel.z);
               rb.AddForce(inputY * jumpForce * Vector3.up, ForceMode.Impulse);

               inputY = 0; //comsume input
               isGrounded = false;
          }
          else
          {
               rb.velocity = new Vector3(vel.x, rb.velocity.y - gravity * G * Time.fixedDeltaTime, vel.z);
          }

          _vel = rb.velocity;
     }

     public Vector3 _vel;

     // rotation ---------------------------------------------------------------------

     public static Vector3 mouseHit;
     float rotateSpeed { get => PlayerStatus.inst.rotateSpeed; }

     void UpdateRotation()
     {
          var player = PlayerChara.mine;
          var ray = cam.ScreenPointToRay(Input.mousePosition);

          if (Physics.Raycast(ray, out var hit))
          {
               mouseHit = hit.point;

               var me = player.transform.position + 1.5f * Vector3.up;
               var dir = hit.point - me;
               var rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)); //remove up/down tilt
               player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, rot, rotateSpeed * Time.deltaTime);
          }
     }



     // spectator camera ---------------------------------------------------------------------

     bool specMode = false;

     public void ToggleSpectatorMode()
     {
          specMode = !specMode;
          CameraMgr.inst.SetSpectatorMode(specMode);
     }

}