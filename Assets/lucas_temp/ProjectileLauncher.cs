using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Pool;


public class ProjectileLauncher : NetworkBehaviour
{
     public GameObject prefab;
     public bool log = false;


     [Header("Damage")]
     public int damage = 2;
     public int AoEDamage = 2;
     public int AoERadius = 10;


     [Header("Projectile")]
     public float velocity = 10f;
     public int penetrate = 0; //1 = penetrate once
     public Vector2 force;//push target away or upwards
     public float despawnRange = 1000;
     public float despawnInSec = 10f;


     [Header("Pool")]
     public int poolCount;
     public int poolActive;

     //private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }


     void Awake()
     {
          InitPool();
     }

     void Update()
     {
          poolCount = pool.CountAll;
          poolActive = pool.CountActive;

          if (Input.GetKeyDown(KeyCode.Mouse0) && PlayerChara.mine != null)
          {
               Fire();
          }
     }



     // fire ---------------------------------------------------------------------------------

     public void Fire()
     {
          //this call is made by the original client initial the Fire()

          if (log) TEST.GUILog("----------------------------------------------");
          if (log) TEST.GUILog("Client " + clientID + " - Fire(): (0) Start");

          var pos = PlayerChara.mine.transform.position;
          var offset = new Vector3(0, 1, 0) + 1 * PlayerChara.mine.transform.forward.normalized;
          var dir = PlayerChara.mine.transform.forward;

          var data = new Package();
          data.clientID = clientID;
          data.start = pos + offset;
          data.direction = dir;

          FireVFX(data);
          Fire_ServerRPC(data);
     }

     void FireVFX(Package data)
     {
          if (log) TEST.GUILog("----------------------------------------------");
          if (log) TEST.GUILog("Client " + clientID + ": FireVFX()");

          var p = pool.Get();
          p.Fire(data.start, data.direction);
     }

     [ServerRpc(RequireOwnership = false)]
     void Fire_ServerRPC(Package data)
     {
          if (log) TEST.GUILog("Client " + clientID + ": OnFire_ServerRPC()");
          FireVFX_ClientRPC(data);
     }

     [ClientRpc]
     void FireVFX_ClientRPC(Package data)
     {
          if (clientID != data.clientID) // everyone except the origin sender
          {
               FireVFX(data);
          }
     }

     struct Package : INetworkSerializable
     {
          public ulong clientID;
          public Vector3 start;
          public Vector3 direction;

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref clientID);
               serializer.SerializeValue(ref start);
               serializer.SerializeValue(ref direction);
          }
     }


     // on hit ---------------------------------------------------------------------------------

     public void AfterHit()
     {
          if (IsServer)
          {
               if (log) TEST.GUILog("Client " + clientID + ": OnHit(), Server register a hit!");

               AfterHit_ClientRPC();
               // this call is for visual
               // on Server, bullet will do the damage, then NetworkVariable will update states for client
          }
          else
          {
               return;

               // TODO: (consider)
               // 
               // here allow client show HP loss instantly, and sync later (client is probably right)
               // but IMPORTANT event like death/loot should only be handled by server
          }
     }

     [ClientRpc]
     void AfterHit_ClientRPC()
     {
          // TODO: (consider)
          //
          // to compensate visually in case object are diverged on the client side: (because high speed?)
          //  - a bullet may instantly travels the last bit distance
          //  - or make a very sharp lerp
     }



     // pool ---------------------------------------------------------------------------------

     ObjectPool<ProjectileAddon> pool;
     int poolStartSize = 50; // resize can be expensive
     int poolMaxSize = 500;

     void InitPool()
     {
          pool = new ObjectPool<ProjectileAddon>(CreateNewProjectile, null, null, null, false, poolStartSize, poolMaxSize);
     }

     ProjectileAddon CreateNewProjectile()
     {
          var gameobj = Instantiate(prefab, transform);

          var mono = gameobj.GetComponent<ProjectileAddon>();
          mono.pool = pool;
          mono.mgr = this;
          mono.AfterHit = AfterHit;

          mono.gameObject.SetActive(false);
          return mono;
     }




}

