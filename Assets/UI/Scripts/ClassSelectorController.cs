using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectorController : MonoBehaviour
{
    public Mesh classMesh;
    public MainMenuController mainMenu;

    private void Start()
    {
        mainMenu = FindObjectOfType<MainMenuController>();
    }

    public void SelectClassButton_OnPress()
    {
        // set the mesh in the main menu
        mainMenu.playerClassMesh = classMesh;

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
}
