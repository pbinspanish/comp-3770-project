using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NotUnique : MonoBehaviour
{


     void Awake()
     {
          var mgr = GetComponent<NetworkManager>();

          if (NetworkManager.Singleton != null && NetworkManager.Singleton != mgr)
          {
               Debug.Log("NotUnique: Destroying duplicate NetworkManager");
               Destroy(gameObject);
          }


     }



}
