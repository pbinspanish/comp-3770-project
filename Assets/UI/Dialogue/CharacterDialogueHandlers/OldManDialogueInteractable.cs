using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManDialogueInteractable : DialogueInteractable
{
    #region Variables

    public DialogueObject[] dialogueObjects;    // 0 is the opening dialogue
                                                // 1 is the opening active dialogue
                                                // 2 is the sword aquired dialogue
                                                // 3 is the flaming sword aquired dialogue
    public int currentDialogueObject = 0;
    public GameObject oldMan;

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        oldMan = GameObject.FindGameObjectWithTag("OldMan");
    }

    protected override void UpdateCurrentDialogue()
    {
        // repeat dialogue after opening
        if (currentDialogueObject == 0)
        {
            currentDialogueObject = 1;
        }

        // sword dialogue
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword)
        {
            currentDialogueObject = 2;
        }

        // update the active dialogue object
        currentDialogue = dialogueObjects[currentDialogueObject];
    }

    protected override void Animate()
    {
        Debug.Log("Dialogue: animating");
        // opening animations
        if (currentDialogueObject == 0 && currentDialogueLocation == 4)
        {
            Debug.Log("Dialogue: opening animation");
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().starterDialogue = false;
            GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().SetBool("WakeUp", true);
            GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().SetBool("Sleep", false);
            oldMan.GetComponent<Animator>().SetBool("Talk", true);
        }
        // opening active animations
        else if (currentDialogueObject > 0)
        {
            Debug.Log("Dialogue: opening active animation");
            oldMan.GetComponent<Animator>().SetBool("Talk", true);
        }
    }

    protected override void EndAnimate()
    {
        // reset animations on dialogue end
        if (currentDialogueLocation == currentDialogue.dialogueText.Length)
        {
            oldMan.GetComponent<Animator>().SetBool("Talk", false);
        }
    }

    #endregion
}
