//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices.WindowsRuntime;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.Pool;


//public class ProjectileLauncher : NetworkBehaviour
//{

//     [SerializeField] public GameObject prefab;
//     public PlayerController owner { get => PlayerController.owner; }

//     void Awake()
//     {
//          InitPool();
//     }

//     void Update()
//     {
//          if (Input.GetKeyDown(KeyCode.Mouse0))
//          {
//               Fire();
//          }
//     }

//     // pool ------------------------------------------------------------------------
//     ObjectPool<ProjectileAddon> pool2;
//     int poolDefault = 10;
//     int poolMax = 500; // start big!! resize is expensive

//     void InitPool()
//     {
//          pool2 = new ObjectPool<ProjectileAddon>(CreatePoolItem, null, null, null, false, poolDefault, poolMax);
//     }

//     ProjectileAddon CreatePoolItem()
//     {
//          var gameobj = Instantiate(prefab, transform);
//          gameobj.SetActive(false);

//          var script = gameobj.GetComponent<ProjectileAddon>();
//          script.pool = pool2;

//          return script;
//     }


//     // fire ------------------------------------------------------------------------
//     public void Fire()
//     {
//          if (PlayerController.owner == null)
//               return;

//          var pos = PlayerController.owner.transform.position;
//          pos += new Vector3(0, 1, 0) + 1 * PlayerController.owner.transform.forward.normalized;
//          var dir = PlayerController.owner.transform.forward;

//          //server
//          if (!IsServer && !IsHost)
//               Fire_ServerRpc(pos, dir);

//          //local
//          Fire_Local(pos, dir);
//     }


//     void Fire_Local(Vector3 pos, Vector3 dir)
//     {
//          if (pool2 == null)
//               Debug.Log("123");
//          var p = pool2.Get();
//          p.Fire(pos, dir);
//     }


//     [ServerRpc(RequireOwnership = false)]
//     public void Fire_ServerRpc(Vector3 pos, Vector3 dir)
//     {
//          Fire_Local(pos, dir);
//     }






//}


//public struct TEST_NetData : INetworkSerializable
//{
//     // pass alone complex data through network
//     // can include local

//     public Vector3 pos;
//     public Vector3 dir;

//     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//     {
//          throw new NotImplementedException();
//     }


//     //void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
//     //{
//     //     throw new NotImplementedException();
//     //}

//}

