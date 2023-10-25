using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// Script attached to a projectile. Handle collision, damage, force etc.
/// </summary>
public class Projectile : MonoBehaviour
{

     //TODO: [TEST] currently let the firing client decide damage and knock, see how it works out
     bool authority { get => setting.clientHasAutority ? (originClientID == clientID) : isServerObj; }
     bool log = false; //test


     // public
     [Header("Setting")]
     public float damageRadius = 1.5f;


     // private
     ProjectileEntry setting { get => launcher.setting; }
     [HideInInspector] public ProjectileLauncher launcher;
     bool isMovingAndColliding = false; //if not inUse, then it won't move/collide, only wait for despawn
     float tDespawn;
     List<GameObject> victims = new List<GameObject>();
     ulong originClientID;
     Vector3 velocity;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     bool isServerObj { get => NetworkManager.Singleton.IsServer; }
     private GameObject Target;


     void Start()
     {
          Target = GameObject.FindGameObjectWithTag("Enemy");
     }


     void FixedUpdate()
     {
          if (Time.fixedTime > tDespawn)
          {
               EndOfUse();
               return;
          }

          if (!isMovingAndColliding)
               return; //eg. I'm stuck to enemy


          // reset victim list? so we can hit them again
          if (setting.hitSameTarget && Time.time > tResetVictim)
          {
               victims.Clear();
               tResetVictim = -1;
          }


          // move
          if (!setting.isHoming)
          {
               Move();
          }
          else
          {
               //Debug.Log("DSA");
               _pos0 = transform.position;
               transform.position += transform.forward * (setting.homingSpeed * Time.fixedDeltaTime);
               //Move();
               GameObject[] targets;

               targets = GameObject.FindGameObjectsWithTag("Enemy");
               //Debug.Log("IDk");
               if (Target != null)
               {
                    //Debug.Log("HI");
                    foreach (GameObject Target in targets)
                    {
                         float distance = Vector3.Distance(Target.transform.position, transform.position);

                         if (distance <= setting.homingDetect)
                         {
                              StartCoroutine(Homing());
                         }
                    }
               }
          }

          // collide
          HandleCollision();

     }


     // fire ---------------------------------------------------------------------------------
     public void Fire(Vector3 start, Vector3 dir, string launcherLayer, ulong originClientID)
     {

          enabled = true;
          gameObject.SetActive(true);

          isMovingAndColliding = true;

          tDespawn = Time.time + setting.range / setting.speed;

          victims.Clear();
          _hitCount = 0;

          this.originClientID = originClientID;

          transform.position = start;
          transform.rotation = Quaternion.LookRotation(dir);

          targetMask = LayerMaskUtil.get_target_mask(setting, launcherLayer); // setup collision mask

          velocity = dir.normalized * setting.speed;

          foreach (var particle in GetComponentsInChildren<ParticleSystem>())
               particle.Play();

     }


     // move ---------------------------------------------------------------------------------
     void Move()
     {
          //choosing 'telerpot' over Rigidbody.Addforce() is because we want to stick the arrow on enemy,
          //but since both arrow and enemy has rb, adding force will pull them apart..
          _pos0 = transform.position;
          transform.position += velocity * Time.fixedDeltaTime;
     }


     // detect collision ---------------------------------------------------------------------------------
     Collider[] _cache = new Collider[50];
     Vector3 _pos0; //cache
     int _hitCount;
     float tResetVictim;
     int targetMask;

     void HandleCollision()
     {
          // detect wall
          var hitWall = Physics.Raycast(_pos0, transform.position - _pos0, setting.speed * Time.fixedDeltaTime, LayerMaskUtil.wall_mask);
          if (hitWall)
          {
               OnHitVFX();
               StuckToObject(null);
               return; //end of use
          }


          // detect target
          int count = Physics.OverlapCapsuleNonAlloc(_pos0, transform.position, damageRadius, _cache, targetMask);

          for (int i = 0; i < count; i++)
          {
               Collider other = _cache[i];
               int otherMask = 1 << other.gameObject.layer; //layer of the thing we hit, to layerMask

               // if target
               if ((otherMask & targetMask) != 0)
               {
                    // if not creature
                    var hpClass = other.GetComponent<HPComponent>();
                    if (hpClass == null)
                    {
                         Debug.LogError("Something in this layer but has no HP?? " + other.name);
                         continue;
                    }

                    // (!) apply constant force / every update
                    if (setting.smoothForce)
                         Knock(other.gameObject, true);


                    if (hpClass.hp == 0)
                    {
                         Knock(other.gameObject, true); // throwing dead enemy around
                         continue; // dead don't block bullet or take damage
                    }


                    // victim don't block bullet or take damage
                    if (victims.Contains(other.gameObject))
                         continue;

                    // good
                    _hitCount++;
                    victims.Add(other.gameObject);

                    // (!) apply instant force / only once a while
                    if (!setting.smoothForce)
                         Knock(other.gameObject);

                    Damage(other.gameObject);
                    OnHitVFX();


                    // reset victim list?
                    if (setting.hitSameTarget && tResetVictim == -1)
                         tResetVictim = Time.time + setting.hitSameTargetEvery / 1000;


                    // finally
                    if (setting.maxHit > 0 && _hitCount >= setting.maxHit)
                    {
                         StuckToObject(other.transform);
                         return; //end of use
                    }
               }

          }

     }

