using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Script attached to a projectile. Handle collision, damage, knock...
/// </summary>
public class Projectile : MonoBehaviour
{

     //TODO: [TEST] currently let the firing client decide damage and knock, see how it works out
     bool authority { get => setting.clientHasAutority ? (originClientID == clientID) : isServerObj; }
     bool log { get => setting.log; } //test


     //public
     [Header("Setting")]
     public Vector3 colliderCenter = new Vector3(0, 0, 0);
     public float colliderRadius = 1.5f;


     //private
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
               Move();
               DetectCollisions();
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


     // detect collision ---------------------------------------------------------------------------------
     void Move()
     {
          //choosing this over Rigidbody.Addforce() is because we want to stick the arrow on enemy, but since both has rb they will seperate when add force...
          _pos0 = transform.position;
          transform.position += velocity * Time.fixedDeltaTime;
     }


     // detect collision ---------------------------------------------------------------------------------
     Collider[] _cache = new Collider[10];
     Vector3 _pos0; //cache
     float _distPerFrame; //cache
     int _hitCount;
     float tResetVictim;


     void DetectCollisions()
     {
          var position = transform.localToWorldMatrix.MultiplyPoint(colliderCenter);

          int sum = 0;
          if (_distPerFrame > colliderRadius * 2)
          {
               // if we are moving super fast, and our collision radius is small, to provent missing an object (size be ~1 unit) in a update, use capsul cast
               sum = Physics.OverlapCapsuleNonAlloc(_pos0, transform.position, colliderRadius, _cache, setting.colMask);
          }
          else
          {
               // good old sphere cast
               sum = Physics.OverlapSphereNonAlloc(transform.position, colliderRadius, _cache, setting.colMask);
          }


          for (int i = 0; i < sum; i++)
          {
               Collider target = _cache[i];
               int mask = 1 << target.gameObject.layer;


               //if wall
               if ((mask & setting.wallMask) != 0)
               {
                    OnHitVFX(target.gameObject);
                    StuckToObject(target.transform, true); //end of use

                    return;
               }


               //if target
               if (setting.hitSameTarget && Time.time > tResetVictim)
               {
                    victims.Clear();
                    tResetVictim = -1; //so we know to get a new value later
               }

               if ((mask & setting.targetMask) != 0)
               {
                    // we don't mind throwing dead enemy around
                    Knock(target.gameObject);


                    // if reset victim timer (some spell can damage the same target every X second)
                    if (setting.hitSameTarget && tResetVictim == -1)
                         tResetVictim = Time.time + setting.hitSameTargetEvery / 1000;

                    // if alive
                    var hpClass = target.GetComponent<HPComponent>();
                    if (hpClass == null || hpClass.hp == 0)
                         continue;

                    // if already victim
                    if (victims.Contains(target.gameObject))
                         continue;


                    // good
                    _hitCount++;
                    victims.Add(target.gameObject);

                    OnHitVFX(target.gameObject);
                    Damage(target.gameObject);

                    //network
                    launcher.AfterHit_ClientRPC(transform.position, target.transform.position);

                    // end of use
                    if (_hitCount >= setting.maxHit)
                    {
                         StuckToObject(target.transform); //end of use
                         return;
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

          UIDamageTextMgr.inst.OnDamage(damageOrHeal, target);

          var hpClass = target.GetComponent<HPComponent>();
          hpClass.DeltaHP(damageOrHeal);
     }


     void Knock(GameObject target)
     {
          if (log) Debug.Log("Knock()");

          if (!isServerObj)
               return;
          if (setting.forceUp == 0 && setting.forceFwd == 0)
               return;
          var _rb = target.GetComponent<Rigidbody>();
          if (!_rb)
               return;

          Vector3 force = new Vector3();
          if (setting.forceDirection == ForceDir.Foward)
          {
               force = transform.forward * setting.forceFwd + Vector3.up * setting.forceUp;
          }
          else if (setting.forceDirection == ForceDir.RelativeToCenter)
          {
               force = (target.transform.position - transform.position).normalized * setting.forceFwd + Vector3.up * setting.forceUp;
          }

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
          Gizmos.color = Color.blue;
          Gizmos.DrawWireSphere(_pos0 + colliderCenter, colliderRadius);

          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position + colliderCenter, colliderRadius);
     }


}

