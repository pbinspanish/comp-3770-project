using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GlobalSetting : MonoBehaviour
{
     public static GlobalSetting singleton { get { if (_sin == null) _sin = FindObjectOfType<GlobalSetting>(); return _sin; } }
     static GlobalSetting _sin;


     [Header("Network")]
     public float clientSmooth = 0.4f;
     public float clientSmoothFlat = 1;
     public float clientMaxDeviation = 10f;


     [Header("Physics")]
     public float gravity = 9.8f;


}
