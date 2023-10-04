using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;


public class ProjectileLauncher : NetworkBehaviour
{
     // create and Fire() Projectile
     // provide Network service

     public KeyCode hotkey = KeyCode.Mouse0;
     public ProjectileSetting setting;

     bool log = false; //debug
     public string poolState; //debug

     //private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }


     void Awake()
     {
          Projectile.InitLayer(setting);

          InitPool();
     }
     void Update()
     {
          poolState = pool.CountActive + " Active | " + pool.CountAll + " Count";

          if (Input.GetKeyDown(hotkey) && PlayerChara.me != null)
               Fire();
     }


     // fire projectile ---------------------------------------------------------------
     public void Fire()
     {
          var data = new NetPackage();
          data.originClientID = clientID;
          data.pos = PlayerChara.me.transform.position;
          data.pos += setting.launchOffset + 1 * PlayerChara.me.transform.forward.normalized;
          data.dir = (PlayerController.mouseHit - data.pos).normalized;

          _fire(data);
          Fire_ServerRPC(data);
     }

     void _fire(NetPackage data)
     {
          var p = pool.Get();
          p.enabled = true;
          p.GetComponent<Projectile>().Fire(data.pos, data.dir, data.originClientID);
     }

     [ServerRpc(RequireOwnership = false)]
     void Fire_ServerRPC(NetPackage data)
     {
          FireVFX_ClientRPC(data);
     }

     [ClientRpc]
     void FireVFX_ClientRPC(NetPackage data)
     {
          if (clientID != data.originClientID) // everyone except the origin caller
               _fire(data);
     }

     struct NetPackage : INetworkSerializable
     {
          public ulong originClientID;
          public Vector3 pos;
          public Vector3 dir;

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref originClientID);
               serializer.SerializeValue(ref pos);
               serializer.SerializeValue(ref dir);
          }
     }


     // after hit ---------------------------------------------------------------------------------
     [ClientRpc]
     public void OnHit_ClientRPC(Vector3 projectilePos, Vector3 targetPos)
     {
          if (!IsServer)
               return;

          if (log) TEST.GUILog("OnHitVFX()");

          //TODO: despawn?   

     }



     // pool ---------------------------------------------------------------------------------

     ObjectPool<Projectile> pool;
     readonly static int pSize = 200;
     readonly static int pSizeCap = 500;

     void InitPool()
     {
          pool = new ObjectPool<Projectile>(CreateNewProjectile, null, null, null, false, pSize, pSizeCap);
     }

     Projectile CreateNewProjectile()
     {
          var gameObj = Instantiate(setting.projectile, transform);
          var p = gameObj.GetComponent<Projectile>();
          p.launcher = this;
          p.gameObject.SetActive(false);

          return p;
     }

     public void Recycle(Projectile p)
     {
          pool.Release(p);
     }




}

