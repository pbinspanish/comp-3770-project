using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;



public class TEST : NetworkBehaviour
{

     public static TEST inst;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }

     void Awake()
     {
          inst = this;
          _inputIP = GetLocalIPv4();// or localhost 127.0.0.1
     }
     void Start()
     {
          TEST_SubscribeNetworkEvent();

          StartCoroutine(UpdateIP());
          StartCoroutine(UpdateFPS());
     }

     IEnumerator UpdateFPS()
     {
          fps = 1f / Time.deltaTime;
          yield return new WaitForSeconds(2);
     }

     // set up connection ----------------------------------------------------------------------------

     UnityTransport uTransport { get => NetworkManager.Singleton.GetComponent<UnityTransport>(); }
     string serverIP { get => NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address; }
     readonly static ushort port = 1313;
     readonly static string listenToAll = "0.0.0.0";


     public void SetupConnection(string serverIPv4)
     {
          uTransport.SetConnectionData(serverIPv4, port, listenToAll);
     }
     public void StartHost()
     {
          SetupConnection(myIP); //for init other values, we don't use the ip part
          NetworkManager.Singleton.StartHost();
     }
     public void StartClient(string serverIPv4)
     {
          SetupConnection(serverIPv4);
          NetworkManager.Singleton.StartClient();
     }


     // ip ----------------------------------------------------------------------------

     public static string myIP { get; private set; } //local IPv4

     IEnumerator UpdateIP()
     {
          while (true)
          {
               myIP = GetLocalIPv4();
               yield return new WaitForSeconds(1);
          }
     }

     string GetLocalIPv4()
     {
          var host = Dns.GetHostEntry(Dns.GetHostName());
          foreach (var ip in host.AddressList)
          {
               if (ip.AddressFamily == AddressFamily.InterNetwork)
               {
                    return ip.ToString();
               }
          }

          throw new Exception("Maybe no IPv4 adapters available??");
     }


     // Server: on connect / disconnect ----------------------------------------------------------------------------
     void TEST_SubscribeNetworkEvent()
     {
          // useful
          //NetworkManager.Singleton.OnClientConnectedCallback += (client) => Debug.Log("OnClientConnectedCallback() " + client);
          //NetworkManager.Singleton.OnClientDisconnectCallback += (client) => Debug.Log("OnClientDisconnectCallback() " + client);

          // maybe useful?
          //NetworkManager.Singleton.OnTransportFailure += () => Debug.Log("OnTransportFailure() ");

          // called first, but use callback seems a better idea
          //NetworkManager.Singleton.OnServerStarted += () => Debug.Log("OnServerStarted() ");
          //NetworkManager.Singleton.OnServerStopped += (flag) => Debug.Log("OnServerStopped() " + flag);
          //NetworkManager.Singleton.OnClientStarted += () => Debug.Log("OnClientStarted() ");
          //NetworkManager.Singleton.OnClientStopped += (flag) => Debug.Log("OnClientStopped() " + flag);
     }



     // spawn gameobject ----------------------------------------------------------------------------

     void BossSummonMinion()
     {
          //W
     }


     // pick up gold coin
     void PickUpCoin() // put latency to test
     {
          //W
     }





     // OnGUI ----------------------------------------------------------------------------

     string _inputIP;
     bool _visible = true;
     public float fps;

     void OnGUI()
     {
          // toggle GUI
          if (GUILayout.Button(fps + "", GUILayout.Height(40), GUILayout.Width(40)))
               _visible = !_visible;
          if (!_visible) return;
          GUILayout.BeginArea(new Rect(16, 16, 400, 5000));//-------------------------------------------------------


          // start host / client
          if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
          {
               if (GUILayout.Button("(A) Host  -  IP: " + myIP))
                    StartHost(); // <-

               GUILayout.BeginHorizontal();
               {
                    if (GUILayout.Button("(B) Conect to", GUILayout.Width(200)))
                         StartClient(_inputIP); // <-

                    _inputIP = GUILayout.TextField(_inputIP);
               }
               GUILayout.EndHorizontal();
          }
          else
          {
               if (NetworkManager.Singleton.IsHost)
                    GUILayout.Box("You are Host. IP: " + myIP);
               else
                    GUILayout.Box("You are Cinet. Connect to: " + serverIP);

               if (GUILayout.Button("Disconnect"))
                    NetworkManager.Singleton.Shutdown();
          }


          // GUI log
          GUILayout.Space(10);
          GUILayout.TextArea(log, GUILayout.Height(420));
          if (GUILayout.Button("Clear log"))
          {
               log = "";
               queue.Clear();
          }


          // Other TEST
          GUILayout.Space(10);





          GUILayout.EndArea();//-------------------------------------------------------
     }



     static string time { get => DateTime.Now.ToString("h:mm:ss"); }
     static string log = "";
     static Queue<string> queue = new Queue<string>();
     static int logMaxLines = 30;

     public static void GUILog(string text)
     {
          queue.Enqueue("[" + time + "] " + text);
          if (queue.Count > logMaxLines)
               queue.Dequeue();

          var _array = queue.ToArray();
          log = _array[0];

          for (int i = 1; i < _array.Length; i++)
               log += "\n" + _array[i];
     }




}