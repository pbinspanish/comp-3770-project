using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Spawn and manage damage text
/// Also see UIDamageNumber
/// </summary>
public class UIDamageTextMgr : MonoBehaviour
{

     public static UIDamageTextMgr inst;


     // setting
     public UIDamageText prefab;
     public Canvas canvas;

     public Color damageColor = Color.white;
     public Color healColor = Color.green;
     public float offset = 4.25f;

     public float maxScale = 3;
     public int damageForMaxScale = 50;


     void Awake()
     {
          inst = this;
          InitPool();

          if (canvas == null)
          {
               var all = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
               if (all.Length > 0)
                    canvas = all[0];
               if (all.Length > 1)
                    Debug.Log("If you have more then 1 canvas, consider manually assgin canvas");
          }
     }

     public void OnDamage(int damage, GameObject target)
     {
          var ui = pool.Get();
          var color = damage < 0 ? damageColor : healColor;

          // big damage = big text
          int scale = (int)Mathf.Lerp(1, maxScale, damage / (float)damageForMaxScale);

          ui.Display(Mathf.Abs(damage), target, color, scale);

     }


     // pool ---------------------------------------------------------------------------------

     ObjectPool<UIDamageText> pool;
     void InitPool()
     {
          var size = 200;
          var sizeCap = 500;
          pool = new ObjectPool<UIDamageText>(CreateNew, null, null, null, false, size, sizeCap);
     }

     UIDamageText CreateNew()
     {
          var gameObject = Instantiate(prefab.gameObject, canvas.transform);
          var ui = gameObject.GetComponent<UIDamageText>();

          gameObject.SetActive(false);
          ui.enabled = false;
          ui.mgr = this;

          return ui;
     }

     public void Recycle(UIDamageText ui)
     {
          ui.enabled = false;
          ui.gameObject.SetActive(false);

          pool.Release(ui);
     }





}
