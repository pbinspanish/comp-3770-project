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

     string animName = "UIDamageTextFly";

     TextMeshProUGUI text;
     Animator anim;
     [HideInInspector] public UIDamageTextMgr mgr;
     GameObject target;


     void Awake()
     {
          anim = GetComponent<Animator>();
          text = GetComponentInChildren<TextMeshProUGUI>();
     }

     void LateUpdate()
     {
          if (target == null)
               return;

          var pos = target.transform.position + new Vector3(0f, mgr.offset, 0f);
          transform.position = Camera.main.WorldToScreenPoint(pos);
     }

     public void Display(int number, GameObject _target, Color color, float scale)
     {
          enabled = true;
          gameObject.SetActive(true);

          target = _target;

          text.text = number + "";
          transform.localScale = new Vector3(scale, scale, scale);

          anim.Play(animName);

     }

     public void OnAnimationEnd() //DO NOT change name, called by animator
     {
          mgr.RecycleUI(this);
     }



}
