using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerSetting : MonoBehaviour
{

     public static PlayerSetting inst { get { if (_inst == null) _inst = FindObjectOfType<PlayerSetting>(); return _inst; } }
     static PlayerSetting _inst;


     public float acc = 80;
     public float accAirborne = 20;
     public float maxValocity = 8.8f;
     public float maxValocityRun = 13.2f;
     public float rotateLerp = 17.5f;
     public float jumpVelocity = 10f;
     public int jumpCount = 2; //2 = double jump


}
