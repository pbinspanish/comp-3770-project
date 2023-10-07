using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Display damage text.
/// This is not a UI, but a text mesh in the game world
/// </summary>
public class TMPDamageNumber : MonoBehaviour
{

     public TextMeshPro TMP;
     public Animator anim;

     // private
     string animationName = "TMPDamageNumberFly";
     [HideInInspector] public UIDamageTextMgr mgr;
     GameObject target;


     void LateUpdate()
     {
          transform.LookAt(transform.position + Camera.main.transform.forward);
     }

     public void Display(int number, GameObject target, Vector3 pos, float scale)
     {
          enabled = true;
          gameObject.SetActive(true);
          transform.position = pos;
          TMP.text = number + "";
          TMP.transform.localScale = new Vector3(scale, scale, scale);
          anim.Play(animationName);
     }

     public void OnAnimationEnd() //DO NOT change name, called by animator
     {
          //mgr.Recycle(this); 
     }



}
