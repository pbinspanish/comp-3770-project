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
public class NetworkChara : NetworkBehaviour
{

     // public static
     public static NetworkChara myChara; //charactor of this cliant

     // public
     public Rigidbody rb { get; private set; } //for controller
     public Collider col { get; private set; }

     // private
     NetworkVariable<Vector3> netPos = new(writePerm: NetworkVariableWritePermission.Owner);
     NetworkVariable<Quaternion> netRot = new(writePerm: NetworkVariableWritePermission.Owner);
     bool isSpawned;
     HPComponent hp;


     // init ---------------------------------------------------------------------------------
     public override void OnNetworkSpawn()
     {
          rb = GetComponent<Rigidbody>();
          col = GetComponent<Collider>();
          hp = GetComponent<HPComponent>();

          if (hp != null && hp.isPlayer && IsOwner)
               if (myChara == null)
                    myChara = this;
               else
                    Debug.LogError("There should be only one player chara");

          isSpawned = true;

     }
     public override void OnNetworkDespawn()
     {
          if (IsOwner && myChara == this)
               myChara = null;

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
     float speedCap { get => hp != null ? hp.isPlayer ? PlayerStatus.singleton.maxValocity : float.MaxValue : float.MaxValue; }


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


