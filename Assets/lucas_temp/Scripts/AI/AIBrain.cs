using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.AI;
using System.Linq;
using Unity.VisualScripting;
using JetBrains.Annotations;

[RequireComponent(typeof(NetworkChara))]
[RequireComponent(typeof(HPComponent))]
public class AIBrain : MonoBehaviour
{

     // How to use:
     // 1) attach an AIState component to GameObject
     // 2) the order of AIState = priority

     // static
     public static int tDespoawn = 5; //sec


     // debug
     public string _monitor;
     public bool _gizmos;
     public bool _log;

     public bool enable_offline_AI = true; //switch on AI when not connected

     //public
     [Header("Setting")]
     public float move_speed = 10f;
     public float maxAcc = 20f; //agent dont use this acc, it has infinite acc to stay a bit ahead of us
     public float rotate_speed = 120f;


     [Header("Alert")]
     public float spot_range = 20; //how far will AI spot you?
     public float alert_nearby = 20; // alert nearby ally when spot enemy


     // public
     public bool is_idle { get => current == null; }


     // private
     public HPComponent hp { get; private set; }
     List<AIState> states;
     AIState current;
     Rigidbody rb;
     NavMeshAgent agent;


     void Awake()
     {
          states = new List<AIState>();
          states.AddRange(GetComponents<AIState>()); //find all state

          hp = GetComponent<HPComponent>();
          hp.On_damage_or_heal += On_surprise_attack;
          hp.On_death_blow += Die;

          rb = GetComponent<Rigidbody>();

          var physicMaterial = GetComponent<Collider>().material;
          physicMaterial.dynamicFriction = 0;
          physicMaterial.staticFriction = 0;
          physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;

          agent = GetComponentInChildren<NavMeshAgent>();
          agent.transform.parent = null; //detached 'ghost' agent
          agent.acceleration = float.MaxValue;

          TEST.OnConnect += On_connect;
          TEST.OnDisconnect += On_local_mode;

          update_pos = false;
          update_rot = false;
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
          Update_pos();
          Update_rotation();

          //force = Vector3.ClampMagnitude(force, force.sqrMagnitude * drag * Time.fixedDeltaTime); //force decay
     }

     void OnDestroy()
     {
          Destroy(agent);
     }


     // network  ----------------------------------------------------------------------
     void On_connect()
     {
          enabled = agent.enabled = NetworkManager.Singleton.IsServer;
     }
     void On_local_mode()
     {
          enabled = agent.enabled = enable_offline_AI;
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

          _monitor = current == null ? "Idle" : current.GetType().ToString();
     }

     AIState Decide_next_state()
     {
          foreach (var state in states)
               if (state.IsValid())
                    return state;

          return null; // = idle
     }


     // target list  ----------------------------------------------------------------------
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
          Destroy(gameObject);
     }


     // using navMesh  ---------------------------------------------------------------------
     public bool update_rot { get; set; }
     public bool update_pos { get; set; }
     public float agent_pos_devation = 0.5f;
     public float agent_rot_devation = 10f; //degree
     public float agent_jitter_filter = 0.1f;
     public float agent_rotate_slerp = 4f;

     public void Set_move_target(Vector3 pos, float stopping_distance)
     {
          update_pos = true;

          agent.destination = pos;
          agent.stoppingDistance = stopping_distance;
          agent.speed = move_speed;
          agent.angularSpeed = rotate_speed;
     }

     void Update_pos()
     {
          //log
          rb_velocity = rb.velocity;
          rb_velocity_mag = rb.velocity.magnitude;

          if (!update_pos)
               return;

          // if agent deviate too far, snap back
          var dist = Vector3.Distance(transform.position, agent.transform.position);
          if (dist > agent_pos_devation)
          {
               agent.transform.position =
                    transform.position
                    + (agent.transform.position - transform.position).normalized * agent_pos_devation;
          }

          // move
          if (dist > agent_jitter_filter)
          {
               //V1
               //transform.position = Vector3.MoveTowards(transform.position, agent.transform.position, move_speed * Time.fixedDeltaTime);


               //V2
               //var dir = (agent.nextPosition - transform.position).normalized;
               //vel += dir * acc * Time.fixedDeltaTime;
               //vel = Vector3.ClampMagnitude(vel, move_speed);

               //rb_velocity = rb.velocity = vel + force; //apply


               //V3
               var vel_desired = (agent.nextPosition - transform.position).normalized * move_speed;
               var acc = vel_desired - rb.velocity;
               acc.y = 0;
               acc = Vector3.ClampMagnitude(acc, maxAcc);


               acc__ = acc;
               accMag__ = acc.magnitude;
               vel_desired__ = vel_desired;
               vel_desired_mag__ = vel_desired.magnitude;

               rb.AddForce(acc, ForceMode.Acceleration);
          }
     }

     public Vector3 vel_desired__;
     public float vel_desired_mag__;

     public Vector3 acc__;
     public float accMag__;

     public Vector3 rb_velocity;
     public float rb_velocity_mag;


     void Update_rotation()
     {
          if (!update_rot)
               return;

          // if rotation deviate too far, snap back
          var angle = Quaternion.Angle(transform.rotation, agent.transform.rotation);
          if (angle > agent_rot_devation)
          {
               //agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, transform.rotation, 0.5f);
               agent.transform.rotation = transform.rotation;
          }

          var to_player = Quaternion.LookRotation(agent.destination - transform.position);
          var to_corner = Quaternion.LookRotation(agent.steeringTarget - transform.position);
          to_player = Quaternion.Lerp(to_player, to_corner, 0.5f); //average

          transform.rotation = Quaternion.Slerp(transform.rotation, to_player, agent_rotate_slerp * Time.fixedDeltaTime);
     }




     // debug  ----------------------------------------------------------------------
     void OnDrawGizmos()
     {
          if (!Application.isPlaying)
               return;

          if (!_gizmos)
               return;

          // alert range
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(transform.position, spot_range);


          // nav agent
          Gizmos.color = Color.cyan;
          Gizmos.DrawWireSphere(agent.transform.position, 0.3f);


          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(agent.destination, 0.23f);
          for (int i = 0; i < agent.path.corners.Length; i++)
          {
               Gizmos.color = Color.green;
               Gizmos.DrawWireSphere(agent.path.corners[i], 0.25f);
               if (i > 0)
               {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i - 1]);
               }
          }







     }







}











