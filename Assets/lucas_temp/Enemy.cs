using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


// TEST class
public class Enemy : NetworkBehaviour
{
     HPComponent hpClass;

     void Awake()
     {
          hpClass = GetComponentInParent<HPComponent>();

          hpClass.OnDeathBlow += OnDeath;
     }

     void OnDeath()
     {
          var rb = GetComponent<Rigidbody>();

          if (rb)
          {
               rb.constraints = 0; //80 = freezeXZ, 0 = no contraint
               rb.mass /= 2f;
          }
     }



}
