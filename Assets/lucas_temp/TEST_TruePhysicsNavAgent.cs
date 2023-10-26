using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


public class TEST_TruePhysicsNavAgent : NetworkBehaviour
{

     public bool chasePlayer;


     Rigidbody rb;
     NavMeshAgent agent;
     HPComponent player;


     void Start()
     {
          agent = GetComponent<NavMeshAgent>();
          rb = GetComponent<Rigidbody>();
     }

     void Update()
     {
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
          else
          {
               agent.destination = transform.position;
          }
     }





}

