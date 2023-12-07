using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    #region Variables

    public int minHealth = 0;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public TextMeshProUGUI healthValue;
    public Slider healthSlider;

    #endregion

    #region Methods

    /// <summary>
    /// Initialize slider and label values.
    /// </summary>
    void Start()
    {
        healthValue.text = maxHealth.ToString() + "/" + maxHealth.ToString();

        healthSlider.minValue = minHealth;
        healthSlider.maxValue = maxHealth;
    }

    /// <summary>
    /// UI is updated on LateUpdate() to allow for player changes to occur and be captured in the UI.
    /// </summary>
    void LateUpdate()
    {
        healthSlider.value = currentHealth;
        healthValue.text = currentHealth.ToString() + "/" + maxHealth.ToString();
    }

    #endregion
}
