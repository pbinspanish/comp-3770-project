using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharaStatus : MonoBehaviour
{
     public static CharaStatus singleton { get { if (_sin == null) _sin = FindObjectOfType<CharaStatus>(); return _sin; } }
     static CharaStatus _sin;

     [Header("Move")]
     public float acc = 80;
     public float speedCap = 10;
     public float speedCapRun = 25;
     public float speedCapAirborne = 5; //max
     public float rotateSpeed = 800;
     public float jumpForce = 100;


     [Header("just for fun")]
     public Color color = Color.red;









}
