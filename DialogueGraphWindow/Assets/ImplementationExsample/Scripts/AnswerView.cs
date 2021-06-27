using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerView : MonoBehaviour
{
    [SerializeField] private Text AnswerText;
    private string TargetNodeGUID;

    public void Initialize(string text, string GUID)
    {
        AnswerText.text = text;
        this.TargetNodeGUID = GUID;
    }

    public void Answer()
    {
        FindObjectOfType<DialogueView>().SetNode(TargetNodeGUID);
    }
}
