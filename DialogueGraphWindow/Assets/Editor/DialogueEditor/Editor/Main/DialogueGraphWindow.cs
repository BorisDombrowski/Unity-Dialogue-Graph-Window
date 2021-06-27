using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Frameblade.DialogueSystem.Editor
{
    public class DialogueGraphWindow : EditorWindow
    {
        public Texture2D WindowIcon;
        private DialogueGraphView graphView;
        private string FileName = "";

        [MenuItem("Window/Custom/Dialogue Editor")]
        public static void ShowWindow()
        {
            GetWindow<DialogueGraphWindow>("Dialogue Editor");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            GenerateMinimap();
        }

        private void ConstructGraphView()
        {
            graphView = new DialogueGraphView(this)
            {
                name = "Dialogue Graph"
            };

            graphView.StretchToParentSize();
            this.rootVisualElement.Add(graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("");
            fileNameTextField.SetValueWithoutNotify(FileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => FileName = evt.newValue);
            toolbar.Add(new Label("File Name:"));
            toolbar.Add(fileNameTextField);

            toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save" });
            toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load" });

            var createNodeButton = new Button(() => { graphView.CreateNode("New Node", Vector2.zero); })
            {
                text = "Create Node"
            };
            toolbar.Add(createNodeButton);

            this.rootVisualElement.Add(toolbar);
        }

        private void GenerateMinimap()
        {
            var minimap = new MiniMap { anchored = true };
            minimap.SetPosition(new Rect(10, 30, 200, 140));
            graphView.Add(minimap);
        }

        private void OnDisable()
        {
            this.rootVisualElement.Remove(graphView);
        }

        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                EditorUtility.DisplayDialog("Invalid File Name!", "File Name can't be null or empty.", "OK");
                return;
            }

            var graphSaveUtility = DialogueGraphSaveUtility.GetInatance(graphView);
            if (save)
            {
                graphSaveUtility.Save(FileName);
            }
            else
            {
                graphSaveUtility.Load(FileName);
            }
        }
    }
}
