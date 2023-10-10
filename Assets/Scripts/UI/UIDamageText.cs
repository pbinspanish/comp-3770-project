using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.UIElements.UxmlAttributeDescription;


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
     Vector3 targetPos;


     void Awake()
     {
          anim = GetComponent<Animator>();
          text = GetComponentInChildren<TextMeshProUGUI>();
     }

     void LateUpdate()
     {
          if (target != null)
          {
               var pos = target.transform.position + new Vector3(0f, mgr.offset, 0f);
               transform.position = Camera.main.WorldToScreenPoint(pos);
          }
          else
          {
               transform.position = Camera.main.WorldToScreenPoint(targetPos);
          }
     }

     public void Display(int value, GameObject _target)
     {
          target = _target; //follow gameObject

          _display(value);
     }
     public void Display(int value, Vector3 pos)
     {
          targetPos = pos; //text don't move
          target = null;

          _display(value);
     }

     float GetScale(int damageOrHealValue)
     {
          //big damage = big text
          return Mathf.LerpUnclamped(1f, UIDamageTextMgr.inst.dynamicScale, Mathf.Abs(damageOrHealValue) / (float)UIDamageTextMgr.inst.dynamicScaleAtDamage);
     }


     // private ------------------------------------------------------------------
     void _display(int value)
     {
          enabled = true;
          gameObject.SetActive(true);

          text.text = Mathf.Abs(value) + "";
          text.color = value < 0 ? UIDamageTextMgr.inst.damageColor : UIDamageTextMgr.inst.healColor;
          var scale = GetScale(value);
          transform.localScale = new Vector3(scale, scale, scale);

          anim.Play(animName);
     }

     void OnAnimationEnd() //DO NOT change name, called by animator
     {
          enabled = false;
          gameObject.SetActive(false);

          target = null;

          mgr.RecycleUI(this);
     }



}
