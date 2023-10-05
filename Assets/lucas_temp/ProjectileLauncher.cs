using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Creates and Fire() Projectile.
/// Also see Projectile, ProjectileSetting.
/// </summary>
public class ProjectileLauncher : NetworkBehaviour
{

     //publics
     public KeyCode hotkey = KeyCode.Mouse0; //replace with input system later
     bool log = false; //debug
     public string poolDebug; //debug
     public string collideCastDebug; //debug
     public ProjectileSetting setting;


     //private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     float tFire;


     void Awake()
     {
          InitPool();
          setting.InitLayerMask();
     }
     void Update()
     {
          poolDebug = pool.CountActive + " Active | " + pool.CountAll + " Count";
          collideCastDebug = Projectile.countCapsule + " capsule | " + Projectile.countSphere + " sphere";

          if (PlayerChara.me != null)
          {
               HandleInput();
          }
     }

     void HandleInput()
     {
          if (Time.time < tFire)
               return;

          bool input = false;
          switch (setting.triggerMode)
          {
               case TriggerMode.KeyDown:
                    input = Input.GetKeyDown(hotkey); break;
               case TriggerMode.KeyUp:
                    input = Input.GetKeyUp(hotkey); break;
               case TriggerMode.FullAuto:
                    input = Input.GetKey(hotkey); break;
          }

          if (!input)
               return;

          if (setting.delay > 0)
               StartCoroutine(CallLater(Fire, setting.delay));
          else
               Fire();
     }

     IEnumerator CallLater(Action call, float ms)
     {
          yield return new WaitForSeconds(ms / 1000);

          call?.Invoke();
     }

     // fire projectile ---------------------------------------------------------------
     public void Fire()
     {
          tFire = Time.time + setting.cooldown / 1000;

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
          public float delay;

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref originClientID);
               serializer.SerializeValue(ref pos);
               serializer.SerializeValue(ref dir);
               serializer.SerializeValue(ref delay);
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

     void InitPool()
     {
          var size = 200;
          var sizeCap = 500;
          pool = new ObjectPool<Projectile>(CreateNewProjectile, null, null, null, false, size, sizeCap);
     }

     Projectile CreateNewProjectile()
     {
          var gameObject = Instantiate(setting.projectile, transform);
          gameObject.SetActive(false);

          var p = gameObject.GetComponent<Projectile>();
          p.launcher = this;

          return p;
     }

     public void Recycle(Projectile p)
     {
          pool.Release(p);
     }




}

