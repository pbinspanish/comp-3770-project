using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;


[CreateAssetMenu(menuName = "3770/UIDamageTextMgr")]
public class UIDamageTextMgr : ScriptableObject
{

     // public ---------------------------------------------------------------------------------
     public UIDamageText prefab;
     public Color damageColor = Color.white;
     public Color healColor = Color.green;
     public float offset = 4.25f; //above head
     public float dynamicScale = 3;
     public int dynamicScaleAtDamage = 100; // big damage = big text


     public static void Init()
     {
          // call this manually decrease (maybe?) the lag when you first hit some enemy
          // TODO: but what cause the lag?? Resources.Load?
          inst.init();
     }

     public static void DisplayDamageText(int value, int netObjectID)
     {
          inst.DisplayText(value, netObjectID);
     }


     // private ---------------------------------------------------------------------------------
     void DisplayText(int value, int netObjectID)
     {
          var ui = pool.Get();

          var target = FindNetworkObject(netObjectID);
          var scale = Mathf.LerpUnclamped(1f, dynamicScale, Mathf.Abs(value) / (float)dynamicScaleAtDamage); //big damage = big text
          var color = value < 0 ? damageColor : healColor;

          ui.Display(Mathf.Abs(value), target, color, scale);
     }
     GameObject FindNetworkObject(int netObjectID)
     {
          foreach (var netObj in FindObjectsOfType<NetObjectID>())
          {
               if (netObj.ID == netObjectID)
               {
                    return netObj.gameObject;
               }
          }

          return null;
     }


     // init ---------------------------------------------------------------------------------
     static UIDamageTextMgr inst { get => GetSingleton(); }
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
