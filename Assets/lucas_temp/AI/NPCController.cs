using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPCController : MonoBehaviour
{

     public float moveSpeed = 10f;
     public float rotateSpeed = 10f;



     public void TEST_MoveTowards(NetworkChara target, float speedPct = 1f)
     {
          //position
          transform.position = Vector3.MoveTowards(
               transform.position,
               target.transform.position,
               moveSpeed * speedPct * Time.deltaTime);
     }

     public void TEST_RotateTowards(NetworkChara target)
     {
          //rotation
          var dir = target.transform.position - transform.position;
          var targetRot = Quaternion.LookRotation(dir);
          transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
     }




}
