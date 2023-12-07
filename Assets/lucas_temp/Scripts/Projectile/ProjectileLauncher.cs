using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Manager class for projectile
/// Create(), Fire()
/// </summary>
public class ProjectileLauncher : MonoBehaviour
{

     //publics
     public KeyCode hotkey = KeyCode.Mouse0; //replace with input system later
     public ProjectileEntry setting;
     //private
     float tNextFire;
     public CharaTeam team;

     public Vector3 start;
     public Vector3 dir;
     void Start(){

     }
     void Awake()
     {
          InitPool();
          InitPoolVFX();
     }
     void Update()
     {
          HandleInput();
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

          FireProjectile_Player();
     }


     // fire projectile ---------------------------------------------------------------

     public void FireProjectile_AI(Vector3 start, Vector3 dir, CharaTeam team)
     {
          FireProjectile(start, dir, team); //this AI should be run by the server, so this clientID is server's ID
     }

     public void FireProjectile_Player()
     {

          // position
          start = gameObject.transform.position;
          // start += gameObject.transform.rotation * new Vector3(0, setting.spawnFwdUp.y, setting.spawnFwdUp.x);

          // direction
          dir = PlayerMove.mouseHit
               - gameObject.transform.position
               - new Vector3(0, setting.spawnFwdUp.y, 0); //compensate height, since we fire from hip, not from feet

          FireProjectile(start, dir, CharaTeam.player_main_chara);
     }

     public void FireProjectile(Vector3 start, Vector3 dir, CharaTeam team)
     {
          tNextFire = Time.time + setting.cooldown / 1000f;


          if (setting.maxDownwardsAgnle == 0 && setting.maxUpwardsAgnle == 0)
          {
               dir.y = 0;
          }
          else
          {
               float xz = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
               float yMin = Mathf.Tan(-setting.maxDownwardsAgnle * Mathf.Deg2Rad) * xz;
               float yMax = Mathf.Tan(setting.maxUpwardsAgnle * Mathf.Deg2Rad) * xz;

               dir.y = Mathf.Clamp(dir.y, yMin, yMax);
          }

          dir = dir.normalized;
          _fire();
     }


     void _fire()
     {
          // fire
          if (setting.burst == 1)
          {
               var p = pool.Get();
               p.enabled = true;
               p.Fire(team,start,dir);
          }
          else
          {
            if (!setting.rotation)
            {
                for (int i = 0; i < setting.burst; i++)
                {
                    var p = pool.Get();
                    p.enabled = true;
                    dir = Quaternion.Euler(0, 360f / setting.burst * (i + 1), 0) * Vector3.forward;

                    p.Fire(team,start,dir);
                }
            }
            else
            {
                //ProjectilePacket temp;
                Vector3 temp = dir;
                for (int i = 0; i < setting.burst; i++)
                {
                    var p = pool.Get();
                    p.enabled = true;
                    dir = Quaternion.Euler(0, 360f / setting.burst * (i + 1), 0) * temp;

                    p.Fire(team,start,dir);
                }
            }
        }
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

