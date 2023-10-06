using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class TEST_just_a_Mono : NetworkBehaviour
{
     public NetworkBehaviour prefab;
     public bool clickToSpawn = false;
     public bool clickToDeSpawn = false;

     NetworkBehaviour netRef;
     float t;

     void Update()
     {
          TEST_Spawn();
          TEST_DeSpawn();
          t = Time.deltaTime;
     }

     void TEST_Spawn()
     {
          if (clickToSpawn)
          {
               clickToSpawn = false;

               netRef = Instantiate(prefab, transform);
               netRef.GetComponent<NetworkObject>().Spawn();

          }
     }

     void TEST_DeSpawn()
     {
          if (clickToDeSpawn)
          {
               clickToDeSpawn = false;

               if (netRef)
                    netRef.GetComponent<NetworkObject>().Despawn();

               Debug.Log("netRef = " + netRef); //null afterwards?
          }
     }


}
