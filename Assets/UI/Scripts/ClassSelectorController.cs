using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles button logic for the class selection screen.
/// </summary>
public class ClassSelectorController : MonoBehaviour
{
    #region Variables

    public GameObject playerPrefab;         // the player prefab to instantiate on game start
    private MainMenuController mainMenu;    // the active main menu

    #endregion

    #region Methods

    /// <summary>
    /// Gets the main menu on start.
    /// </summary>
    private void Start()
    {
        mainMenu = FindObjectOfType<MainMenuController>();
    }

    /// <summary>
    /// Handles each select button being pressed.
    /// </summary>
    public void SelectClassButton_OnPress()
    {
        // set the mesh in the main menu
        mainMenu.playerPrefab = playerPrefab;

        // reset all other button colours
        ClassSelectorController[] controllers = FindObjectsByType<ClassSelectorController>(FindObjectsSortMode.None);

        foreach (ClassSelectorController controller in controllers)
        {
            Button currentButton = controller.GetComponent<Button>();
            ColorBlock colorBlock = currentButton.colors;
            colorBlock.normalColor = Color.black;
            currentButton.colors = colorBlock;
        }

        // highlight this button
        ColorBlock currentColors = GetComponent<Button>().colors;
        currentColors.normalColor = new Color(0, 192, 13);
        GetComponent<Button>().colors = currentColors;
    }

    #endregion
}