     void StuckToObject(Transform target)
     {
          //if (setting.stickToTarget <= 0)
          //     return;

          // stick to wall or enemy
          isMovingAndColliding = false;
          tDespawn = Time.fixedTime + (target == null ? setting.stickToWall : setting.stickToTarget);


          //velocity = Vector3.zero;

          if (target)
               transform.parent = target;  // stick to enemy
     }


     // on hit ---------------------------------------------------------------------------------

     void OnHitVFX() //visual effect
     {
          if (log) Debug.Log("OnHitVFX()");

          if (setting.onHitVFX == null)
               return;

          var vfx = launcher.GetVFX(this);
          vfx.gameObject.SetActive(true);
          vfx.transform.position = transform.position;

          // in case you have particles
          var pList2 = vfx.GetComponentsInChildren<ParticleSystem>(); //this DOES includes <T> on the parent
          if (pList2.Length > 0)
               foreach (var p in pList2)
                    p.Play();

          // finally
          launcher.PlanRecycleVFX(vfx);

     }


     void Damage(GameObject target)
     {
          if (log) Debug.Log("Damage()");

          if (!authority)
               return;

          // absolute value
          int value = setting.damage + Random.Range(-setting.dmgRandomRange, setting.dmgRandomRange + 1);

          // damage or heal
          var hp = target.GetComponent<HPComponent>();

          if (setting.isHeal)
               hp.Heal(value);
          else
               hp.Damage(value);

     }


     void Knock(GameObject target, bool smooth = false)
     {
          if (log) Debug.Log("Knock()");

          if (!isServerObj)
               return;
          var _rb = target.GetComponent<Rigidbody>();
          if (!_rb)
               return;

          //
          Vector3 force = new Vector3();
          if (setting.forceDirection == ForceDir.Foward)
          {
               force = transform.forward * setting.forceFwdUp.x + Vector3.up * setting.forceFwdUp.y;
          }
          else if (setting.forceDirection == ForceDir.AwayFromCenter)
          {
               force = (target.transform.position - transform.position).normalized * setting.forceFwdUp.x + Vector3.up * setting.forceFwdUp.y;
          }

          force *= smooth ? Time.fixedDeltaTime : 1; //impulse or constantly apply

          _rb.AddForce(force, UnityEngine.ForceMode.Force);
     }


     // end of use ---------------------------------------------------------------------------------
     public void EndOfUse()
     {
          if (log) Debug.Log("EndOfUse()");
          isMovingAndColliding = false;
          gameObject.SetActive(false);

          victims.Clear();
          transform.parent = launcher.transform;

          launcher.Recycle(this);
     }


     // show collider ---------------------------------------------------------------------------------
     void OnDrawGizmosSelected()
     {
          if (Application.isPlaying)
          {
               Gizmos.color = Color.blue;
               Gizmos.DrawWireSphere(_pos0, damageRadius);
          }

          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position, damageRadius);
     }


     //Homing -------------------------------------------------------------------
     IEnumerator Homing()
     {
          //time until looking for closest enemy
          yield return new WaitForSeconds(setting.homingDelay);

          FindClosestEnemy();


     }
     public GameObject FindClosestEnemy()
     {
          GameObject[] gos;
          gos = GameObject.FindGameObjectsWithTag("Enemy");
          GameObject closest = null;
          float distance = Mathf.Infinity;
          Vector3 position = transform.position;
          foreach (GameObject go in gos)
          {
               Vector3 diff = go.transform.position - position;
               float curDistance = diff.sqrMagnitude;
               if (curDistance < distance)
               {

                    closest = go;
                    distance = curDistance;
               }


          }

          if (closest != null)
          {
               //move towards closest enemy
               Vector3 direction = (closest.transform.position - transform.position).normalized;
               Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
               transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * setting.homingRotateSpeed);
          }


          return closest;
     }
}

