using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Handle hp for player, enemy or cucumber alike
/// </summary>
public class HPComponent : NetworkBehaviour, IDamageAble
{

     //public
     public int hpMax = 5; //TODO: migrate to some status class
     public Action<int> OnHpChange;
     public Action OnDeathBlow;
     public Action OnRevive;


     //private
     public int hp { get; private set; }
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     float IDamageAble.hp => hp;
     float IDamageAble.hpMax => hpMax;


     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();

          hp = hpMax;
          OnHpChange?.Invoke(hp);
     }

     //public
     public void DeltaHP(int delta)
     {
          var data = new NetPackage();
          data.hp = hp + delta;
          data.maxHP = hpMax;
          data.originClientID = clientID;

          _updateHP(data);
          UpdateHP_ServerRPC(data);
     }

     public void UpdateMaxHP(int maxHP)
     {
          var data = new NetPackage();
          data.hp = hp;
          data.maxHP = maxHP;
          data.originClientID = clientID;

          _updateHP(data);
          UpdateHP_ServerRPC(data);
     }

     //private
     void _updateHP(NetPackage data)
     {
          var was = hp;

          // max hp
          if (hpMax != data.maxHP)
          {
               hpMax = data.maxHP;
          }

          // don't trust data, check first
          var newHP = Mathf.Clamp(data.hp, 0, data.maxHP);

          if (hp != newHP)
          {
               hp = newHP;
               OnHpChange?.Invoke(hp);

               if (hp == 0)
                    OnDeathBlow?.Invoke();
               if (was == 0 && hp > 0)
                    OnRevive?.Invoke();
          }
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
          public int hp;
          public int maxHP;

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref originClientID);
               serializer.SerializeValue(ref hp);
               serializer.SerializeValue(ref maxHP);
          }
     }



}
