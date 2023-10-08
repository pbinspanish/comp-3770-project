using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// Display damage text.
/// </summary>
public class UIDamageText : MonoBehaviour
{

     string animationName = "UIDamageTextFly";

     TextMeshProUGUI ui;
     Animator anim;
     [HideInInspector] public UIDamageTextMgr mgr;
     GameObject target;


     void Awake()
     {
          anim = GetComponent<Animator>();
          ui = GetComponentInChildren<TextMeshProUGUI>();
     }

     void LateUpdate()
     {
          var pos = target.transform.position + new Vector3(0f, mgr.offset, 0f);
          transform.position = Camera.main.WorldToScreenPoint(pos);
     }

     public void Display(int number, GameObject _target, Color color, float scale)
     {
          enabled = true;
          gameObject.SetActive(true);

          target = _target;

          ui.text = number + "";
          ui.transform.localScale = new Vector3(scale, scale, scale);

          //Debug.Log("scale " + scale);

          anim.Play(animationName);
     }

     public void OnAnimationEnd() //DO NOT change name, called by animator
     {
          mgr.Recycle(this);
     }



}
