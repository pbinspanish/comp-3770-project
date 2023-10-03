using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;


public class UIHpText : MonoBehaviour
{

     Text ui_text;
     HPComponent hpClass;

     void Awake()
     {
          ui_text = GetComponent<Text>();
          hpClass = GetComponentInParent<HPComponent>();

          hpClass.OnHpChange += OnChange;
          hpClass.OnRevive += OnChange;
          hpClass.OnDeath += OnDeath;
     }
     void LateUpdate()
     {
          FaceCamera();
     }

     void FaceCamera()
     {
          transform.LookAt(transform.position + Camera.main.transform.forward);
     }
     void OnChange()
     {
          ui_text.text = hpClass.HP + "/" + hpClass.maxHP;
     }
     void OnDeath()
     {
          ui_text.text = "DEAD";
     }


}
