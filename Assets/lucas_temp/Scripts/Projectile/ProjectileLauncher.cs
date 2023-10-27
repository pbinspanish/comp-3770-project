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
     public string debug;
     public ProjectileEntry setting;


     //private
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     float tNextFire;


     void Awake()
     {
          InitPool();
          InitPoolVFX();
     }
     void Update()
     {
          debug = "Pool: " + pool.CountActive + " Active | " + pool.CountAll + " Count";

          if (NetworkChara.myChara != null)
          {
               HandleInput();
          }
     }

     void HandleInput()
     {
          if (Time.time < tNextFire)
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

          var playerID = NetworkChara.myChara.GetComponent<HPComponent>().id;
          FireProjectile(CharaTeam.player_main_chara, playerID); //input is only from player
     }


     // fire projectile ---------------------------------------------------------------

     public void FireProjectile(CharaTeam team, int attackerID = 0)
     {
          tNextFire = Time.time + setting.cooldown / 1000f;

          var data = new ProjectilePacket();
          data.originClientID = clientID;
          data.attackerID = attackerID; // for AI to identify player, 0 = not in use
          data.delay = setting.delay;
          data.team = team;

          // position
          data.start = NetworkChara.myChara.transform.position;
          data.start += NetworkChara.myChara.transform.rotation * new Vector3(0, setting.spawnFwdUp.y, setting.spawnFwdUp.x);


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

          _fire(data);
          Fire_ServerRPC(data);
     }

     [ServerRpc(RequireOwnership = false)]
     void Fire_ServerRPC(ProjectilePacket data)
     {
          Fire_ClientRPC(data);
     }
     [ClientRpc]
     void Fire_ClientRPC(ProjectilePacket data)
     {
          if (clientID != data.originClientID)
               _fire(data);
     }

     async void _fire(ProjectilePacket data)
     {
          if (data.delay > 0)
          {
               int appliedDelay = data.delay;

               if (data.originClientID == clientID) //origin client
               {
                    // cap speed
                    if (setting.capSpeedOnDelay >= 0)
                    {
                         var controller = FindObjectOfType<PlayerController>();
                         if (controller)
                              controller.CapSpeed(setting.capSpeedOnDelay, data.delay / 1000);
                    }
               }
               else //everyone else
               {
                    // it took some lantency for us to get this packet, so we can fire a bit earlier
                    int lantency = 100; //ms, guessed
                    if (!NetworkManager.Singleton.IsServer)
                         lantency *= 2; //RTT x2 for other clients

                    appliedDelay = Mathf.Clamp(appliedDelay - lantency, 0, int.MaxValue);
               }

               await Task.Delay(appliedDelay);
          }

          // fire
          var p = pool.Get();
          p.enabled = true;
          //p.Fire(data.fireFrom, data.dir, data.team, data.originClientID);
          p.Fire(data);

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
          var p = Instantiate(setting.prefab, transform);
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
          if (setting.onHitVFX == null)
               return;

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
          await Task.Delay(setting.recycleVFX);

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


public struct ProjectilePacket : INetworkSerializable
{
     public ulong originClientID;
     public CharaTeam team;
     public int attackerID;

     public Vector3 start;
     public Vector3 dir;
     public int delay;


     void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
     {
          serializer.SerializeValue(ref originClientID);
          serializer.SerializeValue(ref attackerID);
          serializer.SerializeValue(ref start);
          serializer.SerializeValue(ref dir);
          serializer.SerializeValue(ref delay);
          serializer.SerializeValue(ref team);
     }
}
