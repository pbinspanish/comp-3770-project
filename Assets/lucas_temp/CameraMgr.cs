using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class CameraMgr : MonoBehaviour
{

     public static CameraMgr inst;


     public Camera camMain;
     public Camera camSpectator;


     public Vector3 camOffset = new Vector3(0, 2f, 0);
     public Vector2 lockCamAngle = new Vector2(45f, 45f);
     public Vector2 rotateSpeed = new Vector2(5f, 5f);


     //private
     Transform root { get => camMain.transform.parent; } //we move,rorate root obj, and main camera is a child of this
     float rotX;
     float rotY;
     public float camDist; //TEST
     bool specMode;



     void Awake()
     {
          inst = this;
     }

     void Start()
     {
          SetSpectatorMode(false);

          rotX = root.transform.eulerAngles.x;
          rotY = root.transform.eulerAngles.y;
          camDist = camMain.transform.position.z;
     }

     void Update()
     {
          UpdateMainCam();
     }

     void UpdateMainCam()
     {
          if (PlayerChara.me == null) return;

          // roration
          if (Input.GetMouseButton(1))
          {
               rotX -= Input.GetAxis("Mouse Y") * rotateSpeed.x;
               rotY += Input.GetAxis("Mouse X") * rotateSpeed.y;

               rotX = Mathf.Clamp(rotX, lockCamAngle.x, lockCamAngle.y); //clamp so cam don't go underground

               root.eulerAngles = new Vector3(rotX, rotY, 0);
          }

          // position
          root.position = PlayerChara.me.transform.position + camOffset;

          // dist to root
          camMain.transform.localPosition = new Vector3(0, 0, camDist);
     }

     public void SetSpectatorMode(bool flag)
     {
          specMode = flag;

          camMain.gameObject.SetActive(!flag);
          camSpectator.gameObject.SetActive(flag);
     }

     //public void ToggleSpectatorMode()
     //{
     //     SetSpectatorMode(!specMode);
     //}



}
