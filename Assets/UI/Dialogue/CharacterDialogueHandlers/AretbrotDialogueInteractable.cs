using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AretbrotDialogueInteractable : DialogueInteractable
{
    #region Variables

    public GameObject aretBrot;
    
    #endregion

    #region Methods

    protected override void Start()
    {
        aretBrot = GameObject.FindGameObjectWithTag("Uncle");
    }

    protected override void UpdateCurrentDialogue()
    {
        if (currentDialogueLocation == currentDialogue.dialogueText.Length)
        {
            aretBrot.GetComponent<Animator>().SetBool("DieBitch", true);
            Destroy(gameObject);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword = true;
            GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueInitiator>().currentDialogueInteractable = null;
            GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueInitiator>().interactionIndicator.SetActive(false);
        }
    }

    #endregion
}
