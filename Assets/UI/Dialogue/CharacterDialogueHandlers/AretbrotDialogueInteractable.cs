using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AretbrotDialogueInteractable : DialogueInteractable
{
    #region Variables

    public GameObject aretbrot;
    
    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        aretbrot = GameObject.FindGameObjectWithTag("Uncle");
    }

    protected override void EndAnimate()
    {
        aretbrot.GetComponent<Animator>().SetBool("DieBitch", true);
        Destroy(aretbrot);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword = true;
        GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueInitiator>().currentDialogueInteractable = null;
        GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueInitiator>().interactionIndicator.SetActive(false);
    }

    #endregion
}
