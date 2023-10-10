using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LazerBeam : MonoBehaviour
{

     public bool test;


     float maxDist = 500;
     LineRenderer lazer;
     bool inUse;
     int targetLayerMask;
     Vector3 from;
     Vector3 dir;
     ParticleSystem OnHitParticle;

     void Start()
     {
          lazer = GetComponent<LineRenderer>();
          OnHitParticle = GetComponentInChildren<ParticleSystem>(true);
          targetLayerMask = LayerMask.GetMask("Player");
     }
     void Update()
     {
          if (inUse)
               UpdateLazer();

          if (test)
               TEST_ShootWithMouse();
     }

     public void Shoot(Vector3 _from, Vector3 _dir)
     {
          inUse = true;
          from = _from;
          dir = _dir;
     }

     public void Draw(Vector3 p0, Vector3 p1, bool particle) //NOTE: this can go through objects
     {
          lazer.SetPosition(0, p0);
          lazer.SetPosition(1, p1);
          lazer.enabled = true;

          OnHitParticle.gameObject.SetActive(particle);

          if (particle)
          {
               OnHitParticle.transform.position = p1;
               if (!OnHitParticle.isPlaying)
                    OnHitParticle.Play();
          }
     }

     public void Stop()
     {
          inUse = false;
          lazer.enabled = false;
          OnHitParticle.gameObject.SetActive(false);

          //lazer.SetPosition(0, new Vector3(99999, -99999, 99999));
          //lazer.SetPosition(1, new Vector3(99999, -99999, 99999));
     }



     // private ------------------------------------------------

     void UpdateLazer()
     {
          var ray = new Ray(from, dir);
          bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, maxDist, targetLayerMask);

          if (hit)
          {
               Draw(from, hitInfo.point, true);
          }
          else
          {
               Draw(from, from + dir.normalized * maxDist, false);
          }
     }

     void TEST_ShootWithMouse()
     {
          var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
          var hit = Physics.Raycast(ray, out RaycastHit hitInfo);

          if (hit)
               Draw(transform.position, hitInfo.point, true);
          else
               Stop();

          //var p0 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
          //Shoot(transform.position, p0 - Camera.main.transform.position);
     }

     void OnDrawGizmos()
     {
          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position, 2);
     }


}
