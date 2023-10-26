using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(NetworChara))]
[RequireComponent(typeof(HPComponent))]
public class AIBrain : MonoBehaviour
{

     // How to use:
     // 1) attach an AIState component to GameObject
     // 2) the order of AIState matters, 


     // static
     public static int tDespoawn = 5; //sec


     // public
     public bool gizmos;
     public bool log;
     public bool AI_in_use = true;
     [Header("Setting")]
     public float spot_range = 20; //how far will AI spot you?
     public float alert_nearby = 20; // alert nearby ally when spot enemy


     // private
     List<AIState> states;
     AIState current;
     HPComponent hp;


     void Awake()
     {
          states = new List<AIState>();
          states.AddRange(GetComponents<AIState>()); //find all state

          hp = GetComponent<HPComponent>();
          hp.On_death_blow += Die;

          TEST.OnConnect += On_connect;
          TEST.OnDisconnect += On_disconnect;

          rb = GetComponent<Rigidbody>();
          agent = GetComponent<NavMeshAgent>();

          //check
          Debug.Assert(states.Count > 0);
     }

     void Update()
     {
          _updated = false; //reset flags
          _sorted = false;

          Update_state();
          Alert_nearby();
     }

     public bool updatePosition;
     public bool updateRotation;
     public bool updateUpAxis;


     void FixedUpdate()
     {
          updatePosition = agent.updatePosition;
          updateRotation = agent.updateRotation;
          updateUpAxis = agent.updateUpAxis;


          if (Time.time < t_enable_agent)
               Use_agent_and_kinematic(false);
          else if (rb.velocity != Vector3.zero)
               Use_agent_and_kinematic(false);
          else
          {
               Use_agent_and_kinematic(true);
               Update_agent();
          }
     }

     // state machine ----------------------------------------------------------------------
     void Update_state()
     {
          AIState next = Decide_next_state();

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

     bool _updated;
     List<AITargetData> Update_targets()
     {
          if (!_updated)
          {
               _updated = true;

               foreach (var chara in HPComponent.all)
               {
                    // hostile?
                    if (!hp.IsEnemy(chara.team))
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

     bool _sorted;
     public AITargetData Get_target(bool true_closest___false_furthest = true)
     {
          if (targets.Count == 0)
               return null;

          if (!_sorted)
          {
               _sorted = true;
               targets.OrderBy(x => x.dist);
          }

          return true_closest___false_furthest ? targets[0] : targets[targets.Count - 1];
     }


     // die  ----------------------------------------------------------------------
     async void Die()
     {
          AI_in_use = false;

          await Task.Delay(tDespoawn * 1000);
          Despawn();
     }

     void Despawn()
     {
          Destroy(gameObject);
     }


     // alert nearby  ----------------------------------------------------------------------
     bool hasAlert;
     void Alert_nearby()
     {
          if (!hasAlert && targets.Count > 0)
          {
               hasAlert = true;
               Debug.Log("Yeeeha! " + gameObject.name);
          }
     }


     // network  ----------------------------------------------------------------------
     //bool isConnected { get => TEST.isConnected; }
     void On_connect()
     {
          enabled = agent.enabled = NetworkManager.Singleton.IsServer;
     }
     void On_disconnect()
     {
          enabled = agent.enabled = AI_in_use; //switch to local
     }


     // move with navMesh  ---------------------------------------------------------------------
     public float moveSpeed = 10f;
     public float rotateSpeed = 100f;
     public float acc = 20f;
     float reach = 20f;

     NavMeshAgent agent;
     Vector3 target_pos;
     Transform chase_after;
     int move_mode; //1 = Vector3, 2 = Transform

     public void Move_towards(Vector3 pos, float _reach)
     {
          target_pos = pos; //in Update()
          reach = _reach;

          move_mode = 1;
     }
     public void Move_towards(Transform _target, float _reach)
     {
          chase_after = _target; //in Update()
          reach = _reach;

          move_mode = 2;
     }
     void Update_agent()
     {
          if (move_mode == 2) //in FixedUpdate()
               target_pos = chase_after.position;

          if (transform.position == target_pos)
          {
               agent.isStopped = true;
               move_mode = -1;
          }
          else
          {
               agent.isStopped = false;
               agent.destination = target_pos;

               agent.stoppingDistance = reach;
               agent.speed = moveSpeed;
               agent.angularSpeed = rotateSpeed;
               agent.acceleration = acc;
          }
     }


     // physics with navMesh  ---------------------------------------------------------------------
     Rigidbody rb;
     float disable_agent_duration = 0.1f;
     float t_enable_agent;
     public void On_add_force()
     {
          t_enable_agent = Time.time + disable_agent_duration;
          Use_agent_and_kinematic(false);
     }

     void Use_agent_and_kinematic(bool b)
     {
          rb.isKinematic = b;
          agent.enabled = b;

          //if (b)
          //     agent.nextPosition = transform.position;
     }


     // debug  ----------------------------------------------------------------------

     void OnDrawGizmosSelected()
     {
          if (!gizmos)
               return;

          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(transform.position, spot_range);
     }

}





// TEST move and rorate ---------------------------------------------------------------------

//public void Move_towards_TEST(Transform target, float range, float speedPct = 1f)
//{
//     if (moveSpeed == 0)
//          return;
//     if (Vector3.Distance(target.position, transform.position) < range)
//          return;

//     transform.position = Vector3.MoveTowards(
//          transform.position,
//          target.position,
//          moveSpeed * speedPct * Time.deltaTime);
//}

//public void Rotate_towards_TEST(Transform target)
//{
//     if (rotateSpeed == 0)
//          return;

//     var dir = target.position - transform.position;
//     dir.y = 0;
//     var rot = Quaternion.LookRotation(dir);
//     transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotateSpeed * Time.deltaTime);
//}







