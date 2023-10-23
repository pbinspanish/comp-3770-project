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
     public Vector3 colliderCenter = new Vector3(0, 0, 0);
     public float colliderRadius = 1.5f;
     //public bool isHoming;
     //public float homingDelay;
     //public float rotateSpeed = 5.0f;
     //public float speed = 11.0f;


     // private
     ProjectileEntry setting { get => launcher.setting; }
     [HideInInspector] public ProjectileLauncher launcher;
     bool inUse = false; //if not inUse, then it won't move/collide, only wait for despawn
     float tDespawn;
     List<GameObject> victims = new List<GameObject>();
     ulong originClientID;
     Vector3 velocity;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     bool isServerObj { get => NetworkManager.Singleton.IsServer; }
     [SerializeField] private float detectionRange = 10f;  //Detection range for Enemies
     [SerializeField] private float damageRadius = 1f; //range of the projectile to hit
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
          }
          else if (inUse && !setting.isHoming)
          {
               // reset victim list? so we can hit them again
               if (setting.hitSameTarget && Time.time > tResetVictim)
               {
                    victims.Clear();
                    tResetVictim = -1;
               }

               Move();
               HandleCollision();

          }
          else if (inUse && setting.isHoming)
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

                         if (distance <= detectionRange)
                         {
                              StartCoroutine(Homing());
                         }
                    }
               }
               HandleCollision();
          }
     }



     // fire ---------------------------------------------------------------------------------
     public void Fire(Vector3 start, Vector3 dir, string launcherLayer, ulong originClientID)
     {
          SetupCollisionLayer(launcherLayer);

          enabled = true;
          gameObject.SetActive(true);

          inUse = true;

          tDespawn = Time.time + setting.range / setting.speed;

          victims.Clear();
          _hitCount = 0;

          this.originClientID = originClientID;

          transform.position = start;
          transform.rotation = Quaternion.LookRotation(dir);

          velocity = dir.normalized * setting.speed;

          //_distPerFrame = setting.speed * Time.fixedDeltaTime; //cache speed

          foreach (var particle in GetComponentsInChildren<ParticleSystem>())
               particle.Play();

     }

     int allMask;
     int targetMask;
     void SetupCollisionLayer(string launcherLayer)
     {
          targetMask = LayerMaskUtil.get_target_mask(setting, launcherLayer);
          allMask = LayerMaskUtil.get_all_mask(setting, launcherLayer);
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
     Collider[] _cache = new Collider[20];
     Vector3 _pos0; //cache
     int _hitCount;
     float tResetVictim;

     void HandleCollision()
     {
          // detect collision
          int count = Physics.OverlapCapsuleNonAlloc(_pos0, transform.position + colliderCenter, colliderRadius, _cache, allMask);

          // loop through all hits
          for (int i = 0; i < count; i++)
          {
               Collider other = _cache[i];
               int otherMask = 1 << other.gameObject.layer; //layer of the thing we hit, to layerMask

               // if wall
               if ((otherMask & LayerMaskUtil.wall_mask) != 0)
               {
                    OnHitVFX(other.gameObject);
                    StuckToObject(other.transform, true);
                    return; //end of use
               }

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

                    // throwing dead enemy around
                    if (hpClass.hp == 0)
                         Knock(other.gameObject, true);  // it looks cooler, if we apply constant force, even if setting = instant force

                    // the dead don't block bullet or take damage
                    if (hpClass.hp == 0)
                         continue;

                    // same for victim
                    if (victims.Contains(other.gameObject))
                         continue;

                    // good
                    _hitCount++;
                    victims.Add(other.gameObject);

                    // (!) apply instant force / only once a while
                    if (!setting.smoothForce)
                         Knock(other.gameObject);

                    Damage(other.gameObject);
                    OnHitVFX(other.gameObject);


                    // reset victim list?
                    if (setting.hitSameTarget && tResetVictim == -1)
                         tResetVictim = Time.time + setting.hitSameTargetEvery / 1000;


                    // finally
                    if (_hitCount >= setting.maxHit)
                    {
                         StuckToObject(other.transform);
                         return; //end of use
                    }
               }

          }

     }

     void StuckToObject(Transform target, bool isWall = false)
     {
          if (!target)
          {
               Debug.LogError("");
               return;
          }

          if (setting.stickToTarget <= 0)
               return;

          // stick to wall or enemy
          inUse = false;
          velocity = Vector3.zero;

          if (!isWall)
               transform.parent = target;

          tDespawn = Time.fixedTime + (isWall ? setting.stickToWall : setting.stickToTarget);
     }


     // on hit ---------------------------------------------------------------------------------

     void OnHitVFX(GameObject target) //visual effect
     {
          if (log) Debug.Log("OnHitVFX()");

          if (setting.onHitVFX == null)
               return;

          var vfx = launcher.GetVFX(this);
          vfx.gameObject.SetActive(true);
          vfx.transform.position = transform.position + colliderCenter;

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
          int abs = Mathf.Abs(setting.damage) + Random.Range(-setting.dmgRandomRange, setting.dmgRandomRange + 1);
          abs = Mathf.Clamp(abs, 1, int.MaxValue);

          // damage or heal
          int sign = setting.damage > 0 ? -1 : 1; //yes, damage is -
          int damageOrHeal = abs * sign;


          var hpClass = target.GetComponent<HPComponent>();
          hpClass.Damage_or_heal(damageOrHeal);
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
          inUse = false;
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
               Gizmos.DrawWireSphere(_pos0 + colliderCenter, colliderRadius);
          }
          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position + colliderCenter, colliderRadius);
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

