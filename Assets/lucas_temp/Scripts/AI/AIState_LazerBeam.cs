using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class AIState_LazerBeam : AIState
{

     public bool __log;


     [Space(10)]
     public float triggerAfterNotAttackingFor = 5f; //sec
     public float duration = 8.5f;
     public float coolDown = 60f;
     public float verginCoolDown = 20f; //sec                    
     public float lazerMoveSpeed = 9.5f;
     public float lazerHitBoxRadius = 2f;
     public int lazerDamage = 1;
     public float hitInterval = 0.1f;



     // private
     AITargetData target;
     Vector3 targetPos;
     AIState_NormalAttack aiStateAttack;
     LazerBeam lazer;
     bool verginCDTriggered;
     int damageMask;
     Collider[] _cache = new Collider[20];
     float tNextHit;
     int allMask;


     float tStart;
     float tEnd { get => tStart + duration; }
     float tReady { get => tStart + duration + coolDown; }
     bool isReady { get => Time.time > tReady; }
     bool isShooting { get => Time.time < tEnd; }
     bool isCoolingDown { get => Time.time > tEnd && Time.time < tReady; }


     void Awake()
     {
          aiStateAttack = GetComponent<AIState_NormalAttack>();
          lazer = gameObject.GetComponentInChildren<LazerBeam>();

          // 
          damageMask = LayerMaskUtil.Get_target_mask(CharaTeam.enemy, true, false, true);
          allMask = damageMask;
          allMask |= LayerMaskUtil.wall_mask;

     }

     public override bool IsValid()
     {
          if (Time.time < tReady)
               return false;
          if (Time.time < aiStateAttack.tLastAttack + triggerAfterNotAttackingFor)
               return false;
          if (brain.targets.Count == 0)
               return false;

          if (!verginCDTriggered)
          {
               verginCDTriggered = true;
               tStart = Time.time + verginCoolDown - duration - coolDown;
               //tReady = Time.time + verginCoolDown;
               if (__log) Debug.Log("AIState_LazerBeam.IsValid()  ->  coolDownVergin");
               return false;
          }

          return true;
     }

     public override void OnEnter()
     {
          if (lazer == null)
               lazer = brain.GetComponent<LazerBeam>();

          target = brain.Get_target(false);
          targetPos = target.transform.position;

          tStart = Time.time;
     }


     public override void UpdateState()
     {
          if (!isShooting)
               return;

          targetPos = Vector3.MoveTowards(
          targetPos,
          target.transform.position,
          lazerMoveSpeed * Time.deltaTime);

          //shooting lazer, lazer chase target
          lazer.ShootAtPos(lazer.transform.position, targetPos, allMask, OnLazerHit);
     }


     //lazer damage
     void OnLazerHit(Vector3 pos)
     {
          if (Time.time < tNextHit)
               return;
          tNextHit = Time.time + hitInterval;

          var count = Physics.OverlapSphereNonAlloc(pos, lazerHitBoxRadius, _cache, damageMask);
          for (int i = 0; i < count; i++)
          {
               var hpClass = _cache[i].GetComponent<HPComponent>();
               hpClass.Damage(lazerDamage);
          }
     }


     public override void OnExit()
     {
          lazer.Stop();
          if (__log) Debug.Log("AIState_LazerBeam.OnExitState()");
     }


}
