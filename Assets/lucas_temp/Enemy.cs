using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class Enemy : NetworkBehaviour
{

     // TEST class
     // this + HPComponent = almost a whole enemy!

     // public
     public Rigidbody rb { get; private set; }

     // net var
     //NetworkVariable<Vector3> nPos = new(writePerm: NetworkVariableWritePermission.Server);
     //NetworkVariable<Quaternion> nRot = new(writePerm: NetworkVariableWritePermission.Server);

     NetworkVariable<Vector3> nPos = new NetworkVariable<Vector3>();
     NetworkVariable<Quaternion> nRot = new NetworkVariable<Quaternion>();

     HPComponent hpClass;
     readonly static float smooth = 0.05f;
     Vector3 _vel;


     void Awake()
     {
          hpClass = GetComponentInParent<HPComponent>();
          rb = GetComponent<Rigidbody>();

          hpClass.OnDeath += OnDeath;
     }

     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();

          //set initial pos
          nPos.Value = transform.position;
          nRot.Value = transform.rotation;
     }

     void Update()
     {
          if (NetworkManager.Singleton.IsServer)
          {
               nPos.Value = transform.position;
               nRot.Value = transform.rotation;
          }
          else if (NetworkManager.Singleton.IsClient)
          {
               transform.position = Vector3.SmoothDamp(transform.position, nPos.Value, ref _vel, smooth);
               transform.rotation = nRot.Value;
          }
     }

     void OnDeath()
     {
          var rb = GetComponent<Rigidbody>();
          rb.constraints = 0; //0=constraint, 80=freezeXZ
     }



}
