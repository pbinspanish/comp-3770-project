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


     [Header(" - TEST")]
     public bool clientHasAutority = false; //TEST


     [Header(" - Basic")]
     public float speed = 70;
     public float range = 100;
     public int damage = 1;
     public int dmgRandomRange = 0; // -this < x < this. Randomly change damage by x
     public int maxHit = 1;


     [Header(" - Launching")]
     public TriggerMode triggerMode = TriggerMode.KeyDown;
     public float delay = 0; //ms before launch
     //[Range(0, 100)] public int speedWhenDelay = 100; //% of speed
     public float cooldown = 20; //ms before launch again


     [Header(" - Force")]
     public Vector2 forceFwdUp = new Vector2(100, 33);
     public ForceDir forceDirection = ForceDir.Foward;
     public bool smoothForce = false;
     public float corpseForceMultiply = 1.5f;


     [Header(" - AoE")]
     public bool hitSameTarget = false; // can we hit any same targets repeatedly? eg. a fire wall
     public float hitSameTargetEvery = 1000; //ms
     //public ExpireMode despawnMode = ExpireMode.Explode;


     [Header(" - Visual")]
     public Vector2 spawnFwdUp = new Vector2(1.5f, 2f);
     public GameObject onHitVFX; //prefab
     public float recycleVFX = 2; //TODO: right now the strategy is auto recycle after X second 
     public float stickToTarget = 30f;
     public float stickToWall = 30f;


     [Header(" - Other")]
     public bool hitEnemy = true;
     public bool hitWall = true;
     public bool hitPlayer = false;
     [Range(0, 89)] public float maxUpwardsAgnle = 30;
     [Range(0, 89)] public float maxDownwardsAgnle = 0; //can we fire up/downwards?
     public int preheatPool = 0; // lag less when we use it. TODO: does this really help?




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
     FullyAuto,
}

public enum ForceDir
{
     Foward, //eg. push or pull target, using projectile's forward direction
     AwayFromCenter, //eg. push away or suck in target, RELATIVE to the projectile center
}

//public enum ExpireMode
//{
//     Despawn,
//     Explode, //for AOE
//}


