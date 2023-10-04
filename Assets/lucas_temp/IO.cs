using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; //to binary
using System.Xml.Serialization; //to xml


public class IO : MonoBehaviour
{

     public static bool log = false;
     public static string savePath = Application.persistentDataPath + "/Save";


     //public
     public static void SaveData<T>(string path, T content)
     {
          _SaveData(path, content);
     }

     public static T LoadData<T>(string path) where T : class
     {
          return _LoadData<T>(path);
     }


     //private
     static void _SaveData<T>(string path, T content)
     {
          if (log) Debug.Log("SaveData. Type = " + typeof(T) + " Path = " + path);

          Directory.CreateDirectory(savePath);

          //file
          Stream steam = File.Open(path, FileMode.OpenOrCreate);

          //save in binary
          BinaryFormatter bf = new BinaryFormatter();

          bf.Serialize(steam, content);
          steam.Close();
     }

     static T _LoadData<T>(string path) where T : class
     {
          if (log) Debug.Log("LoadData. Type = " + typeof(T) + " Path = " + path);

          if (File.Exists(path) == false)
          {
               Debug.Log("path not found: " + path);
               return null;
          }

          try
          {
               Stream steam = File.Open(path, FileMode.Open);

               BinaryFormatter bf = new BinaryFormatter();

               T output = (T)bf.Deserialize(steam);

               steam.Close();

               return output;
          }
          catch
          {
               Debug.Log("load fail: " + path);
               return null;
          }
     }



}
