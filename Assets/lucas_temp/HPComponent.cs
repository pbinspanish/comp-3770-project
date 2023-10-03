using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



public class HPComponent : NetworkBehaviour
{

     // handles HP for player charactor, enemy, or object like pot / chest


     // TODO: (ideally)
     // allow client mod HP ui locally before sync


     //public
     public int HP
     {
          get => _hp.Value;
          set { _hp.Value = Math.Clamp(value, 0, maxHP); }
     }
     public int maxHP = 5;
     public Action OnHpChange;
     public Action OnDeath;
     public Action OnRevive;

     //private
     [SerializeField] NetworkVariable<int> _hp = new(writePerm: NetworkVariableWritePermission.Server);



     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();

          _hp.OnValueChanged += OnValueChanged; //client
          OnHpChange?.Invoke(); //client manually refresh UI to catch up

          HP = maxHP; //server
     }

     void OnValueChanged(int was, int now)
     {
          if (was != now)
               OnHpChange?.Invoke();
          if (now == 0)
               OnDeath?.Invoke();
          if (was == 0 && now > 0)
               OnRevive?.Invoke();
     }


}
