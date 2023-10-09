using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpMaster : MonoBehaviour
{
     [Header("Mode")]
     public bool defaultMode = true;
     public bool newSpeedCapMode;
     public bool RTT_LerpMode;
     public bool LerpMode;
     public float extrapolate = 2f; //1 is same

     [Header("Default Mode")]
     public float smoothTime = 0.1f;
     public float clientSmoothFlatMove = 1;
     public float clientMaxDeviation = 10f;

     [Header("New SpeedCap Mode")]
     public float speedCapFactor = 1;

     [Header("RTT+LerpMode Mode")]
     public float lerpFactor_RTT = 1f;

     [Header("Unclamped Lerp Mode")]
     public float lerpFactor = 2f;








     public static LerpMaster inst;
     void Awake()
     {
          inst = this;
     }








}
