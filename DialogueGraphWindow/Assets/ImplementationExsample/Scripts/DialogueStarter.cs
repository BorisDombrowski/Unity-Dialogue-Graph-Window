using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frameblade.DialogueSystem.Container;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogueContainer dialogue;
    private bool isDialogueStarted = false;

    private void OnMouseDown()
    {
        if (!isDialogueStarted)
        {
            FindObjectOfType<DialogueView>().StartDialogue(dialogue, "Talking Capsule");
        }
    }
}
