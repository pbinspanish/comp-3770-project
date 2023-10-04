using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;


[Serializable]
public class ProjectileSetting
{
     //data for Projectile

     public GameObject projectile; //prefab
     //public GameObject onHitEffect; //prefab

     [Header("TEST / Debug")]
     public bool log = false; //debug
     public bool clientHasAutority = false; //TEST


     [Header("Basic")]
     public float speed = 20;
     public int damage = 1;
     public int maxVictim = 1;
     public Vector2 knock = new Vector2(100, 30);
     public float range = 100;


     [Header("Launch")]
     public Vector3 launchOffset = new Vector3(0, 1, 0);
     //public float launchDelay = 200;
     //public float moveWhileLaunch = 0.5f;


     //[Header("AoE")]
     //public int AoEDamage = 1; 
     //public int AoERadius = 10; 
     //public Vector2 AoEKnock = new Vector2(100, 30); 
     //public KnockMode AoEKnockMode = KnockMode.KnockFromAoECenter; 


     [Header("TargetMask")]
     public bool hitEnemy = true;
     public bool hitWall = true;
     public bool hitPlayer = false;


     //[Header("Despawn")]
     //public ExpireMode despawnMode = ExpireMode.Despawn; //


     [Header("Visual")]
     public float stickToTarget = 3f;
     public float stickToWall = 30f;



}


public enum ExpireMode
{
     Despawn,
     Explode, //only for AOE
}

public enum KnockMode
{
     KnockFromCaster,
     KnockFromAoECenter,
}

