using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Objects this component is attached to can be interacted with for dialogue.
/// </summary>
public class DialogueInteractable : MonoBehaviour
{
    #region Variables

    public string interactableName;             // the name of the interactable object
    public DialogueObject currentDialogue;      // the DialogueObject to be used by a DialogueInitiator
    protected int currentDialogueLocation = 0;    // the line of dialogue in the current DialogueObject

    private HUDController activeHUD;

    #endregion

    #region Methods

    /// <summary>
    /// Get the current HUDController for the scene on start.
    /// Note that there should only be one HUDController at any given time.
    /// </summary>
    protected virtual void Start()
    {
        activeHUD = FindObjectsByType<HUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
    }

    /// <summary>
    /// Starts or continues the conversation.
    /// </summary>
    /// <returns>True if there is dialogue remaining, false if the displayed line is the last in the conversation.</returns>
    public bool ContinueDialogue()
    {
        activeHUD.ShowDialogue(currentDialogue.dialogueText[currentDialogueLocation], currentDialogue.dialogueAudio[currentDialogueLocation], interactableName);
        Animate();

        currentDialogueLocation++;

        if (currentDialogue.dialogueText.Length == currentDialogueLocation)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Hides the dialogue box and resets the location.
    /// </summary>
    public virtual void EndDialogue()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().canMove = true;
        activeHUD.HideDialogue();
        EndAnimate();
        currentDialogueLocation = 0;
        UpdateCurrentDialogue();
    }

    /// <summary>
    /// Override this method to control how and when the currentDialogue updates and changes.
    /// </summary>
    protected virtual void UpdateCurrentDialogue()
    {

    }

    /// <summary>
    /// Override this method to control animations tied to dialogue.
    /// </summary>
    protected virtual void Animate()
    {

    }

    protected virtual void EndAnimate()
    {

    }

    #endregion
}
