using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
using Unity.VisualScripting;


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


     void FixedUpdate()
     {
          if (Time.fixedTime > tDespawn)
          {
               EndOfUse();
          }
          else if (inUse)
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
     }



     // fire ---------------------------------------------------------------------------------
     public void Fire(Vector3 start, Vector3 dir, ulong originClientID)
     {
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

          _distPerFrame = setting.speed * Time.fixedDeltaTime; //cache speed

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
     Collider[] _cache = new Collider[20];
     Vector3 _pos0; //cache
     float _distPerFrame; //cache
     int _hitCount;
     float tResetVictim;

     void HandleCollision()
     {
          var position = transform.localToWorldMatrix.MultiplyPoint(colliderCenter);
          int hit = 0;
          if (_distPerFrame > colliderRadius * 2)
          {
               // if we are moving super fast, and our collision radius is small, to provent missing an object (size be ~1 unit) in a update, use capsul cast
               hit = Physics.OverlapCapsuleNonAlloc(_pos0, transform.position + colliderCenter, colliderRadius, _cache, setting.allMask);
          }
          else
          {
               // good old sphere cast
               hit = Physics.OverlapSphereNonAlloc(transform.position + colliderCenter, colliderRadius, _cache, setting.allMask);
          }


          //check collisions
          for (int i = 0; i < hit; i++)
          {
               Collider other = _cache[i];
               int otherMask = 1 << other.gameObject.layer; //layer to layerMask


               // if wall
               if ((otherMask & setting.wallMask) != 0)
               {
                    OnHitVFX(other.gameObject);
                    StuckToObject(other.transform, true);
                    return; //end of use
               }


               // if target
               if ((otherMask & setting.targetMask) != 0)
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

                    //network
                    launcher.AfterHit_ClientRPC(transform.position, other.transform.position);


                    // reset victim?
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
          hpClass.DamageOrHeal(damageOrHeal);
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


}

