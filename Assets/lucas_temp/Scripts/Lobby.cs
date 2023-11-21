using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour
{
     public static Lobby inst;

     public bool __gizmos;

     [Header("Start Area")]
     public Transform lobby_zone;
     public Transform start_zone;
     public Vector2 zone_size;

     [Header("UI")]
     public GameObject connectUI_root;
     public Button b_host;
     public Button b_connect;
     public TMP_InputField input_ip;
     [Space(10)]
     public GameObject lobbyUI_root;
     public Button b_start_game;
     public Button b_leave_lobby;


     bool skip_lobby;


     void Awake()
     {
          inst = this;
     }

     void Start()
     {
          //UI
          b_host.onClick.AddListener(Host);
          b_connect.onClick.AddListener(Connect);
          b_leave_lobby.onClick.AddListener(Leave_lobby);
          b_start_game.onClick.AddListener(GameStart);

          connectUI_root.SetActive(true);
          lobbyUI_root.SetActive(false);

          input_ip.text = TEST.inst.GetLocalIPv4();
     }

     // spawn  ----------------------------------------------------------

     public Vector3 Get_spawn_pos()
     {
          if (skip_lobby)
               return inst.Get_start_pos();
          else
               return inst.Get_lobby_pos();
     }

     Vector3 Get_lobby_pos()
     {
          var pos = lobby_zone.position;
          pos.x += Random.Range(-zone_size.x, zone_size.x) / 2f;
          pos.z += Random.Range(-zone_size.y, zone_size.y) / 2f;

          return pos;
     }

     Vector3 Get_start_pos()
     {
          var pos = start_zone.position;
          pos.x += Random.Range(-zone_size.x, zone_size.x) / 2f;
          pos.z += Random.Range(-zone_size.y, zone_size.y) / 2f;

          return pos;
     }


     // UI  ----------------------------------------------------------
     void Host()
     {
          TEST.inst.StartHost();

          connectUI_root.SetActive(false);
          lobbyUI_root.SetActive(true);
          b_start_game.gameObject.SetActive(true);
     }

     void Connect()
     {
          TEST.inst.StartClient(input_ip.text);
          //Debug.Log("connect to = " + input_ip.text);

          connectUI_root.SetActive(false);
          lobbyUI_root.SetActive(true);
          b_start_game.gameObject.SetActive(false);
     }

     void Leave_lobby()
     {
          TEST.inst.Shutdown();

          connectUI_root.SetActive(true);
          lobbyUI_root.SetActive(false);
          b_start_game.gameObject.SetActive(false);
     }



     // start  ----------------------------------------------------------

     //[ServerRpc]
     void GameStart()
     {
          if (!TEST.isServer__)
               return;

          GameStart_ClientRPC();
     }
     [ClientRpc]
     void GameStart_ClientRPC()
     {
          skip_lobby = true;

          // close all UI
          connectUI_root.SetActive(false);
          lobbyUI_root.SetActive(false);
          b_start_game.gameObject.SetActive(false);

          // move player to start
          NetworkChara.myChara.transform.position = Get_start_pos();
     }


     // UI  ----------------------------------------------------------
     void OnDrawGizmos()
     {
          if (!__gizmos)
               return;

          var size = new Vector3(zone_size.x, 0.1f, zone_size.y);

          Gizmos.color = Color.cyan;
          Gizmos.DrawWireCube(lobby_zone.position, size);
          Gizmos.DrawLine(lobby_zone.position, start_zone.position);

          Gizmos.color = Color.yellow;
          Gizmos.DrawWireCube(start_zone.position, size);
     }






}
