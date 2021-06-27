using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Frameblade.DialogueSystem.Editor
{
    public class DialogueSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView graphView;
        private EditorWindow window;
        private Texture2D icon;

        public void Init(EditorWindow window, DialogueGraphView graphView)
        {
            this.graphView = graphView;
            this.window = window;

            icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0), 
            //new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Node", icon))
            {
                userData = new DialogueNode(),
                level = 1
            }
        };
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePos = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent,
                                                         context.screenMousePosition - window.position.position);
            var localMousePos = graphView.contentViewContainer.WorldToLocal(worldMousePos);

            switch (SearchTreeEntry.userData)
            {
                case DialogueNode dialogueNode:
                    graphView.CreateNode("New Node", localMousePos);
                    return true;

                default:
                    return false;
            }
        }
    }
}
