using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    #region Variables

    // Health Variables
    public int minHealth = 0;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public TextMeshProUGUI healthValue;
    public Slider healthSlider;

    // Dialogue Variables
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerText;
    public AudioSource dialogueAudio;
    public GameObject dialoguePanel;        // The gameobject to be shown/hidden that contains the dialogue box
    public float charactersPerSecond = 5;

    // Quest Variables
    public GameObject activeQuests;
    public TextMeshProUGUI activeQuestText;
    public GameObject questBackground;
    public GameObject questBackgroundOutline;

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

    /// <summary>
    /// Shows the dialogue box.
    /// </summary>
    /// <param name="dialogue">The text of the dialogue to display.</param>
    /// <param name="voiceLine">The audio clip to play matching the dialogue.</param>
    /// <param name="speaker">The name of the character speaking the dialogue.</param>
    public void ShowDialogue(string dialogue, AudioClip voiceLine, string speaker)
    {
        speakerText.text = speaker;
        dialogueText.text = dialogue;
        dialogueAudio.clip = voiceLine;

        dialogueAudio.Play();

        dialoguePanel.SetActive(true);
    }

    /// <summary>
    /// Hides the dialogue box.
    /// </summary>
    public void HideDialogue()
    {
        speakerText.text = null;
        dialogueText.text = null;
        dialogueAudio.clip = null;

        dialogueAudio.Stop();

        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Shows the applied text on the quest marker. Passing null will cause the quest indicator to be hidden.
    /// </summary>
    /// <param name="questText">The text to display on the quest indicator. A null value will hide the indicator entirely.</param>
    public void UpdateActiveQuest(string questText)
    {
        if (questText != null)
        {
            activeQuests.SetActive(true);
            activeQuestText.text = questText;
        }
        else
        {
            activeQuests.SetActive(false);
        }
    }

    #endregion
}
