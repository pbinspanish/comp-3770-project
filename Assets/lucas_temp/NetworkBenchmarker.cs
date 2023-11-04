using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class NetworkBenchmark : NetworkBehaviour
{
     // test frequency of NetworkVariable and RPC

     public bool inUse;
     public bool click_to_reset;
     public float duration;


     [Header("Server")]
     public int serverTick;
     public int serverTick_F;
     public float serverTickPerSec;
     public float serverTickPerSec_F;

     [Header("Client - NetVar")]
     public int clientTick;
     public int clientTick_F;
     public int clientTick_UinF;
     public int clientTick_FinU;
     public float clientTickPerSec;
     public float clientTickPerSec_F;
     public float clientTickPerSec_UinF;
     public float clientTickPerSec_FinU;

     [Header("Client - RPC")]
     public int RPCTick;
     public int RPCTick_F;
     public float RPCTickPerSec;
     public float RPCTickPerSec_F;


     // private
     bool isServer { get => NetworkManager.Singleton.IsServer; }
     bool isClient { get => NetworkManager.Singleton.IsClient; }
     bool isOnlyClient { get => !isServer && NetworkManager.Singleton.IsClient; }
     bool hasClient { get => isServer ? (NetworkManager.Singleton.ConnectedClients.Count > 1) : false; }

     NetworkVariable<float> U = new NetworkVariable<float>();
     NetworkVariable<float> F = new NetworkVariable<float>();
     NetworkVariable<float> serverTime = new NetworkVariable<float>();

     float U0;
     float F0;
     float U_inF0;
     float F_inU0;


     void Update()
     {
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
               duration = 0;
               RPCTick = 0;
               RPCTick_F = 0;
          }

          if (isServer && hasClient)
          {
               serverTime.Value = Time.time;

               U.Value++;
               serverTick++;
               serverTickPerSec = serverTick / duration;

               SendData_ClientRpc();
          }

          if (isOnlyClient || (isServer && hasClient))
          {
               duration += Time.deltaTime;

               if (U0 != U.Value)
               {
                    U0 = U.Value;
                    clientTick++;
                    clientTickPerSec = clientTick / duration;
               }

               if (F_inU0 != F.Value)
               {
                    F_inU0 = F.Value;
                    clientTick_FinU++;
                    clientTickPerSec_FinU = clientTick_FinU / duration;
               }
          }
     }
     void FixedUpdate()
     {
          if (isServer && hasClient)
          {
               F.Value++;
               serverTick_F++;
               serverTickPerSec_F = serverTick_F / duration;

               SendData_F_ClientRpc();
          }

          if (isOnlyClient || (isServer && hasClient))
          {
               if (F0 != F.Value)
               {
                    F0 = F.Value;
                    clientTick_F++;
                    clientTickPerSec_F = clientTick_F / duration;
               }

               if (U_inF0 != U.Value)
               {
                    U_inF0 = U.Value;
                    clientTick_UinF++;
                    clientTickPerSec_UinF = clientTick_UinF / duration;
               }
          }
     }

     [ClientRpc]
     void SendData_ClientRpc()
     {
          RPCTick++;
          RPCTickPerSec = RPCTick / duration;
     }
     [ClientRpc]
     void SendData_F_ClientRpc()
     {
          RPCTick_F++;
          RPCTickPerSec_F = RPCTick_F / duration;
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
