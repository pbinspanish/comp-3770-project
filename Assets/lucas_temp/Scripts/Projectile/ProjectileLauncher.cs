using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Creates and Fire() Projectile.
/// </summary>
public class ProjectileLauncher : NetworkBehaviour
{

     //publics
     public KeyCode hotkey = KeyCode.Mouse0; //replace with input system later
     public string monitor;
     public ProjectileEntry setting;


     //private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     float tFire;


     void Awake()
     {
          InitPool();
          InitPoolVFX();
          //setting.InitLayerMask();
     }
     void Update()
     {
          monitor = "Pool: " + pool.CountActive + " Active | " + pool.CountAll + " Count";

          if (NetworkChara.myChara != null)
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
               case TriggerMode.FullyAuto:
                    input = Input.GetKey(hotkey); break;
          }

          if (!input)
               return;


          FireProjectile();

     }

     // fire projectile ---------------------------------------------------------------
     public void FireProjectile(string launcherLayer = "")
     {
          tFire = Time.time + setting.cooldown / 1000;

          var data = new Packet();
          data.originClientID = clientID;
          data.delay = setting.delay;
          data.launcherLayer = launcherLayer;

          // position
          data.fireFrom = NetworkChara.myChara.transform.position;
          data.fireFrom += NetworkChara.myChara.transform.rotation * new Vector3(0, setting.spawnFwdUp.y, setting.spawnFwdUp.x);


          // direction
          data.dir = PlayerController.mouseHit
               - NetworkChara.myChara.transform.position
               - new Vector3(0, setting.spawnFwdUp.y, 0); //compensate height, since we fire from hip, not from feet

          //gizmos_dir = data.dir; //debug
          if (setting.maxDownwardsAgnle == 0 && setting.maxUpwardsAgnle == 0)
          {
               data.dir.y = 0;
          }
          else
          {
               float xz = Mathf.Sqrt(data.dir.x * data.dir.x + data.dir.z * data.dir.z);
               float yMin = Mathf.Tan(-setting.maxDownwardsAgnle * Mathf.Deg2Rad) * xz;
               float yMax = Mathf.Tan(setting.maxUpwardsAgnle * Mathf.Deg2Rad) * xz;

               data.dir.y = Mathf.Clamp(data.dir.y, yMin, yMax);
          }

          data.dir = data.dir.normalized;

          //_fire_projectile(data);
          OnFire_ServerRPC(data);
     }

     async void _fire_projectile(Packet data, bool hasWaited = false)
     {
          if (data.delay > 0)
          {
               // cap speed on origin client (others will sync pos)
               if (setting.capSpeedOnDelay >= 0 && data.originClientID == clientID)
               {
                    var controller = FindObjectOfType<PlayerController>();
                    if (controller)
                         controller.CapSpeed(setting.capSpeedOnDelay, data.delay / 1000);
               }

               // wait
               var t = Time.time + data.delay / 1000;
               while (Time.time < t)
                    await Task.Yield();
          }

          // fire
          var p = pool.Get();
          p.enabled = true;
          p.Fire(data.fireFrom, data.dir, data.launcherLayer, data.originClientID);

     }


     // RPC ---------------------------------------------------------------------------------
     [ServerRpc(RequireOwnership = false)]
     void OnFire_ServerRPC(Packet data)
     {
          OnFire_ClientRPC(data);
     }
     [ClientRpc]
     void OnFire_ClientRPC(Packet data)
     {
          //if (clientID != data.originClientID) // everyone except the origin caller
          _fire_projectile(data);
     }
     struct Packet : INetworkSerializable
     {
          public ulong originClientID;
          public Vector3 fireFrom;
          public Vector3 dir;
          public float delay;
          public string launcherLayer;

          //    if a spell has not projectile,
          //    but ONLY targets ground
          //    then use the ground pos, which mask the visual latency of other clients
          //    (but I probably should have a static ground spell class???)
          //public Vector3 targetGroundPos; //TODO: maybe?

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref originClientID);
               serializer.SerializeValue(ref fireFrom);
               serializer.SerializeValue(ref dir);
               serializer.SerializeValue(ref delay);
               serializer.SerializeValue(ref launcherLayer);

               //serializer.SerializeValue(ref targetGroundPos);
          }
     }


     // after hit ---------------------------------------------------------------------------------
     [ClientRpc]
     public void AfterHit_ClientRPC(Vector3 projectilePos, Vector3 targetPos)
     {
          if (!IsServer)
               return;

          //TODO
          //if projectile not destroyed yet, destroy it
     }


     // projectile pool ---------------------------------------------------------------------------------

     ObjectPool<Projectile> pool;
     void InitPool()
     {
          var size = 200;
          var sizeCap = 500;
          pool = new ObjectPool<Projectile>(CreateNew, null, null, null, false, size, sizeCap);

          for (int i = 0; i < setting.preheatPool; i++)
          {
               var obj = pool.Get();
               obj.EndOfUse();
          }
     }

     Projectile CreateNew()
     {
          var p = Instantiate(setting.projectile, transform);
          p.gameObject.SetActive(false);
          p.enabled = false;

          p.launcher = this;

          return p;
     }

     public void Recycle(Projectile p)
     {
          pool.Release(p);
     }


     // onHitEffect pool ---------------------------------------------------------------------------------

     ObjectPool<GameObject> poolVFX;
     void InitPoolVFX()
     {
          var size = 200;
          var sizeCap = 500;
          poolVFX = new ObjectPool<GameObject>(CreateNewVFX, null, null, null, false, size, sizeCap);

          for (int i = 0; i < setting.preheatPool; i++)
          {
               var obj = poolVFX.Get();
               obj.SetActive(false);
          }
     }

     GameObject CreateNewVFX()
     {
          var vfx = Instantiate(setting.onHitVFX, transform);
          vfx.gameObject.SetActive(false);

          return vfx;
     }

     public GameObject GetVFX(Projectile p)
     {
          if (p.launcher == this && setting.onHitVFX != null)
               return poolVFX.Get();

          return null;
     }

     public async void PlanRecycleVFX(GameObject vfx)
     {
          //StartCoroutine(RecycleVFX(vfx));

          if (setting.recycleVFX > 0)
          {
               var t = Time.time + setting.recycleVFX;
               while (Time.time < t)
                    await Task.Yield();
          }

          vfx.SetActive(false);
          poolVFX.Release(vfx);
     }


     // debug ---------------------------------------------------------------------------------

     //Vector3 gizmos_dir;
     //Vector3 gizmos_dirNEW;
     //Vector3 gizmos_from;

     //void OnDrawGizmosSelected()
     //{
     //     if (PlayerChara.me == null)
     //          return;

     //     Gizmos.color = Color.red;
     //     Gizmos.DrawLine(gizmos_from, gizmos_from + gizmos_dir);
     //     Gizmos.color = Color.green;
     //     Gizmos.DrawLine(gizmos_from, gizmos_from + gizmos_dirNEW);
     //}


     //void OnDrawGizmos()
     //{
     //     //     Gizmos.DrawLine(gizmos_from, gizmos_from + gizmos_dir);

     //     Gizmos.


     //}



}

