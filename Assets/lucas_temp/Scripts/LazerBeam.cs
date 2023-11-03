using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LazerBeam : MonoBehaviour
{

     public bool TEST_MouseLazer;


     LineRenderer lazer;
     ParticleSystem OnHitParticle;

     float maxDist = 500;
     bool inUse;
     Vector3 from;
     Vector3 to;
     int mask;
     Action<Vector3> onHit;


     void Start()
     {
          lazer = GetComponent<LineRenderer>();
          OnHitParticle = GetComponentInChildren<ParticleSystem>(true);
     }

     void Update()
     {
          TEST_ShootWithMouse();

          if (inUse)
          {
               UpdateLazer();
          }
     }

     public void ShootAtPos(Vector3 _from, Vector3 _to, int _mask, Action<Vector3> _onHit)
     {
          inUse = true;

          from = _from;
          to = _to;
          mask = _mask;
          onHit = _onHit;
     }

     public void Stop()
     {
          inUse = false;
          lazer.enabled = false;
          OnHitParticle.gameObject.SetActive(false);
     }

     void UpdateLazer()
     {
          var _dir = to - from;
          var ray = new Ray(from, _dir);
          bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, maxDist, mask);

          //lazer
          lazer.enabled = true;
          lazer.SetPosition(0, from);
          var p2 = hit ? hitInfo.point : _dir.normalized * maxDist;
          lazer.SetPosition(1, p2);


          //particle
          OnHitParticle.gameObject.SetActive(hit);
          if (hit)
          {
               OnHitParticle.transform.position = hitInfo.point;
               if (!OnHitParticle.isPlaying)
                    OnHitParticle.Play();

               onHit?.Invoke(hitInfo.point);
          }

     }


     // TEST ------------------------------------------------

     bool _wasTesting;
     void TEST_ShootWithMouse()
     {
          if (_wasTesting && !TEST_MouseLazer)
          {
               Stop();
          }
          else if (TEST_MouseLazer)
          {
               var from = transform.position;
               var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
               var hit = Physics.Raycast(ray, out RaycastHit hitInfo);

               ShootAtPos(from, hitInfo.point, ~0, null);
          }

          _wasTesting = TEST_MouseLazer;
     }

     void OnDrawGizmos()
     {
          Gizmos.color = Color.green;
          Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
     }


}
