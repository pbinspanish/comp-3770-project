using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Data for Projectile.
/// </summary>
[CreateAssetMenu(menuName = "3770/ProjectileEntry")]
public class ProjectileEntry : ScriptableObject
{

     public Projectile projectile; //prefab
     //public GameObject onHitEffect; //prefab

     [Header(" - TEST")]
     public bool log = false; //debug
     public bool clientHasAutority = false; //TEST


     [Header(" - Basic")]
     public float speed = 60;
     public float range = 100;
     public int damage = 1;
     public int dmgRandomRange = 0; // -this < x < this. Randomly change damage by x
     public int maxHit = 1;


     [Header(" - Launching")]
     public TriggerMode triggerMode = TriggerMode.KeyDown;
     public Vector2 spawnFwdUp = new Vector2(1.5f, 2f);
     public float delay = 0; //ms before launch
     public float cooldown = 20; //ms before launch again


     [Header(" - Force")]
     public Vector2 forceFwdUp = new Vector2(33, 100);
     public ForceDir forceDirection = ForceDir.Foward;
     public bool smoothForce = false;
     public float corpseForceMultiply = 1.5f;


     [Header(" - AoE")]
     public bool hitSameTarget = false; // can we hit more then once? eg a fire wall
     public float hitSameTargetEvery = 1000; //ms


     [Header(" - Other")]
     public float stickToTarget = 30f;
     public float stickToWall = 30f;
     public bool hitEnemy = true;
     public bool hitWall = true;
     public bool hitPlayer = false;
     [Range(0, 89)] public float maxUpwardsAgnle = 30;
     [Range(0, 89)] public float maxDownwardsAgnle = 0; //can we fire up/downwards?
     public int preheatPool = 0; // so lag less when we actually play


     [Header(" - WIP")]
     [Range(0, 100)] public int speedWhenDelay = 100; //% of speed



     //[Header("Despawn")]
     //public ExpireMode despawnMode = ExpireMode.Despawn; //




     // not setting  ---------------------------------------------------------------------------------
     [HideInInspector] public int colMask;
     [HideInInspector] public int targetMask;
     [HideInInspector] public int wallMask;

     public void InitLayerMask()
     {
          targetMask = wallMask = colMask = 0;

          if (hitEnemy) targetMask |= LayerMask.GetMask("Enemy");
          if (hitPlayer) targetMask |= LayerMask.GetMask("Player");
          if (hitWall) wallMask |= LayerMask.GetMask("Default");

          colMask |= targetMask;
          colMask |= wallMask;
     }


}


public enum TriggerMode
{
     KeyDown,
     KeyUp,
     FullAuto,
}

public enum ForceDir
{
     Foward, //eg push / pull target
     RelativeToCenter, //eg push away / suck to center
}

//public enum ExpireMode
//{
//     Despawn,
//     Explode, //only for AOE
//}


