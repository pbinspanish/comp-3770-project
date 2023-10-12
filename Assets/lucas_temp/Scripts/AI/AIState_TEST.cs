using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIState_TEST : AIState
{

     public float duration = 3; //once enter, how long does it last?
     public float coolDown = 10; //once exit, the earliest time of next enter?
     public bool startWithMaxCoolDown;
     //eg. say this is NukeAllPlayer, with coolDown=100
     //do we start the battle with coolDown 0 (nuke them RIGHT NOW) or 100 (nuke them later)?

     //private
     float tNextActive;
     float tStayHere;
     bool maxCDTriggered;


     void Awake()
     {
          if (startWithMaxCoolDown)
               tNextActive = coolDown;
     }

     public override bool IsValid()
     {
          if (Time.time < tNextActive)
               return false;

          return true;
     }

     public override void OnEnterState()
     {
          if (startWithMaxCoolDown && maxCDTriggered == false)
          {
               maxCDTriggered = true;
               tNextActive = coolDown;
               if (log) Debug.Log("AIState_TEST.OnEnterState() but startWithMaxCoolDown");
               return;
          }

          tStayHere = Time.time + duration;
          if (log) Debug.Log("AIState_TEST.OnEnterState()");

     }

     public override void UpdateState()
     {
          if (Time.time > tStayHere)
          {
               tNextActive = Time.time + coolDown;
          }
     }

     //public override void FixedUpdateState()
     //{
     //     //
     //}

     public override void OnExitState()
     {
          if (log) Debug.Log("AIState_TEST.OnExitState()");
     }


}
