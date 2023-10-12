using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class AIState_LazerBeam : AIState
{

     public float triggerAfterNotAttackingFor = 5f; //sec
     public float duration = 6.5f;
     public float coolDown = 60f;
     public float verginCoolDown = 20f; //sec                    
     public float lazerMoveSpeed = 9.5f;
     public float moveWhileLazer = 0.25f; //percentage speed                             
     public float lazerHitBoxRadius = 2f;
     public int lazerDamage = 1;
     public float minHitInterval = 0.1f;


     // private
     NetworkChara target;
     Vector3 targetPos;
     float tNextCast;
     float tEnd;
     AIState_Attack aiStateAttack;
     LazerBeam lazer;
     bool maxCDTriggered;
     int damageMask;
     Collider[] _cache = new Collider[20];
     float tNextHit;


     void Awake()
     {
          aiStateAttack = GetComponent<AIState_Attack>();
          lazer = gameObject.GetComponentInChildren<LazerBeam>();
          damageMask = LayerMask.GetMask("Player");
     }

     public override bool IsValid()
     {
          if (Time.time < tNextCast)
               return false;
          if (Time.time < aiStateAttack.tLastAttack + triggerAfterNotAttackingFor)
               return false;
          if (!brain.hasTarget)
               return false;

          if (!maxCDTriggered)
          {
               maxCDTriggered = true;
               tNextCast = Time.time + verginCoolDown;
               if (log) Debug.Log("AIState_LazerBeam.IsValid()  ->  coolDownVergin");
               return false;
          }


          return true;
     }

     public override void OnEnter()
     {
          if (lazer == null)
               lazer = brain.GetComponent<LazerBeam>();

          target = brain.GetTarget_Furthest(float.MaxValue);
          targetPos = target.transform.position;

          tEnd = Time.time + duration;
     }

     public override void UpdateState()
     {
          if (Time.time > tEnd)
          {
               tNextCast = Time.time + coolDown;
               return;
          }

          targetPos = Vector3.MoveTowards(
               targetPos,
               target.transform.position,
               lazerMoveSpeed * Time.deltaTime);

          //shooting lazer, lazer chase target
          lazer.ShootAtPos(lazer.transform.position, targetPos, OnLazerHit);


          //move slowly towards
          controller.TEST_MoveTowards(target, moveWhileLazer);
     }


     //lazer damage
     void OnLazerHit(Vector3 pos)
     {
          if (Time.time < tNextHit)
               return;

          var count = Physics.OverlapSphereNonAlloc(pos, lazerHitBoxRadius, _cache, damageMask);

          for (int i = 0; i < count; i++)
          {
               var hpClass = _cache[i].GetComponent<HPComponent>();
               hpClass.DamageOrHeal(-lazerDamage);

               tNextHit = Time.time + minHitInterval;
          }
     }


     //public override void FixedUpdateState()
     //{
     //     //
     //}

     public override void OnExit()
     {
          lazer.Stop();
          if (log) Debug.Log("AIState_LazerBeam.OnExitState()");
     }


}
