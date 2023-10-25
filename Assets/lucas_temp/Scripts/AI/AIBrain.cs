using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Experimental.GraphView;


[RequireComponent(typeof(NetworkChara))]
[RequireComponent(typeof(HPComponent))]
public class AIBrain : MonoBehaviour
{

     // How to use:
     // 1) attach an AIState component to GameObject
     // 2) the order of AIState matters, 

     // debug
     public bool log;
     public bool AI_in_use = true;

     [Header("Setting")]
     public float spot_range = 20; //how far will AI spot you?
     public float alert_nearby = 20; // alert nearby ally when spot enemy

     // private
     HPComponent hp;
     List<AIState> states;
     AIState current;
     bool hasAlert;
     bool isAlive { get => hp.hp > 0; }


     void Awake()
     {
          states = new List<AIState>();
          states.AddRange(GetComponents<AIState>()); //find all state

          hp = GetComponent<HPComponent>();
          hp.On_death_blow += Die;

          //check
          Debug.Assert(states.Count > 0);
     }

     void Update()
     {
          if (AI_in_use)
          {
               _new = false;
               _sort = false;
               Update_state();
          }
     }


     // state machine ----------------------------------------------------------------------

     void Update_state()
     {
          AIState next = Decide_next_state();

          // alert near by ally
          if (!hasAlert && targets.Count > 0)
          {
               hasAlert = true;
               Alert_nearby();
          }

          if (next == null)
          {
               current = null; // idle
          }
          else
          {
               // change state?
               if (current != next)
               {
                    if (current)
                         current.OnExit();
                    current = next;
                    current.OnEnter();
               }

               // update
               current.UpdateState();
          }
     }

     AIState Decide_next_state()
     {
          foreach (var state in states)
               if (state.IsValid())
                    return state;

          return null; // = idle
     }


     // target util  ----------------------------------------------------------------------
     public List<AITargetData> targets { get => Update_targets(); }
     List<AITargetData> _targets = new List<AITargetData>();
     bool _new;

     List<AITargetData> Update_targets()
     {
          if (!_new)
          {
               _new = true;

               foreach (var chara in HPComponent.all)
               {
                    // hostile?
                    if (hp.IsFriend(chara.isPlayer))
                         continue;

                    // too far?
                    var dist = Vector3.Distance(transform.position, chara.transform.position);
                    if (dist > spot_range)
                         continue;

                    // add to list
                    var data = _targets.Find(x => x.hp == chara);

                    if (data == null)
                    {
                         data = new AITargetData();
                         data.hp = chara;
                         _targets.Add(data);
                    }

                    data.dist = dist;
               }
          }
          return _targets;
     }

     bool _sort;
     public AITargetData Get_target(bool true_closest___false_furthest = true)
     {
          if (targets.Count == 0)
               return null;

          if (!_sort)
          {
               _sort = true;
               targets.OrderBy(x => x.dist);
          }

          return true_closest___false_furthest ? targets[0] : targets[targets.Count - 1];
     }



     // die  ----------------------------------------------------------------------
     void Die()
     {
          AI_in_use = false;
     }


     // alert ally  ----------------------------------------------------------------------

     void Alert_nearby()
     {
          Debug.Log("Yeeeha! " + gameObject.name);
     }


     // TEST move and rorate ---------------------------------------------------------------------
     // replace with navmash later
     public float moveSpeed = 10f;
     public float rotateSpeed = 100f;

     public void TEST_Move_towards(Transform target, float range, float speedPct = 1f)
     {
          if (moveSpeed == 0)
               return;
          if (Vector3.Distance(target.position, transform.position) < range)
               return;

          transform.position = Vector3.MoveTowards(
               transform.position,
               target.position,
               moveSpeed * speedPct * Time.deltaTime);
     }

     public void TEST_Rotate_towards(Transform target)
     {
          if (rotateSpeed == 0)
               return;

          var dir = target.position - transform.position;
          dir.y = 0;
          var rot = Quaternion.LookRotation(dir);
          transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotateSpeed * Time.deltaTime);
     }


     // debug  ----------------------------------------------------------------------

     void OnDrawGizmosSelected()
     {
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(transform.position, spot_range);
     }

}


