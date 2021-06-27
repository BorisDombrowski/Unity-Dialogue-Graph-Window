using Frameblade.DialogueSystem.Container.Internal;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Frameblade.DialogueSystem.Editor
{
    public class DialoguePortContainer : VisualElement
    {
        public string GUID;
        public string NodeGuid;
        public ChoiseCondition ChoiseCondition = null;
        public Port WrappedPort;

        public void WrapPort(Port port)
        {
            WrappedPort = port;
            this.Add(port);
        }
    }
}
