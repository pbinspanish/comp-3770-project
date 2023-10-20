using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;


/// <summary>
/// Controlled by owner, smooth movement on non-owner.
/// Must attach to and spawn with the player/enemy GameObject.
/// </summary>
[RequireComponent(typeof(HPComponent))]
public class NetworkChara : NetworkBehaviour
{

     // setup before use
     [Header("Player")]
     public bool isPlayer;
     public bool isEnemy = true;


     // public
     public static NetworkChara myChara; //ref for client's charactor
     public static List<NetworkChara> list = new List<NetworkChara>(); //include player and enemy
     public Rigidbody rb { get; private set; } //to control this charactor
     public Collider col { get; private set; }

     // private
     NetworkVariable<Vector3> netPos = new(writePerm: NetworkVariableWritePermission.Owner);
     NetworkVariable<Quaternion> netRot = new(writePerm: NetworkVariableWritePermission.Owner);
     bool isSpawned;


     // init ---------------------------------------------------------------------------------
     public override void OnNetworkSpawn()
     {
          rb = GetComponent<Rigidbody>();
          col = GetComponent<Collider>();

          if (IsOwner && isPlayer)
               if (myChara == null)
                    myChara = this;
               else
                    Debug.LogError("There should be only one player chara");

          if (isPlayer)
               gameObject.layer = LayerMask.NameToLayer("Player");
          else if (isEnemy)
          {
               gameObject.layer = LayerMask.NameToLayer("Enemy");
               GetComponentInParent<HPComponent>().On_death_blow += TEST_BecomeRagdoll; //just for fun
          }

          list.Add(this);

          isSpawned = true;
     }
     public override void OnNetworkDespawn()
     {
          if (IsOwner && myChara == this)
               myChara = null;

          list.Remove(this);

          isSpawned = false;
     }

     void FixedUpdate()
     {
          if (!isSpawned)
               return;

          if (IsOwner)
          {
               //Server controls all NPC, Player control his own PC
               netRot.Value = transform.rotation;
               netPos.Value = transform.position;
          }
          else
          {
               //Every other PC/NPC only update accordingly
               UpdateRot();
               UpdatePos();
          }
     }


     // smooth --------------------------------------------------------------------------------------------
     Vector3 _vel;
     SmoothSetting setting { get => SmoothSetting.inst; }
     Vector3 pos { get => transform.position; set => transform.position = value; }
     float speedCap { get => isPlayer ? PlayerStatus.singleton.maxValocity : float.MaxValue; }


     void UpdateRot()
     {
          transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, netRot.Value, setting.rotLerp * Time.fixedDeltaTime);
     }

     void UpdatePos()
     {
          if (pos == netPos.Value)
               return;

          if (Vector3.Distance(pos, netPos.Value) <= setting.maxDeviation)
          {
               var predict = pos + setting.extrapolation * (netPos.Value - pos); // TODO: linear extrapolation, if this works well consider non-linear one
               pos = Vector3.MoveTowards(pos, predict, setting.flatVelocity * Time.fixedDeltaTime);

               // default
               if (setting.mode == ClientSmoothMode.Default)
                    pos = Vector3.SmoothDamp(pos, predict, ref _vel, setting.smoothTime, float.MaxValue, Time.fixedDeltaTime);

               // speed cap at
               else if (setting.mode == ClientSmoothMode.SpeedCap)
                    pos = Vector3.SmoothDamp(pos, predict, ref _vel, setting.smoothTime, speedCap * setting.speedCapX, Time.fixedDeltaTime);

               // lerp
               else if (setting.mode == ClientSmoothMode.Lerp)
                    pos = Vector3.LerpUnclamped(pos, predict, setting.posLerp * Time.fixedDeltaTime);
          }
          else
          {
               pos = netPos.Value; //no smooth
          }
     }


     // fun for NPC --------------------------------------------------------------------------------------------
     void TEST_BecomeRagdoll()
     {
          var rb = GetComponent<Rigidbody>();

          if (rb)
          {
               rb.constraints = 0; //80 = freezeXZ, 0 = no contraint
               rb.mass /= 2f;
          }
     }


     // debug --------------------------------------------------------------------------------------------
     void OnDrawGizmos()
     {
          if (Vector3.Distance(transform.position, netPos.Value) > 0.1f)
          {
               Gizmos.color = Color.cyan;
               Gizmos.DrawWireCube(netPos.Value + new Vector3(0, 1, 0), new Vector3(1, 2, 1));
          }

     }


}


