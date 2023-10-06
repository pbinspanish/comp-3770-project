using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "DropTable")]
[ExecuteInEditMode]
public class DropTable : ScriptableObject
{
     [SerializeField]
     public DropMode mode = DropMode.EverythingIsPossible;
     public bool clickToTrim; //delete null or duplicated items
     public List<DropEntry> entries;

     [HideInInspector] public bool trim;

     void OnValidate()
     {
          if (clickToTrim)
          {
               clickToTrim = false;
               Trim();
          }
     }

     public void Trim()
     {
          //this structure appears in inspector, who knows what user'd do?

          entries = entries.DistinctBy(e => e.obj).ToList(); //remove duplicate

          for (int i = entries.Count - 1; i >= 0; i--)
               if (entries[i].obj == null)
                    entries.RemoveAt(i); //remove nullD

          entries.TrimExcess(); //trim

          trim = true;
     }

}


[Serializable]
public class DropEntry
{
     public GameObject obj; //prefab

     [Range(0, 100)]
     public int chance; //percentage
}


public enum DropMode
{
     EverythingIsPossible, //roll every item independently, so you may get iron, lemon and diamond all at once!
     OneAmongAll, //will ONLY and ALWAYS drop 1 item. Use weighted probability
}