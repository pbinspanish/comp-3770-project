using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TEST_async : MonoBehaviour
{

     void OnGUI()
     {
          if (GUILayout.Button("Start 1"))
          {
               BunchOfCoroutine();
          }
          if (GUILayout.Button("Start 2"))
          {
               Debug.Log("F1");

               StartCoroutine(BunchOfCoroutine());
               Debug.Log("F2");
          }
          if (GUILayout.Button("Start 3"))
          {

          }
     }

     IEnumerator BunchOfCoroutine()
     {
          Debug.Log("Start");
          yield return StartCoroutine(Something1());
          yield return StartCoroutine(Something2());
          yield return StartCoroutine(Something3());
     }

     IEnumerator Something1()
     {
          yield return new WaitForSeconds(1);
          Debug.Log("Something1");
     }
     IEnumerator Something2()
     {
          yield return new WaitForSeconds(1);
          Debug.Log("Something2");
     }
     IEnumerator Something3()
     {
          yield return new WaitForSeconds(1);
          Debug.Log("Something3");
     }






}
