using System;
using System.Collections;
using Unity.Netcode;
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
     public bool isPlayerMainCharactor = false;
     public bool layer_player;
     [Header("Enemy")]
     public bool layer_enemy;


     // public
     public static NetworkChara myChara; //ref for client's charactor
     public Rigidbody rb { get; private set; } //to control this charactor


     // private
     NetworkVariable<Vector3> netPos = new(writePerm: NetworkVariableWritePermission.Owner);
     NetworkVariable<Quaternion> netRot = new(writePerm: NetworkVariableWritePermission.Owner);
     bool isSpawned;



     // init ---------------------------------------------------------------------------------
     public override void OnNetworkSpawn()
     {
          rb = GetComponent<Rigidbody>();

          if (IsOwner && isPlayerMainCharactor)
               if (myChara == null)
                    myChara = this;
               else
                    Debug.LogError("There should be only one player chara");

          if (layer_player)
               gameObject.layer = LayerMask.NameToLayer("Player");
          else if (layer_enemy)
               gameObject.layer = LayerMask.NameToLayer("Enemy");

          isSpawned = true;
     }
     public override void OnNetworkDespawn()
     {
          isSpawned = false;
     }
     public float rbVEL;

     void FixedUpdate()
     {
          if (isSpawned == false)
               return;

          rbVEL = rb.velocity.magnitude;

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
     float speedCap { get => CharaStatus.singleton.speedCap; }


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
          Gizmos.color = Color.cyan;
          Gizmos.DrawWireCube(netPos.Value + new Vector3(0, 1, 0), new Vector3(1, 2, 1));
     }


}


