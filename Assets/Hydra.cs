using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Not the AI. But control movement of head and body
/// </summary>
[ExecuteInEditMode]
public class Hydra : MonoBehaviour

{
     public bool gizmos;
     public List<Transform> list; //in between head & root
     public float max_dist = 10f;
     Transform head { get => list[0]; } //will bite
     Transform root { get => list[list.Count - 1]; } //not moving

     void Awake()
     {
          list = new List<Transform>();

          foreach (Transform child in transform)
               list.Add(child);

          if (list.Count < 3)
          {
               Debug.LogError("must contain at least 3 children : head + body(s) + root");
               enabled = false;
          }
     }

     void OnValidate() //for editor
     {
          list = new List<Transform>();
          foreach (Transform child in transform)
               list.Add(child);
     }

     void Update()
     {
          Constrain_head();
          Update_body();
     }

     void Constrain_head()
     {
          var dist = Vector3.Distance(head.position, root.position);

          if (dist > max_dist)
          {
               var dir = head.position - root.position;
               head.position = root.position + dir.normalized * max_dist;
          }
     }


     public float lerp_strength = 2f;

     void Update_body()
     {
          for (int i = 1; i < list.Count - 1; i++) //skip head & root
          {
               var desiredPos = Vector3.Lerp(head.position, root.position, (float)i / (list.Count - 1));

               var lerp = Mathf.Pow(lerp_strength, (list.Count - i)); //power seems most snake-like

               list[i].position = Vector3.Lerp(list[i].position, desiredPos, lerp * Time.deltaTime);
          }
     }

     void OnDrawGizmos()
     {
          if (!gizmos)
               return;

          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(root.position, max_dist);

          for (int i = 1; i < list.Count; i++)
          {
               Gizmos.DrawLine(list[i].position, list[i - 1].position);
          }
     }







}
