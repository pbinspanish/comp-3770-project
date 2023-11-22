using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEditor.Undo;

[ExecuteInEditMode]
public class TeleportArea : MonoBehaviour
{

     public TeleportArea pair;
     public bool _gizmos = true;

     public bool exit_only; // only allow pair->this

     //private
     [HideInInspector] public GameObject prefab;
     [HideInInspector] public bool destroy_in_progress;
     [HideInInspector] public bool is_clone;
     Dictionary<GameObject, float> recent = new Dictionary<GameObject, float>();
     static float cooldown = 0.5f;
     SphereCollider col;


     void Start()
     {
          col = GetComponent<SphereCollider>();

          Debug.Assert(gameObject.layer == LayerMask.NameToLayer("TeleportArea")); // check error
          undoRedoPerformed += OnUndoRedo;

          Initial();
     }

     void OnTriggerEnter(Collider other)
     {
          if (exit_only)
               return;
          if (recent.ContainsKey(other.gameObject)) // prevent back and forth
               return;

          AIBrain.OnTeleport(other.GetComponent<HPComponent>());

          pair.recent.Add(other.gameObject, Time.time + cooldown);
          other.transform.position = pair.transform.position;

     }

     void OnTriggerExit(Collider other)
     {
          if (!recent.ContainsKey(other.gameObject)) return;
          if (Time.time < recent[other.gameObject]) return;

          recent.Remove(other.gameObject);
     }

     void Initial()
     {
          if (is_clone) return; // only A create B
          if (Application.isPlaying) return; // only when editing
          if (pair != null) return;

          // name thyself
          gameObject.name = "TeleportA " + GetInstanceID();

          // create pair B
          pair = Instantiate(prefab).GetComponent<TeleportArea>();
          pair.pair = this;
          pair.transform.position = transform.position + new Vector3(5, 0, 5);
          pair.is_clone = true;
          pair.gameObject.name = "TeleportB " + GetInstanceID();


          EditorApplication.ExecuteMenuItem("File/Save"); // prevent ctrl+z undo ref

     }


     // editor  ----------------------------------------------------------------------------
     void OnDestroy()
     {
          undoRedoPerformed -= OnUndoRedo;

          //destroy both together
          destroy_in_progress = true;

          if (pair != null && pair.destroy_in_progress == false)
               DestroyImmediate(pair.gameObject);
     }

     void OnUndoRedo()
     {
          if (!pair)
               DestroyImmediate(gameObject); // if pair is undo with ctrl+z, destroy self
     }


     // debug  ----------------------------------------------------------------------------
     void OnDrawGizmos()
     {
          if (!_gizmos)
               return;

          if (is_clone)
               Gizmos.color = Color.yellow;
          else
          {
               Gizmos.color = Color.cyan;
               Gizmos.DrawLine(transform.position, pair.transform.position);
          }

          Gizmos.DrawWireSphere(transform.position, col.radius);
     }

}
