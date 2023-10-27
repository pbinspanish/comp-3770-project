using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.AI;
using System.Linq;


[RequireComponent(typeof(NetworkChara))]
[RequireComponent(typeof(HPComponent))]
public class AIBrain : MonoBehaviour
{

     // How to use:
     // 1) attach an AIState component to GameObject
     // 2) the order of AIState = priority

     // static
     public static int tDespoawn = 5; //sec


     //public
     public bool gizmos;
     public bool log;
     public bool enable_offline_AI = true; //switch on AI when not connected


     [Header("Setting")]
     public float move_speed = 10f;
     public float acc = 20f;
     public float rotate_speed = 120f;
     public float mass = 1f; //usually 1, larger mass = less vulnerable to force


     [Header("Alert")]
     public float spot_range = 20; //how far will AI spot you?
     public float alert_nearby = 20; // alert nearby ally when spot enemy


     // public
     public bool is_idle { get => current == null; }


     // private
     List<AIState> states;
     AIState current;
     HPComponent hp;
     Rigidbody rb;
     NavMeshAgent ghost;


     void Awake()
     {
          states = new List<AIState>();
          states.AddRange(GetComponents<AIState>()); //find all state

          hp = GetComponent<HPComponent>();
          hp.On_damage_or_heal += On_surprise_attack;
          hp.On_death_blow += Die;

          rb = GetComponent<Rigidbody>();

          ghost = GetComponentInChildren<NavMeshAgent>();
          ghost.transform.parent = null; //detach ghost
          ghost.acceleration = float.MaxValue;

          TEST.OnConnect += On_connect;
          TEST.OnDisconnect += On_local_mode;

          On_local_mode();

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

     void FixedUpdate()
     {
          if (is_moving)
          {
               ghost.isStopped = false;

               Update_pos();
               Update_rotation();
          }
          else
          {
               ghost.isStopped = true;
          }

          is_moving = false;
     }

     void OnDestroy()
     {
          Destroy(ghost);
     }

     // network  ----------------------------------------------------------------------
     void On_connect()
     {
          enabled = ghost.enabled = NetworkManager.Singleton.IsServer;
     }
     void On_local_mode()
     {
          enabled = ghost.enabled = enable_offline_AI;
     }

     // state machine ----------------------------------------------------------------------
     void Update_state()
     {
          AIState next = Decide_next_state();

          if (next == null)
          {
               current = null; //dile
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
     public List<AITargetData> targets { get => Update_target_list(); }
     List<AITargetData> _targets = new List<AITargetData>();

     bool _updated;
     List<AITargetData> Update_target_list()
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

                    // good
                    var found = _targets.Find(x => x.hp == chara);
                    if (found == null) // new target?
                    {
                         found = new AITargetData();
                         found.hp = chara;
                         _targets.Add(found);
                    }

                    // update dist
                    found.dist = dist;
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


     // alert  ----------------------------------------------------------------------
     bool hasAlert;
     void Alert_nearby()
     {
          if (!hasAlert && targets.Count > 0)
          {
               hasAlert = true;
               Debug.Log("Yeeeha! " + gameObject.name);
          }
     }

     void On_surprise_attack(int value, int hpWas, int hpIs, int attackID)
     {
          if (is_idle && value < 0)
          {
               var attacker = HPComponent.all.Find(x => x.id == attackID);

               if (attacker == null)
                    return;

               if (!_targets.Exists(x => x.hp.id == attackID))
               {
                    var data = new AITargetData();
                    data.hp = attacker;
                    _targets.Add(data);
               }
               else
               {
                    Debug.LogError("this should not happen??");
               }
          }
     }


     // die  ----------------------------------------------------------------------
     async void Die()
     {
          await Task.Delay(tDespoawn * 1000);
          Despawn();
     }

     void Despawn()
     {
          Destroy(gameObject);
     }


     // using navMesh  ---------------------------------------------------------------------
     bool is_moving;
     Vector3 target_pos;
     float speed_pct;
     public void Move(Vector3 pos, float stopping_distance, float _speed_pct = 100)
     {
          is_moving = true;
          target_pos = pos;
          speed_pct = _speed_pct;

          //if (Vector3.Distance(pos, transform.position) < stopping_distance)
          //     return;

          //var speed = move_speed * speed_pct / 100 * Time.deltaTime;
          //transform.position = Vector3.MoveTowards(transform.position, pos, speed);
     }
     //public void Rotate_towards_TEST(Transform target)
     //{
     //     var dir = target.position - transform.position;
     //     dir.y = 0;
     //     var rot = Quaternion.LookRotation(dir);
     //     transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotate_speed * Time.deltaTime);
     //}

     public float ghost_max_dist = 0.5f;
     public float ghost_jitter_filter = 0.1f;
     public float ghost_rotate_slerp = 4f;

     void Update_pos()
     {
          ghost.destination = target_pos;
          ghost.speed = move_speed * speed_pct / 100;

          // if ghost too far, snap back
          var dist = Vector3.Distance(transform.position, ghost.transform.position);
          if (dist > ghost_max_dist)
          {
               ghost.transform.position =
                    transform.position
                    + (ghost.transform.position - transform.position).normalized * ghost_max_dist;
          }

          // apply pos
          if (dist > ghost_jitter_filter)
          {
               var speed = move_speed * speed_pct / 100 * Time.fixedDeltaTime;
               transform.position = Vector3.MoveTowards(transform.position, ghost.nextPosition, speed);
          }

          // reset
          speed_pct = 100;
     }

     void Update_rotation()
     {
          var to_player = Quaternion.LookRotation(target_pos - transform.position);
          var to_corner = Quaternion.LookRotation(ghost.steeringTarget - transform.position);
          to_player = Quaternion.Lerp(to_player, to_corner, 0.5f); //average

          transform.rotation = Quaternion.Slerp(transform.rotation, to_player, ghost_rotate_slerp * Time.fixedDeltaTime);
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











