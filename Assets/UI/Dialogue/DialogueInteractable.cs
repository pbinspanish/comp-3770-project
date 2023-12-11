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
    public DialogueObject secondDialogue;       //optional
    private int currentDialogueLocation = 0;    // the line of dialogue in the current DialogueObject

    private HUDController activeHUD;

    GameObject oldMan;

    #endregion

    #region Methods

    /// <summary>
    /// Get the current HUDController for the scene on start.
    /// Note that there should only be one HUDController at any given time.
    /// </summary>
    private void Start()
    {
        activeHUD = FindObjectsByType<HUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
    }

    /// <summary>
    /// Starts or continues the conversation.
    /// </summary>
    /// <returns>True if there is dialogue remaining, false if the displayed line is the last in the conversation.</returns>
    public bool ContinueDialogue()
    {
        if (interactableName == "Old Man")
        {
            oldMan = GameObject.FindGameObjectWithTag("OldMan");
            bool active = oldMan.GetComponentInChildren<OldMan>().active;
            DialogueObject curDialogue = active ? secondDialogue : currentDialogue;

            activeHUD.ShowDialogue(curDialogue.dialogueText[currentDialogueLocation], curDialogue.dialogueAudio[currentDialogueLocation], interactableName);

            currentDialogueLocation++;

            if (!active && currentDialogueLocation == 4)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                player.GetComponent<PlayerMove>().starterDialogue = false;
                player.GetComponent<Animator>().SetBool("WakeUp", true);
                player.GetComponent<Animator>().SetBool("Sleep", false);
                oldMan.GetComponent<Animator>().SetBool("Talk", true);
            }
            if (active)
            {
                oldMan.GetComponent<Animator>().SetBool("Talk", true);
            }

            if (curDialogue.dialogueText.Length == currentDialogueLocation)
            {
                
                if (!active)
                { oldMan.GetComponentInChildren<OldMan>().active = true; }
                // no more remaining lines
                return false;
            }
            else
            {
                return true;
            }
        }
        if (interactableName == "Aretbrot")
        {
            GameObject uncle = GameObject.FindGameObjectWithTag("Uncle");

            activeHUD.ShowDialogue(currentDialogue.dialogueText[currentDialogueLocation], currentDialogue.dialogueAudio[currentDialogueLocation], interactableName);

            currentDialogueLocation++;

            if (currentDialogue.dialogueText.Length == currentDialogueLocation)
            {
                // no more remaining lines
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {

            activeHUD.ShowDialogue(currentDialogue.dialogueText[currentDialogueLocation], currentDialogue.dialogueAudio[currentDialogueLocation], interactableName);

            currentDialogueLocation++;

            if (currentDialogue.dialogueText.Length == currentDialogueLocation)
            {
                // no more remaining lines
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Hides the dialogue box and resets the location.
    /// </summary>
    public void EndDialogue()
    {
        if (interactableName == "Old Man")
        {
            oldMan.GetComponent<Animator>().SetBool("Talk", false);
        }
        if (interactableName == "Aretbrot")
        {
            GameObject.FindGameObjectWithTag("Uncle").GetComponent<Animator>().SetBool("DieBitch", true);
            Destroy(gameObject);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword = true;
            GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueInitiator>().currentDialogueInteractable = null;
            GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueInitiator>().interactionIndicator.SetActive(false);
        }
        activeHUD.HideDialogue();
        currentDialogueLocation = 0;
        UpdateCurrentDialogue();
    }

    /// <summary>
    /// Override this method to control how and when the currentDialogue updates and changes.
    /// </summary>
    public virtual void UpdateCurrentDialogue()
    {
        
    }

    #endregion
}
