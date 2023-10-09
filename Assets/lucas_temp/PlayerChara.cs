using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;


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

     void Awake()
     {
          rb = GetComponent<Rigidbody>();
          col = GetComponent<CapsuleCollider>();
          col.isTrigger = false;

          ghost = GetComponentInChildren<PlayerChara_GhostCollider>();
          ghostCol = ghost.GetComponent<CapsuleCollider>();
          ghostCol.isTrigger = true;

          gameObject.layer = LayerMask.NameToLayer("Player");
          ghost.gameObject.layer = LayerMask.NameToLayer("PlayerGhost");

          pingClass = FindObjectOfType<PING>();
     }

     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();

          if (IsOwner)
               me = this;
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

     float extrapolate { get => LerpMaster.inst.extrapolate; }
     // smooth --------------------------------------------------------------------------------------------
     Vector3 _vel;
     float speedCap { get => CharaStatus.singleton.speedCap * 1.5f; }
     PING pingClass;

     void SmoothPos()
     {
          if (transform.position == netPos.Value)
               return;

          if (Vector3.Distance(transform.position, netPos.Value) <= LerpMaster.inst.clientMaxDeviation)
          {
               // prodiction
               var prediction = transform.position + extrapolate * (netPos.Value - transform.position); // TODO: linear prediction, if this works then change to fancy curve

               // smooth
               transform.position = Vector3.MoveTowards(transform.position, prediction, LerpMaster.inst.clientSmoothFlatMove * Time.fixedDeltaTime);
               if (LerpMaster.inst.defaultMode)
                    transform.position = Vector3.SmoothDamp(transform.position, prediction, ref _vel, LerpMaster.inst.smoothTime, float.MaxValue, Time.fixedDeltaTime);

               //TEST: 1.5*speed as max speed
               else if (LerpMaster.inst.newSpeedCapMode)
                    transform.position = Vector3.SmoothDamp(transform.position, prediction, ref _vel, LerpMaster.inst.smoothTime, LerpMaster.inst.speedCapFactor * speedCap, Time.fixedDeltaTime);

               //TEST: RTT as smoothTime
               else if (LerpMaster.inst.RTT_LerpMode)
                    transform.position = Vector3.LerpUnclamped(transform.position, prediction, pingClass.RTT / 1000 * LerpMaster.inst.lerpFactor_RTT * Time.fixedDeltaTime);

               //TEST: Use lerp
               else if (LerpMaster.inst.LerpMode)
                    transform.position = Vector3.LerpUnclamped(transform.position, prediction, LerpMaster.inst.lerpFactor * Time.fixedDeltaTime);
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

     // debug --------------------------------------------------------------------------------------------


     void OnDrawGizmos()
     {
          Gizmos.color = Color.cyan;
          Gizmos.DrawWireCube(netPos.Value + new Vector3(0, 1, 0), new Vector3(1, 2, 1));
     }


}


