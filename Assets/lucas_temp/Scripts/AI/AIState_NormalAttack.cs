using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;


public class AIState_NormalAttack : AIState
{

     public bool gizmos;
     public float switchTarget = 5;
     public float tLastAttack { get; private set; } //how long have we been chasing but not attacking??

     // private
     AITargetData target;
     float tSwitchTarget;


     public override bool IsValid()
     {
          return brain.targets.Count > 0;
     }

     public override void OnEnter()
     {
          //
     }

     public override void UpdateState()
     {
          // recover from last action
          if (Time.time < tLastAttack + atkRecover)
          {
               //brain.Stop_move();
               return;
          }

          // pick target?
          if (target == null)
          {
               DecideTarget();
          }
          else if (Time.time > tSwitchTarget)
          {
               DecideTarget();
          }

          // attack or chase
          bool inRange = atkRange > Vector3.Distance(transform.position, target.transform.position);
          if (isAtkReady && inRange)
          {
               TEST_Attack(target.hp);
          }
          else
          {
               brain.Move(target.transform.position, atkRange);
               //brain.Move_towards(target.transform, atkRange);
          }
     }

     public override void OnExit()
     {
          target = null;

          if (log) Debug.Log("AIState_Attack | OnExitState()");
     }


     // private  ---------------------------------------------------------------------
     void DecideTarget()
     {
          target = brain.Get_target();
          tSwitchTarget = Time.time + switchTarget;

          if (log) if (target != null) Debug.Log("AIState_Attack | DecideTarget() | target = " + target.hp.name);
     }

     // TEST attack ---------------------------------------------------------------------
     [Header("TEST")]
     public int atkDamage = 1;
     public float atkRange = 5;
     public float atkCoolDown = 2.8f;
     public float atkDelay = 0.5f; //delay before an attack connect
     public float atkRecover = 1.5f; //delay after an attack, then you can start moving again
     bool isAtkReady { get => Time.time > tLastAttack + atkCoolDown; }

     async void TEST_Attack(HPComponent hp)
     {
          if (isAtkReady)
          {
               // delay
               tLastAttack = Time.time;
               await Task.Delay((int)(atkDelay * 1000));

               // am I still alive?
               var myHP = GetComponent<HPComponent>();
               if (myHP && myHP.hp == 0)
                    return;

               // business
               hp.Damage(atkDamage);

               if (log) Debug.Log("AIState_Attack | TEST_CastSpell()");
          }
     }

     void OnDrawGizmosSelected()
     {
          if (!gizmos)
               return;

          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position, atkRange);
     }


}
