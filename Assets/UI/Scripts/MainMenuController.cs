using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains definitions for methods called by buttons in the main menu.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    #region Variables

    public GameObject mainMenu;             // main menu object to be destroyed on start
    public GameObject playerPrefab;         // player prefab to spawn on game start
    public Transform playerStartPosition;   // position and angle to spawn the player at

    #endregion

    #region Methods

    /// <summary>
    /// Destroys the menu and spawns the player on game start.
    /// </summary>
    public void StartGameButton_OnPress()
    {
        FindObjectsByType<HUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].gameObject.SetActive(true);
        Instantiate(playerPrefab, playerStartPosition.position, playerStartPosition.rotation);
        Destroy(mainMenu);

    }

    /// <summary>
    /// Quits the application on game exit.
    /// </summary>
    public void ExitGameButton_OnPress()
    {
        Application.Quit();
    }

    #endregion
}
