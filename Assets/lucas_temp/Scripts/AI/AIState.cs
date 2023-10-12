using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AIBrain))]
public abstract class AIState : MonoBehaviour
{

     //abstract
     public abstract bool IsValid(); //before neter state, check valid
                                     // eg.if this is AttackState, is there any enemy in melee range / in sight so we can chase?

     public abstract void OnEnterState(); //choose target here

     public abstract void UpdateState(); //then update is called repeatly until...
     //public abstract void FixedUpdateState();

     public abstract void OnExitState(); //clean up here



     //protected
     protected bool log { get => brain.log; }

     protected AIBrain brain { get { if (_brain == null) _brain = GetComponent<AIBrain>(); return _brain; } }
     AIBrain _brain;

     protected NPCController controller { get { if (_ctrl == null) _ctrl = GetComponent<NPCController>(); return _ctrl; } }
     NPCController _ctrl;





}
