using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using System;


public class PlayerChara : NetworkBehaviour
{

     // this is player's charactor, phyically moving in the game world
     // controlled by PlayerController


     // public
     public static PlayerChara mine;
     public Rigidbody rb { get; private set; }
     public Renderer model { get; private set; }

     // net var
     NetworkVariable<Vector3> nPos = new(writePerm: NetworkVariableWritePermission.Owner);
     NetworkVariable<Quaternion> nRot = new(writePerm: NetworkVariableWritePermission.Owner);

     // private
     readonly static float smooth = 0.05f;
     Vector3 _vel;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }


     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();

          if (IsOwner)
               mine = this;

          rb = GetComponent<Rigidbody>();
          model = GetComponentInChildren<Renderer>();
     }


     void Update()
     {
          if (IsOwner)
          {
               nPos.Value = transform.position;
               nRot.Value = transform.rotation;
          }
          else
          {
               transform.position = Vector3.SmoothDamp(transform.position, nPos.Value, ref _vel, smooth);
               transform.rotation = nRot.Value;
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void ChangeColor_ServerRpc(Color color) //just for fun
     {
          ChangeColor_ClientRpc(color);
     }

     [ClientRpc]
     void ChangeColor_ClientRpc(Color color)
     {
          model.material.color = color;
     }



}


