using System.Collections.Generic;
using UnityEngine;

namespace Frameblade.DialogueSystem.Container.Internal
{
    [System.Serializable]
    public class DialogueNodeModel
    {
        public string GUID;
        public string DialogueText;
        public Vector2 Position;
        public List<ChoisePortModel> Ports = new List<ChoisePortModel>();
        public List<NodeProperty> Properties = new List<NodeProperty>();
    }
}