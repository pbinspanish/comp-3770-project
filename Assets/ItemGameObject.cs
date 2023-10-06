using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Item on the ground, waiting for pick up
[ExecuteInEditMode]
public class ItemGameObject : MonoBehaviour
{

     public string itemName;
     public string description;

     public Text ui_name; //temp
     public Text ui_description; //temp


     // private
     [HideInInspector] public DropManager mgr;
     Collider col;


     void Start()
     {
          col = GetComponent<Collider>();
          col.isTrigger = true;

          gameObject.layer = LayerMask.NameToLayer("Item");
     }
     void OnValidate()
     {
          if (ui_name)
               ui_name.text = itemName;
          if (ui_description)
               ui_name.text = description;
     }

     void OnTriggerEnter(Collider other)
     {
          Debug.Log("OnTriggerEnter " + other.name);

          gameObject.SetActive(false);
     }




}
