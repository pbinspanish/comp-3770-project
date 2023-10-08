using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;


[CreateAssetMenu(menuName = "3770/UIDamageTextMgr")]
public class UIDamageTextMgr : ScriptableObject
{

     // public
     public UIDamageText prefab;
     public Color damageColor = Color.white;
     public Color healColor = Color.green;
     public float offset = 4.25f;
     public float maxScale = 3;
     public int damageForMaxScale = 50;


     public static void OnDamage(int damage, GameObject target)
     {
          inst._OnDamage(damage, target);
     }
     void _OnDamage(int damage, GameObject target)
     {
          var ui = pool.Get();
          var color = damage < 0 ? damageColor : healColor;

          // big damage = big text
          int scale = (int)Mathf.Lerp(1, maxScale, damage / (float)damageForMaxScale);

          ui.Display(Mathf.Abs(damage), target, color, scale);
     }


     // initial ---------------------------------------------------------------------------------
     static UIDamageTextMgr inst { get => GetSingleton(); }
     static UIDamageTextMgr _inst;
     static UIDamageTextMgr GetSingleton()
     {
          if (_inst == null)
          {
               _inst = Resources.Load(typeof(UIDamageTextMgr).Name) as UIDamageTextMgr;
               _inst.Init();
          }
          return _inst;
     }

     void Init()
     {
          InitPool();
     }

     static Canvas canvas { get => inst.GetCanvas(); }
     static Canvas _canvas;
     Canvas GetCanvas()
     {
          if (_canvas != null)
               return _canvas;


          // or find a ScreenSpaceOverlay canvas
          var all = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

          foreach (var c in all)
               if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return _canvas = c;

          // or create one
          var obj = new GameObject();
          var newCanvas = obj.AddComponent<Canvas>();
          newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
          return _canvas = newCanvas;

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
