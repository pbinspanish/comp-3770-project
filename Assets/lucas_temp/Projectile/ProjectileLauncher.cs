using System;
using System.Collections;
using System.Collections.Generic;
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


     public int _InstanceID;
     public ulong _NetworkObjectId;
     public ulong _OwnerClientId;


     //private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     float tFire;



     void OnDrawGizmos()
     {
          if (PlayerChara.me == null)
               return;

          string text = "InstID=" + PlayerChara.me.GetInstanceID();
          text += " | NetObjId=" + PlayerChara.me.NetworkObjectId;
          text += " | OwnerId=" + PlayerChara.me.OwnerClientId;

          Gizmos.color = Color.cyan;
          Handles.Label(transform.position, text);

          Gizmos.color = Color.cyan;
          Handles.Label(transform.position + new Vector3(0, 3.5f, 0), "TEST");
     }



     void Awake()
     {
          InitPool();
          InitPoolVFX();
          setting.InitLayerMask();
     }
     void Update()
     {
          if (PlayerChara.me)
          {
               //test
               _InstanceID = PlayerChara.me.GetInstanceID();
               _NetworkObjectId = PlayerChara.me.NetworkObjectId;
               _OwnerClientId = PlayerChara.me.OwnerClientId;

          }



          monitor = "Pool: " + pool.CountActive + " Active | " + pool.CountAll + " Count";

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
               case TriggerMode.FullyAuto:
                    input = Input.GetKey(hotkey); break;
          }

          if (!input)
               return;


          Fire();

     }

     // fire projectile ---------------------------------------------------------------
     public void Fire()
     {
          tFire = Time.time + setting.cooldown / 1000;

          var data = new NetPackage();
          data.originClientID = clientID;
          data.delay = setting.delay;

          // position
          data.fireFrom = PlayerChara.me.transform.position;
          data.fireFrom += PlayerChara.me.transform.rotation * new Vector3(0, setting.spawnFwdUp.y, setting.spawnFwdUp.x);


          // direction
          data.dir = PlayerController.mouseHit
               - PlayerChara.me.transform.position
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

          //gizmos_dirNEW = data.dir; //debug
          //gizmos_from = data.fireFrom; //debug
          data.dir = data.dir.normalized;

          _fire(data);
          OnFire_ServerRPC(data);
     }

     void _fire(NetPackage data, bool hasWaited = false)
     {
          if (data.delay > 0 && !hasWaited)
          {
               StartCoroutine(_fireDelayed(data));
               return;
          }

          var p = pool.Get();
          p.enabled = true;
          //p.Fire(PlayerChara.me.transform.position, data.dir, data.originClientID);
          p.Fire(data.fireFrom, data.dir, data.originClientID);

     }

     IEnumerator _fireDelayed(NetPackage data)
     {
          yield return new WaitForSeconds(data.delay / 1000);
          _fire(data, true);
     }



     // network RPC  ---------------------------------------------------------------------------------
     [ServerRpc(RequireOwnership = false)]
     void OnFire_ServerRPC(NetPackage data)
     {
          OnFire_ClientRPC(data);
     }
     [ClientRpc]
     void OnFire_ClientRPC(NetPackage data)
     {
          if (clientID != data.originClientID) // everyone except the origin caller
               _fire(data);
     }
     struct NetPackage : INetworkSerializable
     {
          public ulong originClientID;
          public Vector3 fireFrom; //no longer in use
          public float delay;
          public Vector3 dir;


          //    if a spell has not projectile,
          //    but ONLY targets ground
          //    then use the ground pos, which mask the visual latency of other clients
          //    (but I probably should have a static ground spell class???)
          public Vector3 targetGroundPos; //TODO: maybe?

          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref originClientID);
               serializer.SerializeValue(ref fireFrom);
               serializer.SerializeValue(ref dir);
               serializer.SerializeValue(ref delay);
               serializer.SerializeValue(ref targetGroundPos);
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

     public void PlanRecycleVFX(GameObject vfx)
     {
          StartCoroutine(RecycleVFX(vfx));
     }

     IEnumerator RecycleVFX(GameObject vfx)
     {
          yield return new WaitForSeconds(setting.recycleVFX);

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

