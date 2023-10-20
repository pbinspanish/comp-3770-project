using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI;


[CreateAssetMenu(menuName = "3770/UIDamageTextMgr")]
public class UIDamageTextMgr : ScriptableObject
{

     public static UIDamageTextMgr inst { get => GetSingleton(); }

     // public ---------------------------------------------------------------------------------
     public UIDamageText prefab;
     public Color damageColor = Color.white;
     public Color healColor = Color.green;
     public float offset = 4.25f; //above head
     public float dynamicScale = 3;
     public int dynamicScaleAtDamage = 100; // big damage = big text


     public static void Init()
     {
          // call this to decrease (maybe?) lag when first hit enemy
          // TODO: but why?? Resources.Load?
          inst.init();
     }

     public static void DisplayDamageText(int value, Vector3 pos)
     {
          var ui = inst.pool.Get();
          ui.Display(value, pos);
     }
     public static void DisplayDamageText(int value, GameObject obj)
     {
          var ui = inst.pool.Get();
          ui.Display(value, obj);
     }

     public void Example()
     {
          if (Input.GetKeyDown(KeyCode.Mouse0))
          {
               var damage = -100;
               UIDamageTextMgr.DisplayDamageText(damage, new Vector3(0, 2, 0));
          }
          if (Input.GetKeyDown(KeyCode.Mouse1))
          {
               var heal = 100;
               UIDamageTextMgr.DisplayDamageText(heal, new Vector3(0, 2, 0));
          }
     }


     // init --------------------------------------------------------------------------------- 
     static UIDamageTextMgr _inst;
     static UIDamageTextMgr GetSingleton()
     {
          if (_inst == null)
          {
               _inst = Resources.Load(typeof(UIDamageTextMgr).Name) as UIDamageTextMgr;
               _inst.init();
          }
          return _inst;
     }
     void init()
     {
          if (pool == null)
               inst.InitPool();
     }

     Canvas canvas { get => inst.GetOrCreateCanvas(); }
     Canvas _canvas;
     Canvas GetOrCreateCanvas()
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
          pool = new ObjectPool<UIDamageText>(CreateNewUI, null, null, null, false, size, sizeCap);
     }

     UIDamageText CreateNewUI()
     {
          var gameObject = Instantiate(prefab.gameObject, canvas.transform);
          var ui = gameObject.GetComponent<UIDamageText>();

          gameObject.SetActive(false);
          ui.enabled = false;
          ui.mgr = this;

          return ui;
     }

     public void RecycleUI(UIDamageText ui)
     {
          ui.enabled = false;
          ui.gameObject.SetActive(false);

          pool.Release(ui);
     }


}
