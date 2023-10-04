using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using System;

public class PlayerChara : NetworkBehaviour
{

     // player's charactor, controlled by PlayerController


     // public
     public static PlayerChara me; //the charactor you control. everyone else is controlled by server

     public Rigidbody rb { get; private set; }
     public CapsuleCollider col { get; private set; } //physical collider, smoothed
     public CapsuleCollider ghostCol { get; private set; } //trigger collider, no smooth, for spell hit / pick up item / map event etc..

     //public Action<Collider> OnTriggerEnter_ { set => ghost.OnTriggerEnter_ = value; }
     //public Action<Collider> OnTriggerExit_ { set => ghost.OnTriggerExit_ = value; }
     //public Action<Collider> OnTriggerStay_ { set => ghost.OnTriggerStay_ = value; }

     // private
     NetworkVariable<Vector3> netPos = new(writePerm: NetworkVariableWritePermission.Owner);
     NetworkVariable<Quaternion> netRot = new(writePerm: NetworkVariableWritePermission.Owner);
     PlayerChara_GhostCollider ghost;


     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();

          if (IsOwner)
               me = this;

          rb = GetComponent<Rigidbody>();
          col = GetComponent<CapsuleCollider>();
          col.isTrigger = false;

          ghost = GetComponentInChildren<PlayerChara_GhostCollider>();
          ghostCol = ghost.GetComponent<CapsuleCollider>();
          ghostCol.isTrigger = true;

          gameObject.layer = LayerMask.NameToLayer("Player");
          ghost.gameObject.layer = LayerMask.NameToLayer("PlayerGhost");
     }

     void FixedUpdate()
     {
          if (IsOwner)
          {
               netRot.Value = transform.rotation;
               netPos.Value = transform.position;
          }
          else
          {
               transform.rotation = netRot.Value;
               SmoothPos();
          }
     }


     // smooth --------------------------------------------------------------------------------------------
     Vector3 _vel;
     void SmoothPos()
     {
          if (transform.position == netPos.Value)
               return;

          if (Vector3.Distance(transform.position, netPos.Value) <= GlobalSetting.singleton.clientMaxDeviation)
          {
               // smooth
               transform.position = Vector3.MoveTowards(transform.position, netPos.Value, GlobalSetting.singleton.clientSmoothFlat * Time.fixedDeltaTime);
               transform.position = Vector3.SmoothDamp(transform.position, netPos.Value, ref _vel, GlobalSetting.singleton.clientSmooth, float.MaxValue, Time.fixedDeltaTime);
          }
          else
          {
               transform.position = netPos.Value; //no smooth
          }

          ghostCol.transform.position = netPos.Value; // collider with no smooth
     }


     // RPC --------------------------------------------------------------------------------------------

     [ServerRpc(RequireOwnership = false)]
     public void ChangeColor_ServerRpc(Color color) //just for fun
     {
          ChangeColor_ClientRpc(color);
     }
     [ClientRpc]
     void ChangeColor_ClientRpc(Color color)
     {
          GetComponentInChildren<Renderer>().material.color = color;
     }



}


