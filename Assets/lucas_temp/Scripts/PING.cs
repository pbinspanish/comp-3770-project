using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

public class PING : NetworkBehaviour
{

     public float pingInterval = 0.5f;
     public bool click_to_reset;
     [Header("RTT")]
     public float RTT;
     public float RTTmax;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }


     //private
     List<int> _remove = new List<int>();

     public override void OnNetworkSpawn()
     {
          pingList.Clear();
     }

     void Update()
     {
          if (click_to_reset)
          {
               click_to_reset = false;
               RTT = 0;
               RTTmax = 0;
          }

          Ping();
     }

     //private  ----------------------------------------------------------------------------
     Dictionary<int, float> pingList = new Dictionary<int, float>();
     public int pingID;
     float tNextPing;

     void Ping()
     {
          if (!IsSpawned)
               return; //this class attacked to managers, so it can start Update() before 'Spawned'

          if (!NetworkManager.Singleton.IsClient)
               return;

          if (Time.realtimeSinceStartup >= tNextPing)
          {
               tNextPing = Time.realtimeSinceStartup + pingInterval;

               pingList.Add(pingID, Time.realtimeSinceStartup);
               Ping_ServerRPC(pingID, clientID);

               pingID++;
          }
     }

     [ServerRpc(RequireOwnership = false)]
     void Ping_ServerRPC(int _pingID, ulong sender)
     {
          Pong_ClientRPC(_pingID, sender);
     }
     [ClientRpc]
     void Pong_ClientRPC(int _pingID, ulong receiver)
     {
          if (NetworkManager.Singleton.IsServer || clientID != receiver)
               return;

          var pingTime = pingList[_pingID];
          pingList.Remove(_pingID);

          float rtt = (Time.realtimeSinceStartup - pingTime) * 1000;
          RTT += (rtt - RTT) / 2; //average RTT

          //all time RTT and max
          if (rtt > RTTmax)
               RTTmax = rtt;
     }




}
