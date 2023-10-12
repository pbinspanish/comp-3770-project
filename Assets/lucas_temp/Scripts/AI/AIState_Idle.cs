using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIState_Idle : AIState
{

     public override bool IsValid()
     {
          return true;
     }

     public override void OnEnterState()
     {
          //make a smoothie?
     }

     public override void UpdateState()
     {
          //drinking smoothie
     }

     //public override void FixedUpdateState()
     //{
     //     //drinking smoothie faster!!!
     //}

     public override void OnExitState()
     {
          //bye
     }


}
