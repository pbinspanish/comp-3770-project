using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Handle hp for player / enemy / object
/// [!] Must be on the same GameObject with colliders
/// </summary>
public class HPComponent : NetworkBehaviour
{

     // for targeting
     public static List<HPComponent> all = new List<HPComponent>();


     // TEST
     public string monitor;
     public int TEST_Set_max_hp = 10;


     public int maxHP { get; private set; }
     public int hp { get; private set; }

     // event  
     public Action<int, int, int> On_damage_or_heal; // (damage, hp was, hp is), eg you deal 999 damage to X who has 20 HP. Then damage=999, hp was 20, is 0
     public Action<int, int> On_config_hp; // (hp, maxHP), for Init HP or passive +maxHP, or similar but isn't a heal
     public Action On_death_blow;


     // initial  ----------------------------------------------------------------------------

     public override void OnNetworkSpawn()
     {
          Config_hp(TEST_Set_max_hp, TEST_Set_max_hp);
          all.Add(this);

          //check
          Debug.Assert(TEST_Set_max_hp > 0);
          if (isPlayer) Debug.Assert(gameObject.layer == LayerMask.NameToLayer("Player"));
          if (isEnemy) Debug.Assert(gameObject.layer == LayerMask.NameToLayer("Enemy"));
     }

     public override void OnNetworkDespawn()
     {
          all.Remove(this);
     }

     // friend or foe  ----------------------------------------------------------------------------

     public bool isPlayer;
     public bool isEnemy = true;

     public bool IsFriend(bool otherIsPlayer)
     {
          return otherIsPlayer == isPlayer;
     }


     // config HP  ----------------------------------------------------------------------------

     // for initial HP, or passive skill +maxHP. They are not healing so won't show UI number
     public void Config_hp(int newHP, int newMaxHP)
     {
          var packet = new ConfigPacket();
          packet.HP = newHP;
          packet.maxHP = newMaxHP;

          Config_hp_ServerRPC(packet);
     }

     [ServerRpc(RequireOwnership = false)]
     void Config_hp_ServerRPC(ConfigPacket data)
     {
          Config_hp_ClientRPC(data);
     }
     [ClientRpc]
     void Config_hp_ClientRPC(ConfigPacket data)
     {
          hp = data.HP;
          maxHP = data.maxHP;

          On_config_hp?.Invoke(hp, maxHP); //event
     }

     struct ConfigPacket : INetworkSerializable
     {
          public int HP;
          public int maxHP;
          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref HP);
               serializer.SerializeValue(ref maxHP);
          }
     }


     // damage or heal  ----------------------------------------------------------------------------

     public void Damage_or_heal(float delta, string damageType = "")
     {
          Debug.Assert(delta != 0);

          //delta: damage is - , healing is +
          var data = new DamageHealPacket();
          data.delta = Mathf.FloorToInt(delta * GetDamageTypeCoefficient(damageType) / 100f);

          Damage_or_heal_ServerRPC(data);
     }

     [ServerRpc(RequireOwnership = false)]
     void Damage_or_heal_ServerRPC(DamageHealPacket data)
     {
          Damage_or_heal_ClientRPC(data);
     }
     [ClientRpc]
     void Damage_or_heal_ClientRPC(DamageHealPacket data)
     {
          var was = hp;
          hp = Mathf.Clamp(hp + data.delta, 0, maxHP);

          monitor = hp + "/" + maxHP;
          On_damage_or_heal?.Invoke(data.delta, was, hp); //event
          UIDamageTextMgr.DisplayDamageText(data.delta, gameObject); //ui

          if (was != 0 && hp == 0)
          {
               if (isEnemy)
                    TEST_BecomeRagdoll();

               On_death_blow?.Invoke(); //event
          }
     }

     struct DamageHealPacket : INetworkSerializable
     {
          public int delta;
          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref delta);
          }
     }

     // damage type  ----------------------------------------------------------------------------
     public float VS_default = 100; // VS different damage types. In percentage
     public float VS_Siege = 100; // eg. boss destroy floor/terrain


     float GetDamageTypeCoefficient(string _type)
     {
          _type = _type.ToLower(); //ignore up or lower case

          if (_type == "" || _type == "default")
               return VS_default;
          else if (_type == "siege")
               return VS_Siege;

          throw new Exception("unknown type - if this is not a typo, add a new coefficient");
     }


     // fun for NPC --------------------------------------------------------------------------------------------
     void TEST_BecomeRagdoll()
     {
          var rb = GetComponent<Rigidbody>();
          if (rb)
          {
               rb.constraints = 0; //80 = freezeXZ, 0 = no contraint
               rb.mass /= 3f;
          }

          var fc = GetComponentInChildren<FaceCamera>();
          if (fc)
          {
               fc.End_of_use();
          }
     }



}
