using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Frameblade.DialogueSystem.Container;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private GameObject DialoguePanel;

    [SerializeField] private Text NameText;
    [SerializeField] private Text DialogueText;
    [SerializeField] private Transform AnswersPanel;
    [SerializeField] private AnswerView AnswerButtonPrefab;

    private DialogueContainer currentDialogue;
    private List<GameObject> currentAnswers = new List<GameObject>();

    public void StartDialogue(DialogueContainer container, string companionName)
    {
        currentDialogue = container;
        SetNode(currentDialogue.NodeLinks[0].TargetNodeGuid);        

        NameText.text = companionName; 
        DialoguePanel.SetActive(true);
    }

    public void SetNode(string GUID)
    {
        var node = currentDialogue.Nodes.Find(x => x.GUID == GUID);
        var links = currentDialogue.NodeLinks.Where(x => x.BaseNodeGuid == GUID);
        if(links.Count() == 0) { EndDialogue(); }

        foreach(var prop in node.Properties)
        {
            ValuesRegister.SetValue(prop.PropertyName, int.Parse(prop.PropertyValue));
        }

        foreach(var go in currentAnswers)
        {
            Destroy(go);
        }
        currentAnswers.Clear();

        DialogueText.text = node.DialogueText;

        foreach(var link in links)
        {
            var cond = node.Ports.Find(x => x.Guid == link.PortGuid).ChoiseCondition;
            if(!string.IsNullOrEmpty(cond.ConditionName))
            {
                if (ValuesRegister.Contains(cond.ConditionName))
                {
                    if (int.Parse(cond.ConditionValue) == ValuesRegister.GetValue(cond.ConditionName))
                    {
                        var answer = Instantiate(AnswerButtonPrefab.gameObject, AnswersPanel);
                        var answerText = node.Ports.Find(x => x.Guid == link.PortGuid).ChoiseText;
                        answer.GetComponent<AnswerView>().Initialize(answerText, link.TargetNodeGuid);
                        currentAnswers.Add(answer);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            else
            {
                var answer = Instantiate(AnswerButtonPrefab.gameObject, AnswersPanel);
                var answerText = node.Ports.Find(x => x.Guid == link.PortGuid).ChoiseText;
                answer.GetComponent<AnswerView>().Initialize(answerText, link.TargetNodeGuid);
                currentAnswers.Add(answer);
            }            
        }
    }

    private void EndDialogue()
    {
        currentDialogue = null;
        DialoguePanel.SetActive(false);
    }
}
