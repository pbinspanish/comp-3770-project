using System;
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
     ProjectileSetting setting { get => launcher.setting; }
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
          inUse = true;

          victims.Clear();
          tDespawn = Time.time + setting.range / setting.speed;
          this.originClientID = originClientID;

          gameObject.SetActive(true);

          transform.position = start;
          transform.rotation = Quaternion.LookRotation(dir);

          velocity = dir.normalized * setting.speed;

          _distPerFrame = setting.speed * Time.fixedDeltaTime;
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

     public static int countCapsule;
     public static int countSphere;


     void DetectCollisions()
     {
          var position = transform.localToWorldMatrix.MultiplyPoint(colliderCenter);

          int sum = 0;
          if (_distPerFrame > colliderRadius * 2)
          {
               // if we are moving super fast, and our collision radius is small, to provent missing an object (size be ~1 unit) in a update, use capsul cast
               sum = Physics.OverlapCapsuleNonAlloc(_pos0, transform.position, colliderRadius, _cache, setting.colMask);

               countCapsule++;
          }
          else
          {
               // good old sphere cast
               sum = Physics.OverlapSphereNonAlloc(transform.position, colliderRadius, _cache, setting.colMask);

               countSphere++;
          }

          for (int i = 0; i < sum; i++)
          {
               var col = _cache[i];
               int colMask = 1 << col.gameObject.layer;

               //if wall
               if ((colMask & setting.wallMask) != 0)
               {
                    OnHitVFX(transform.position, col.gameObject);
                    StuckToObject(col.transform, true);

                    return;
               }

               //if target
               if ((colMask & setting.targetMask) != 0)
               {
                    if (victims.Contains(col.gameObject))
                         continue;

                    victims.Add(col.gameObject);

                    //meat and juice
                    OnHitVFX(transform.position, col.gameObject);
                    Damage(col.gameObject);
                    Knock(col.gameObject);

                    launcher.OnHit_ClientRPC(transform.position, col.transform.position);

                    if (victims.Count >= setting.maxVictim)
                    {
                         StuckToObject(col.transform); //stick to last target
                         return; //skip the loop
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
     void OnHitVFX(Vector3 projectilePos, GameObject target) //visual effect
     {
          if (log) Debug.Log("OnHitVFX()");
     }

     void Damage(GameObject target)
     {
          if (log) Debug.Log("Damage()");

          if (!authority)
               return;

          var hpClass = target.GetComponent<HPComponent>();
          if (!hpClass) hpClass = target.GetComponentInParent<HPComponent>();

          if (hpClass)
               hpClass.DeltaHP(-setting.damage);
          else
               Debug.LogError(target.name);
     }

     void Knock(GameObject target)
     {
          if (log) Debug.Log("Knock()");

          if (!isServerObj)
               return;
          if (setting.knock.x == 0 && setting.knock.y == 0)
               return;
          var _rb = target.GetComponent<Rigidbody>();
          if (!_rb)
               return;

          Vector3 force = transform.forward * setting.knock.x + Vector3.up * setting.knock.y;
          _rb.AddForce(force, ForceMode.Force);
     }


     // end of use ---------------------------------------------------------------------------------
     void EndOfUse()
     {
          if (log) Debug.Log("EndOfUse()");

          inUse = false;

          gameObject.SetActive(false);
          victims.Clear();
          transform.parent = launcher.transform;
          transform.localScale = Vector3.one; //last parent might be scaled, return to 1

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

