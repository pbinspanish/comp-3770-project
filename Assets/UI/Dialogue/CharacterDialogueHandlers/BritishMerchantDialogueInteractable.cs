using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public class BritishMerchantDialogueInteractable : DialogueInteractable
{
    public GameObject demon;
    public override void EndDialogue()
    {
        base.EndDialogue();

        Transform pos = transform.parent.transform;
        Destroy(transform.parent.gameObject);
        Instantiate(demon, pos.position, pos.rotation);

        // add code here
    }
}
