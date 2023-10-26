using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class TEST_NavMeshAgent : MonoBehaviour
{

     public Vector3 velAgent__;


     [Header("Chase Player")]
     public bool chasePlayer;

     [Header("Warp")]
     public Transform warpPosition;
     public bool click_warp;
     public Transform nextPosition;
     public bool click_nextPos;

     [Header("STOP")]
     public bool SetVel0;
     public bool StopNow;
     public bool BREAK;

     [Header("Vel")]
     public Vector3 velocity;
     public bool use_velocity;

     [Header("???")]
     public bool click_resetPath;
     public float remainingDistance;
     public bool hasPath;
     public bool pathPending;
     public bool isPathStale;
     public NavMeshPathStatus pathStatus;
     public bool isStopped;

     //private
     NavMeshAgent agent;
     Rigidbody rb;
     HPComponent player;


     void Awake()
     {
          //if (NetworkManager.Singleton.IsServer)
          //{

          //}

          agent = GetComponent<NavMeshAgent>();
          rb = GetComponent<Rigidbody>();
     }


     void FixedUpdate()
     {
          if (Time.time < tAddForce + forceMinDuration)
               Use_agent_and_kinematic(false);
          else if (rb.velocity != Vector3.zero)
               Use_agent_and_kinematic(false);
          else
               Use_agent_and_kinematic(true);
     }


     void Update()
     {
          velAgent__ = agent.velocity;

          if (agent.enabled == false)
               return;

          //stop
          if (SetVel0) //stop without inertia
          {
               SetVel0 = false;
               agent.velocity = Vector3.zero;
               return;
          }

          if (StopNow) //stop without inertia (0 distance teleport)
          {
               agent.destination = transform.position;
               return;
          }

          agent.isStopped = BREAK; //stop with inertia
          if (BREAK)
               return;

          //velocity
          if (use_velocity)
          {
               agent.velocity = velocity;
          }

          //destination
          if (chasePlayer)
          {
               foreach (var thing in FindObjectsOfType<HPComponent>())
               {
                    if (thing.team == CharaTeam.player_main_chara)
                    {
                         player = thing;
                         break;
                    }
               }
               agent.destination = player.transform.position;
          }
          else if (click_nextPos) //teleport, to closest pos if destination is not connected to NavMesh
          {
               click_nextPos = false;
               agent.nextPosition = nextPosition.position; //note teleport will push away/up physical object
          }
          else if (click_warp) //teleport
          {
               click_warp = false;
               agent.Warp(warpPosition.position);
          }


          remainingDistance = agent.remainingDistance;
          hasPath = agent.hasPath;
          pathPending = agent.pathPending;
          isPathStale = agent.isPathStale;
          pathStatus = agent.pathStatus;
          isStopped = agent.isStopped;


          if (click_resetPath)
          {
               click_resetPath = false;
               agent.ResetPath();
          }

     }


     // mix with physics  -------------------------------------------------------

     float tAddForce;
     float forceMinDuration = 0.1f;

     public void OnAddForce(Vector3 _force)
     {
          tAddForce = Time.time;
          Use_agent_and_kinematic(false);
     }

     void Use_agent_and_kinematic(bool b)
     {
          rb.isKinematic = b;
          agent.enabled = b;

          //if (b)
          //     agent.nextPosition = transform.position;
     }

}
