using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{

     public static PlayerStatus inst;

     [Header("Customize")]
     public Color color = Color.red;
     public bool clickToRandColor = false;


     [Header("Move")]
     public float acc = 80;
     public float speedCap = 10;
     public float speedCapRun = 25;
     public float speedCapAirborne = 5; //max
     public float rotateSpeed = 800;
     public float jumpForce = 100;


     [Header("Physics")]
     public float gravity = 9.8f;
     public float G = 2; //times gravity


     void Awake()
     {
          if (inst != null)
          {
               Debug.LogError("");
               return;
          }

          inst = this;
     }

     Color RandomColor()
     {
          return new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
     }

     void OnValidate()
     {
          if (clickToRandColor)
          {
               clickToRandColor = false;
               color = RandomColor();
          }

          if (Application.isPlaying && PlayerChara.mine != null)
          {
               PlayerChara.mine.ChangeColor_ServerRpc(color);
          }
     }

}
