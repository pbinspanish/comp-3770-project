using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



public class HPComponent : NetworkBehaviour
{

     // handle hp for player, enemy or cucumber alike


     //public
     public int maxHP = 5; //TODO: migrate to some status class
     public Action<int> OnHpChange;
     public Action<int> OnAnyDamageHeal; //will call even 0 damage, 0 heal, or hitting dead enemy
     public Action<int> OnDeathBlow;
     public Action<int> OnRevive;


     //private
     public int hp { get; private set; }
     int was;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }


     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();
          hp = was = maxHP;

          OnHpChange?.Invoke(maxHP);
     }

     //public
     public void UpdateHP(int delta)
     {
          var data = new NetPackage();
          data.delta = delta;
          data.originClientID = clientID;

          _updateHP(data);
          UpdateHP_ServerRPC(data);
     }


     //private
     void _updateHP(NetPackage data)
     {
          OnAnyDamageHeal?.Invoke(data.delta);

          if (data.delta == 0)
               return;

          was = hp;
          hp = Mathf.Clamp(hp + data.delta, 0, maxHP);

          if (was == hp)
               return;

          //events
          OnHpChange?.Invoke(data.delta);
          if (hp == 0) OnDeathBlow?.Invoke(data.delta);
          if (was == 0 && hp > 0) OnRevive?.Invoke(data.delta);
     }


     //RPC
     [ServerRpc(RequireOwnership = false)]
     void UpdateHP_ServerRPC(NetPackage data)
     {
          UpdateHP_ClientRPC(data);
     }

     [ClientRpc]
     void UpdateHP_ClientRPC(NetPackage data)
     {
          if (clientID != data.originClientID) // everyone except the origin caller
               _updateHP(data);
     }

     struct NetPackage : INetworkSerializable
     {
          public ulong originClientID;
          public int delta;

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref originClientID);
               serializer.SerializeValue(ref delta);
          }
     }



}
