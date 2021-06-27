using Frameblade.DialogueSystem.Container.Internal;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Frameblade.DialogueSystem.Editor
{
    public class DialogueNode : Node
    {
        public string GUID;
        public string DialogueText;
        public bool EntryPoint = false;
        public List<NodeProperty> Properties = new List<NodeProperty>();
        public List<DialoguePortContainer> Ports = new List<DialoguePortContainer>();

        public void AddPort(DialoguePortContainer port)
        {
            Ports.Add(port);
            this.outputContainer.Add(port);
        }

        public void RemovePort(DialoguePortContainer port)
        {
            Ports.Remove(port);
            this.outputContainer.Remove(port);
        }
    }
}