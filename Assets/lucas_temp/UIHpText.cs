using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;


// TEST class
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
          hpClass.OnDeathBlow += OnDeathBlow;
     }
     void LateUpdate()
     {
          FaceCamera();
     }

     void FaceCamera()
     {
          transform.LookAt(transform.position + Camera.main.transform.forward);
     }
     void OnChange(int delta)
     {
          ui_text.text = hpClass.hp + "/" + hpClass.maxHP;
     }
     void OnDeathBlow(int delta)
     {
          ui_text.text = "DEAD";
     }


}
