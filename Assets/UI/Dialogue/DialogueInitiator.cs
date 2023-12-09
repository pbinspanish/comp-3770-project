using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Objects this component is attached to are able to initiate dialogue.
/// Dialogue can be initated with objects with the DialogueInteractable componenent.
/// </summary>
public class DialogueInitiator : MonoBehaviour
{
    #region Variables

    public bool isInConversation = false;                           // true if the DialogueInitiator is currently having dialogue with a DialogueInteractable
    public bool hasConversationEnded = false;                       // true if there are no remaining lines of dialogue in the active conversation, false otherwise 
    public DialogueInteractable currentDialogueInteractable = null; // the current DialogueInteractable the DialogueInitator is either talking with or able to talk to
                                                                    // if this value is null there is no DialogueInteractable the DialogueInitiator can talk to
    private GameObject interactionIndicator;                        // the parent object for the interaction indicator in the UI
    private TextMeshProUGUI interactionIndicatorNameField;          // the name field for the interaction indicator

    #endregion

    #region Methods

    private void Start()
    {
        interactionIndicator = GameObject.Find("InteractionIndicator");
        interactionIndicatorNameField = GameObject.Find("InteractionIndicatorLabel").GetComponent<TextMeshProUGUI>();

        interactionIndicator.SetActive(false);
    }

    /// <summary>
    /// Gets the DialogueInteractable and shows the interaction indicator
    /// when colliding with a trigger with a DialgueInteractable.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DialogueInteractable>() != null)
        {
            currentDialogueInteractable = other.GetComponent<DialogueInteractable>();

            interactionIndicator.SetActive(true);
            interactionIndicatorNameField.text = currentDialogueInteractable.interactableName;
        }
    }

    /// <summary>
    /// Resets currentDialogueInteractable and
    /// hides the interaction indicator when exiting a trigger.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        currentDialogueInteractable = null;

        interactionIndicator.SetActive(false);
    }

    /// <summary>
    /// Checks for dialogue interaction input and modifies the conversation accordingly.
    /// </summary>
    private void Update()
    {
        // enter or continue the conversation if the player presses the F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            // start, continue, or end the conversation if able
            if (currentDialogueInteractable != null)
            {
                // end the conversation
                if (hasConversationEnded)
                {
                    currentDialogueInteractable.EndDialogue();
                    hasConversationEnded = false;               // reset for the next conversation
                    isInConversation = false;
                }
                else
                {
                    isInConversation = true;
                    // start or continue the conversation
                    if (currentDialogueInteractable.ContinueDialogue())
                    {
                        hasConversationEnded = false;
                    }
                    else
                    {
                        hasConversationEnded = true;
                    }
                }
            }
        }
    }

    #endregion
}
