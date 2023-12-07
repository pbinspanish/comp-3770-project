using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject menuPrefab;
    public GameObject playerPrefab; 
    public Transform playerStart;

    public void StartGameButton_OnPress()
    {
        Instantiate(playerPrefab, playerStart.position, playerStart.rotation);
        
        Destroy(menuPrefab);
    }

    public void ExitGameButton_OnPress()
    {
        Application.Quit();
    }
}
