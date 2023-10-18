using TMPro;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(PlayerStatus))]
public class PlayerController : MonoBehaviour
{
     // get input and control PlayerChara

     // public
     public static Vector3 mouseHit { get; private set; } //if mouse ray didn't hit anything, pretend it hit the skybox
     public bool enableMoveInput = true; //lose control when eg. knocked away / stunned
     public string groundLayer = "Default";
     public float groundedSphereCastRadius = 0.8f; //smaller then chara so bumping into wall don't count

     // private
     // Rigidbody rb { get => NetworkChara.myChara.rb; }
     // Collider col { get => NetworkChara.myChara.col; }
     Rigidbody rb;
     Collider col;
     Camera cam;


     void Start()
     {
          cam = Camera.main;

          if (NetworkChara.myChara != null)
          {
               rb = NetworkChara.myChara.rb;
               col = NetworkChara.myChara.col;
          }
          else
          {
               rb = GetComponentInChildren<Rigidbody>();
               col = GetComponentInChildren<Collider>();

               if (rb == null)
                    rb = gameObject.AddComponent<Rigidbody>();
               rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //freeze rotation


               if (col == null)
                    col = gameObject.AddComponent<CapsuleCollider>();
               var capsule = col as CapsuleCollider;
               capsule.center = new Vector3(0, 1, 0);
               capsule.height = 2;
          }
     }
     void Update()
     {
          if (rb != null && col != null)
          {
               if (Input.GetKeyDown(KeyCode.Alpha3)) Blink(); //TEST
               UpdateInput();
          }
          else
          {
               Debug.LogError("");
          }
     }

     void FixedUpdate()
     {
          if (rb != null && col != null)
          {
               //reset flag first
               flag_updateIsGrounded = false;

               UpdateRotation();
               UpdatePosition();
               UpdateJump();
          }
          else
          {
               Debug.LogError("");
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



     // input ----------------------------------------------------------------------------------
     float inputX; //left right
     float inputZ; //forward
     bool inputJump;
     bool isRunning = false;

     void UpdateInput()
     {
          if (enableMoveInput)
          {
               inputX = Input.GetAxisRaw("Horizontal");
               inputZ = Input.GetAxisRaw("Vertical");
               isRunning = Input.GetKey(KeyCode.LeftShift);
               if (!inputJump) inputJump = Input.GetKeyDown(KeyCode.Space);
          }
          else
          {
               inputX = 0;
               inputZ = 0;
               isRunning = false;
               inputJump = false;
          }
     }


     // move -------------------------------------------------------------------------------------
     PlayerStatus status { get => PlayerStatus.singleton; }
     float acc { get => isGrounded ? status.acc : status.accAirborne; }
     float maxValocity { get => isRunning ? status.maxValocityRun : status.maxValocity; }
     float jumpVelocity { get => status.jumpVelocity; }
     float rotateLerp { get => status.rotateLerp; }
     int jumpCount { get => status.jumpCount; }
     int jumpRemain;

     void UpdatePosition()
     {
          // XZ = velocity on the ground
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


          //apply
          float clamp = (Time.time < tCapSpeed) ? capSpeed : maxValocity;
          vel = Vector3.ClampMagnitude(vel, clamp);
          rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
     }

     void UpdateJump()
     {
          if (isGrounded)
               jumpRemain = jumpCount;

          if (inputJump)
          {
               inputJump = false; //comsume input
               if (jumpRemain > 1)
               {
                    jumpRemain--;
                    isGrounded = false;

                    rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z); //reset old Y velocity
               }
          }

          BeSlippery(!isGrounded); //prevent sticking to the wall in air
     }

     void UpdateRotation()
     {
          var ray = cam.ScreenPointToRay(Input.mousePosition);
          var me = rb.transform.position;

          if (Physics.Raycast(ray, out var hit))
          {
               mouseHit = hit.point;
          }
          else
          {
               mouseHit = ray.origin + ray.direction * 1000f; //pretend we hit the skybox

               //this does not equal to raycast, since player/camera pos/rot are all different
               //but the behaviour is entertaining, as if player suddenly look into some far far away place
          }

          var rot = Quaternion.LookRotation(new Vector3(mouseHit.x - me.x, 0, mouseHit.z - me.z)); //remove y tilt
          rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, rot, rotateLerp * Time.deltaTime);

     }


     // slow debuff  ---------------------------------------------------------------------------------
     float capSpeed;
     float tCapSpeed;
     public void CapSpeed(float speed, float sec)
     {
          capSpeed = speed;
          tCapSpeed = Time.time + sec;
     }


     // check if grounded ----------------------------------------------------------------------------

     bool isGrounded { get => UpdateIsGrounded(); set => _isGrounded = value; }
     bool _isGrounded;

     float _small = 0.01f;
     bool flag_updateIsGrounded;


     bool UpdateIsGrounded()
     {
          if (!flag_updateIsGrounded)
          {
               flag_updateIsGrounded = true;

               var origin = rb.transform.position + Vector3.up * (groundedSphereCastRadius + _small); //+a small number to avoid sphere touching the ground initially (it will ignore these obj)
               var dist = groundedSphereCastRadius + _small * 2; //add 1 back, add another 1 as allowed error
                                                                 // var dist =  _small * 2; //add 1 back, add another 1 as allowed error

               _isGrounded = Physics.SphereCast(origin, groundedSphereCastRadius, Vector3.down, out _, dist, LayerMask.GetMask(groundLayer));
          }

          return _isGrounded;
     }


     // prevent sticking to wall in air ----------------------------------------------------------------------------

     void BeSlippery(bool slip)
     {
          col.material.dynamicFriction = slip ? 0 : 0.6f;
          col.material.staticFriction = slip ? 0 : 0.6f;
          col.material.frictionCombine = slip ? PhysicMaterialCombine.Minimum : PhysicMaterialCombine.Average;
     }


}
