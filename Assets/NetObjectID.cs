using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;


public class NetObjectID : NetworkBehaviour
{

     //identify the same object across different devices

     public int ID { get => _id.Value; }

     [TextArea]
     public string monitor;


     //private
     NetworkVariable<int> _id = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
     static Dictionary<int, NetworkBehaviour> dict = new Dictionary<int, NetworkBehaviour>();


     // public
     public static GameObject Find(int netObjectID)
     {
          if (dict.ContainsKey(netObjectID))
               return dict[netObjectID].gameObject;

          return null;
     }

     void Update()
     {
          monitor = "";
          foreach (var pair in dict)
          {
               monitor += pair.Key + "," + pair.Value.name + "\n";
          }

     }

     // init
     public override void OnNetworkSpawn()
     {
          if (IsOwner)
          {
               _id.Value = GetInstanceID();
               dict.Add(_id.Value, this);

               Debug.Log(gameObject.name + " | NetworkID = " + ID);
          }
     }

     public override void OnNetworkDespawn()
     {
          dict.Remove(_id.Value);
     }

}
