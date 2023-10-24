using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(NPCController))]
public class AIBrain : MonoBehaviour
{

     // How to use:
     // 1) attach an AIState component to GameObject
     // 2) the order of AIState matters, 

     // debug
     public string curState__;
     public bool log;
     public bool TEST_inUse;

     // setting
     public float alarmRange = 20; //how far will AI spot you?

     // private
     List<AIState> stateList;
     AIState current;
     static AIState idleState = new AIState(); //shared idle state, everybody do nothing


     void Awake()
     {
          stateList = new List<AIState>(GetComponents<AIState>());
          stateList.Add(idleState); //lowest priority

          current = idleState;
     }

     void Update()
     {
          if (TEST_inUse)
          {
               listUpdated = false;
               UpdateState();
          }
     }

     // Utility ----------------------------------------------------------------------
     public bool hasTarget
     {
          get => knownEnemy.Count > 0 || TargetList.Count > 0;
     }

     Dictionary<NetworkChara, int> knownEnemy = new Dictionary<NetworkChara, int>(); //the int is not in use yet, maybe do a Hate list?
     bool listUpdated;
     List<(NetworkChara, float)> _targetList = new List<(NetworkChara, float)>();
     public List<(NetworkChara, float)> TargetList
     {
          get
          {
               if (!listUpdated)
               {
                    listUpdated = true;
                    _targetList.Clear();
                    foreach (NetworkChara chara in NetworkChara.list)
                    {
                         if (!chara.isPlayer)
                              continue;

                         var dist = Vector3.Distance(transform.position, chara.transform.position);
                         if (dist > alarmRange)
                              continue;

                         _targetList.Add((chara, dist));

                         if (!knownEnemy.ContainsKey(chara))
                              knownEnemy.Add(chara, 0);
                    }
               }

               return _targetList;
          }
     }

     public NetworkChara GetTarget_Closest()
     {
          if (TargetList.Count > 0)
          {
               return TargetList.OrderBy(x => x.Item2).First().Item1;
          }
          else
          {
               foreach (var target in knownEnemy)
                    return target.Key; //we can't do Dictionary[0], so just give a random one
          }

          return null;
     }

     public NetworkChara GetTarget_Furthest(float maxRange)
     {
          TargetList.OrderBy(x => x.Item2); //this will refresh list of known
          knownEnemy.OrderBy(x => Vector3.Distance(transform.position, x.Key.transform.position)); //know we just shuffle the know

          if (knownEnemy.Count > 0)
               return knownEnemy.Last().Key;

          return null;
     }


     // state machine ----------------------------------------------------------------------

     void UpdateState()
     {
          curState__ = "CurState = " + current;

          AIState next = DecideNextState();

          // change state?
          if (current != next)
          {
               current.OnExit();
               current = next;
               current.OnEnter();
          }

          // update
          current.UpdateState();
     }

     AIState DecideNextState()
     {
          foreach (var state in stateList)
               if (state.IsValid())
                    return state;

          return null; // = idle
     }

     void OnDrawGizmosSelected()
     {
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(transform.position, alarmRange);
     }



}


