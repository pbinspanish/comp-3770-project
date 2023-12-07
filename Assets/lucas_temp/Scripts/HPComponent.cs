using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


public enum CharaTeam
{
     player_main_chara = 10,
     enemy = 20,
     terrain = 30, //NPC?
}


public class HPComponent : MonoBehaviour
{

     // Anything that has HP - player / NPC / breakable wall
     // - handle HP
     // - friend or foe
     // - layer mask for damage collision



     public string monitor;

     // setting
     public int set_max_hp = 10;
     public CharaTeam team = CharaTeam.enemy;
     public bool requireSiegeAttack; //soft terrain can only be damaged by siege attack

     // static
     public static List<HPComponent> players = new List<HPComponent>();

     // public
     public int maxHP { get; private set; }
     public int hp { get; private set; }
     public Action<int, int, int, int> On_damage_or_heal; // (damage, hp was, hp is, attackerID), eg. you deal 999 damage to X who has 20 HP. Then damage=999, hp was 20, is 0
     public Action<int, int> On_config_hp; // (hp, maxHP), for Init HP or passive +maxHP, or similar but isn't a heal
     public Action On_death_blow;
     public float t_last_damaged;


     // private
     // NetworkVariable<int> _id = new NetworkVariable<int>();


     // initial  ----------------------------------------------------------------------------

     void Start()
    {
          Config_hp(set_max_hp, set_max_hp); //initial hp
    }


     // config HP  ----------------------------------------------------------------------------

     // for initial HP, or passive skill +maxHP. They are not healing so won't show UI number
     public void Config_hp(int newHP, int newMaxHP)
     {
          hp = newHP;
          maxHP = newMaxHP;

     }


     // damage or heal  ----------------------------------------------------------------------------

     /// <summary> If player is attacking, include attackerID or AI wouldn't know who </summary>
     public void Damage(int value)
     {
          hp -=value;
          if(hp<=0){
               die();
          }
     }
     public void Heal(int value)
     {
          if(hp+value>maxHP){
               hp=maxHP;
          }
          else{
               hp+=value;
          }
     }

     void die()
     {
          if (gameObject.CompareTag("Player"))
          {
               // PlayerMove.respawn();
          }
          else
          {
               Destroy(gameObject);
          }
     }


     // friend or foe  ----------------------------------------------------------------------------
     public bool IsEnemy(CharaTeam team)
     {
          return this.team != team;
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
