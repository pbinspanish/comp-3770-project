using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChara_GhostCollider : MonoBehaviour
{

     //send Trigger events to parent

     public Action<Collider> OnTriggerEnter_;
     public Action<Collider> OnTriggerExit_;
     public Action<Collider> OnTriggerStay_;


     void OnTriggerEnter(Collider other) { OnTriggerEnter_?.Invoke(other); }
     void OnTriggerExit(Collider other) { OnTriggerEnter_?.Invoke(other); }
     void OnTriggerStay(Collider other) { OnTriggerEnter_?.Invoke(other); }

}
