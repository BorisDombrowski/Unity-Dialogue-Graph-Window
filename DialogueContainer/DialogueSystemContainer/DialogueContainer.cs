using Frameblade.DialogueSystem.Container.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Frameblade.DialogueSystem.Container
{
    [System.Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public List<DialogueEdgeModel> NodeLinks = new List<DialogueEdgeModel>();
        public List<DialogueNodeModel> Nodes = new List<DialogueNodeModel>();
    }
}

