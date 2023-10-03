using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;


public class ProjectileAddon : MonoBehaviour
{

     // attached to projectile to detect collision



     //setup before use
     public ProjectileLauncher mgr; //contains settings
     public ObjectPool<ProjectileAddon> pool;
     public Action AfterHit; //for extra awesomeness, currently unused


     //var
     Collider col;
     Rigidbody rb;
     Vector3 dir;
     Vector3 startPos;
     int penetrateCount;
     float tFire;
     bool log { get => mgr.log; }
     bool isServerObj { get => NetworkManager.Singleton.IsServer; }


     void Awake()
     {
          rb = GetComponent<Rigidbody>();
          col = GetComponent<Collider>();
          col.isTrigger = true;
     }
     void Update()
     {
          CheckRangeAndLifeTime();
     }
     void OnTriggerEnter(Collider other)
     {
          OnHit(other.gameObject);
     }



     // fire ---------------------------------------------------------------------------------
     public void Fire(Vector3 _start, Vector3 _direction)
     {
          gameObject.SetActive(true);
          tFire = Time.time;

          dir = _direction;
          startPos = _start;
          transform.position = _start;
          rb.velocity = _direction * (mgr.velocity);
     }


     // hit ---------------------------------------------------------------------------------
     public void OnHit(GameObject target)
     {
          OnHitVFX();

          if (isServerObj)
          {
               Damage(target);
               Push(target);
               Penetrate();
          }

          AfterHit?.Invoke();
     }

     void OnHitVFX() //visual effect
     {
          if (log) TEST.GUILog("OnHitVFX()");
     }

     void Damage(GameObject target)
     {
          if (log) TEST.GUILog("Damage()");

          // deal damage
          var hpClass = target.GetComponent<HPComponent>();

          if (hpClass != null)
               hpClass.HP -= mgr.damage;
     }

     void Penetrate()
     {
          if (log) TEST.GUILog("Penetrate()");

          penetrateCount--;
          if (penetrateCount < 0) EndOfUse();
     }

     void Push(GameObject target)
     {
          if (log) TEST.GUILog("Push()");

          var _rb = target.GetComponent<Rigidbody>();
          if (_rb == null) return;

          Vector3 force = dir.normalized * mgr.force.x + Vector3.up * mgr.force.y;
          _rb.AddForce(force, ForceMode.Force);
     }



     // end of use ---------------------------------------------------------------------------------

     void EndOfUse()
     {
          if (log) TEST.GUILog("EndOfUse()");

          gameObject.SetActive(false);
          penetrateCount = mgr.penetrate;

          pool.Release(this);
     }

     void OutOfRange()
     {
          EndOfUse();
     }

     void CheckRangeAndLifeTime()
     {
          if (mgr.despawnRange > 0 && Vector3.Distance(startPos, transform.position) > mgr.despawnRange)
               OutOfRange();
          if (mgr.despawnInSec > 0 && Time.time - tFire > mgr.despawnInSec)
               EndOfUse();
     }


}

