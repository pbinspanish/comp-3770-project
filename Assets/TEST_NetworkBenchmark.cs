using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class TEST_NetworkBenchmark : NetworkBehaviour
{
     // test frequency of NetworkVariable and RPC

     [Header("Debug")]
     public int clientCount;
     public bool click_to_reset;

     [Header("Server")]
     public NetworkVariable<float> U = new NetworkVariable<float>();
     public NetworkVariable<float> F = new NetworkVariable<float>();
     public int serverTick;
     public int serverTick_F;

     [Header("Client")]
     public int clientTick;
     public int clientTick_F;
     public int clientTick_UinF;
     public int clientTick_FinU;

     [Header("Time difference")]
     public NetworkVariable<float> serverTime = new NetworkVariable<float>();
     public float serverTime0;
     public float serverTime_inF0;

     float U0;
     float F0;
     float U_inF0;
     float F_inU0;

     void Update()
     {
          serverTime0 = serverTime.Value;

          if (click_to_reset)
          {
               click_to_reset = false;

               serverTick = 0;
               serverTick_F = 0;
               clientTick = 0;
               clientTick_F = 0;
               clientTick_UinF = 0;
               clientTick_FinU = 0;
               U0 = 0;
               F0 = 0;
               U_inF0 = 0;
               F_inU0 = 0;
          }

          if (NetworkManager.Singleton.IsServer)
          {
               serverTime.Value = Time.time;

               clientCount = NetworkManager.Singleton.ConnectedClients.Count;
               if (clientCount > 1)
               {
                    U.Value++;
                    serverTick++;
               }
          }

          if (NetworkManager.Singleton.IsClient)
          {
               if (U0 != U.Value)
               {
                    U0 = U.Value;
                    clientTick++;
               }

               if (F_inU0 != F.Value)
               {
                    F_inU0 = F.Value;
                    clientTick_FinU++;
               }
          }
     }
     void FixedUpdate()
     {
          serverTime_inF0 = serverTime.Value;

          if (NetworkManager.Singleton.IsServer)
          {
               clientCount = NetworkManager.Singleton.ConnectedClients.Count;
               if (clientCount > 1)
               {
                    F.Value++;
                    serverTick_F++;
               }
          }

          if (NetworkManager.Singleton.IsClient)
          {
               if (F0 != F.Value)
               {
                    F0 = F.Value;
                    clientTick_F++;
               }

               if (U_inF0 != U.Value)
               {
                    U_inF0 = U.Value;
                    clientTick_UinF++;
               }
          }
     }

     //public override void OnNetworkSpawn()
     //{
     //     Debug.Log("Spawn: " + IsServer);
     //}
     //public override void OnNetworkDespawn()
     //{
     //     Debug.Log("Despawn: " + IsServer);
     //}





}
