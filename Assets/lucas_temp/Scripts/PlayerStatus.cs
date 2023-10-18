using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStatus : MonoBehaviour
{

     public static PlayerStatus singleton { get { if (_sin == null) _sin = FindObjectOfType<PlayerStatus>(); return _sin; } }
     static PlayerStatus _sin;


     [Header("Move")]
     public float acc = 80;
     public float accAirborne = 20;
     public float maxValocity = 8.8f;
     public float maxValocityRun = 13.2f;
     public float rotateLerp = 17.5f;
     public float jumpVelocity = 10f;
     public int jumpCount = 2; //2 = double jump


     [Header("just for fun")]
     public Color color = Color.red;









}
