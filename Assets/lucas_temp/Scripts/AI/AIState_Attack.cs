using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIState_Attack : AIState
{

     public float switchTarget = 5;


     public float tLastAttack; //useful when eg. if AI is chasing a lot but not attacking, maybe it's time to nuke
     public float tSwitchTarget;


     //private
     NetworkChara target;


     // public
     public override bool IsValid()
     {
          if (!brain.hasTarget)
               return false;

          return true;
     }

     public override void OnEnter()
     {
          DecideTarget();
          if (log) Debug.Log("AIState_Attack | OnEnterState() | target = " + target.name);
     }

     void DecideTarget()
     {
          target = brain.GetTarget_Closest();
          tSwitchTarget = Time.time + switchTarget;
     }

     public int TEST_AtkDamage = 1;
     public float TEST_AtkRange = 5;
     public string TEST_Atk = "You are already dead";
     public float TEST_AtkCD = 2.8f;
     float TEST_tNextAtk;

     public override void UpdateState()
     {
          if (Time.time > tSwitchTarget)
          {
               DecideTarget();
          }

          //if too far, move to target
          //if close enough, melee / shoot
          controller.TEST_RotateTowards(target);

          if (TEST_AtkRange > Vector3.Distance(transform.position, target.transform.position))
          {
               TEST_CastSpell(target);
               tLastAttack = Time.time;
          }
          else
          {
               controller.TEST_MoveTowards(target);
          }
     }

     //public override void FixedUpdateState()
     //{
     //     if (TEST_SpellRange < Vector3.Distance(transform.position, target.transform.position))
     //     {
     //          controller.TEST_MoveTowards(target);
     //     }
     //}

     public override void OnExit()
     {
          if (log) Debug.Log("AIState_Attack | OnExitState()");
          target = null;
     }

     public void TEST_CastSpell(NetworkChara target)
     {
          if (Time.time > TEST_tNextAtk)
          {
               TEST_tNextAtk = Time.time + TEST_AtkCD;
               if (log) Debug.Log("AIState_Attack | TEST_CastSpell()");

               var hpClass = target.GetComponent<HPComponent>();
               hpClass.DamageOrHeal(-TEST_AtkDamage);

          }
     }


     void OnDrawGizmos()
     {
          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position, TEST_AtkRange);
     }


}
