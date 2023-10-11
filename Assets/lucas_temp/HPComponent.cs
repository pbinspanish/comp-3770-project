using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Handle hp for player or enemy
/// [!!!] Must attach to root GameObject
/// </summary>
[RequireComponent(typeof(NetObjectID))]
public class HPComponent : NetworkBehaviour
{

     // inspector
     public int TEST_SetMaxHp = 20;
     bool log = false;

     // public
     public int hpMax { get; private set; }
     public int hp { get; private set; }
     public Action OnDeathBlow;


     // private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     int networkObjID { get => idClass.id; }
     NetObjectID idClass;


     public override void OnNetworkSpawn()
     {
          ConfigHP(TEST_SetMaxHp, TEST_SetMaxHp);
          idClass = GetComponent<NetObjectID>();
     }

     public override void OnNetworkDespawn()
     {
          //
     }


     //public
     public void DamageOrHeal(int _delta) //damage- and heal+
     {
          UpdateHP(_delta, hpMax, true);
     }
     public void ConfigHP(int _newHP, int _newMaxHP) //will not trigger UI
     {
          UpdateHP(_newHP, _newMaxHP, false);
     }


     //private
     void UpdateHP(int _delta, int _hpMax, bool ui)
     {
          var data = new NetPackage();
          data.senderID = clientID;
          data.hpMax = _hpMax;
          data.delta = _delta;
          data.objNetID = ui ? networkObjID : -1;

          _apply(data);
          _apply_ServerRPC(data);

          if (log) Debug.Log("UpdateHP() " + _delta + "/" + hpMax);
     }

     void _apply(NetPackage data)
     {
          var was = hp;

          hpMax = data.hpMax;
          hp = Mathf.Clamp(hp + data.delta, 0, data.hpMax);

          if (was != 0 && hp == 0)
               OnDeathBlow?.Invoke();

          if (data.objNetID != -1)
               UIDamageTextMgr.DisplayDamageText(data.delta, data.objNetID);
     }


     //RPC
     [ServerRpc(RequireOwnership = false)]
     void _apply_ServerRPC(NetPackage data)
     {
          UpdateHP_ClientRPC(data);
     }
     [ClientRpc]
     void UpdateHP_ClientRPC(NetPackage data)
     {
          if (clientID != data.senderID) // everyone except the origin caller
               _apply(data);
     }

     struct NetPackage : INetworkSerializable
     {
          public ulong senderID;
          public int hpMax;
          public int delta;
          public int objNetID;

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref senderID);
               serializer.SerializeValue(ref hpMax);
               serializer.SerializeValue(ref delta);
               serializer.SerializeValue(ref objNetID);
          }
     }



}
