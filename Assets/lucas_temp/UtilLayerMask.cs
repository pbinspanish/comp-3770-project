using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LayerMaskUtil
{
     // calculate layer mask for you

     // other class will use this too
     // so this is extracted from Projectile.cs


     public static int wall_mask { get => LayerMask.GetMask("Default"); }

     public static int get_target_mask(ProjectileEntry setting, string launcherLayer)
     {
          int mask = 0; // who will be hit?

          if (launcherLayer == "Player")
          {
               if (setting.hitFoe) mask |= LayerMask.GetMask("Enemy");
               if (setting.hitFriend) mask |= LayerMask.GetMask("Player");
          }
          else
          {
               if (setting.hitFoe) mask |= LayerMask.GetMask("Player");
               if (setting.hitFriend) mask |= LayerMask.GetMask("Enemy");
          }


          if (setting.hitSoftTerrain) mask |= LayerMask.GetMask("TerrainWithHP");

          return mask;
     }


     public static int get_all_mask(ProjectileEntry setting, string launcherLayer)
     {
          int mask = 0; // = target_mask + wall

          mask |= get_target_mask(setting, launcherLayer);
          mask |= wall_mask;

          return mask;
     }


}
