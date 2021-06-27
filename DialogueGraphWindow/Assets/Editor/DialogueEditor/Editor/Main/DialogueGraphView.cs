using Frameblade.DialogueSystem.Container.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Frameblade.DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
        private DialogueSearchWindow searchWindow;

        public DialogueGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphGrid"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            GenerateEntryPointNode();
            AddSearchWindow(editorWindow);
        }

        private void AddSearchWindow(EditorWindow editorWindow)
        {
            searchWindow = ScriptableObject.CreateInstance<DialogueSearchWindow>();
            searchWindow.Init(editorWindow, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }

            });

            return compatiblePorts;
        }

        public void GenerateEntryPointNode()
        {
            var node = new DialogueNode
            {
                GUID = Guid.NewGuid().ToString(),
                title = "<< Entry Point >>",
                DialogueText = "Entry",
                EntryPoint = true
            };

            var port = GeneratePort(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);

            node.capabilities &= ~Capabilities.Deletable;
            //node.capabilities &= ~Capabilities.Movable;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            AddElement(node);
        }

        private Port GeneratePort(DialogueNode node, Direction target, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, target, capacity, typeof(float));
        }

        public DialogueNode CreateNode(string nodeName, Vector2 position, string overrideGuid = null)
        {
            var node = CreateDialogueNode(nodeName, position, overrideGuid);
            AddElement(node);
            return node;
        }

        public DialogueNode CreateDialogueNode(string nodeName, Vector2 position, string overrideGuid = null)
        {
            var node = new DialogueNode
            {
                GUID = overrideGuid == null ? Guid.NewGuid().ToString() : overrideGuid,
                title = nodeName,
                DialogueText = nodeName,
                EntryPoint = false
            };

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            var addChoiseButton = new Button(() => { AddChoicePort(node); })
            {
                text = "New Choise"
            };
            node.titleContainer.Add(addChoiseButton);

            var textField = new TextField(string.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                node.DialogueText = evt.newValue;
                node.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(node.title);
            node.mainContainer.Add(textField);

            var addPropButton = new Button(() => AddPropToNode(node))
            {
                text = "Add Property"
            };
            node.mainContainer.Add(addPropButton);
            //node.mainContainer.Add(new Label(" Properties: "));

            node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(position, defaultNodeSize));

            return node;
        }

        public void AddPropToNode(DialogueNode node, string overridenPropName = "New Prop", string overridenPropVal = "Value")
        {
            var prop = new NodeProperty()
            {
                PropertyName = overridenPropName,
                PropertyValue = overridenPropVal
            };

            while (node.Properties.Any(x => x.PropertyName == prop.PropertyName))
            {
                prop.PropertyName = $"{prop.PropertyName}_new";
            }
            node.Properties.Add(prop);

            var propNameField = new TextField("") { value = prop.PropertyName };
            propNameField.RegisterValueChangedCallback(evt =>
            {
                if (node.Properties.Any(x => x.PropertyName == evt.newValue))
                {
                    EditorUtility.DisplayDialog("Warning", "Property name is already using in this node", "OK");
                }

                if (string.IsNullOrEmpty(evt.newValue))
                {
                    EditorUtility.DisplayDialog("Warning", "Don't use empty property name", "OK");
                }

                prop.PropertyName = evt.newValue;
            });

            var propValField = new TextField("Value:") { value = prop.PropertyValue };
            propValField.RegisterValueChangedCallback(evt =>
            {
                prop.PropertyValue = evt.newValue;
            });

            var container = new BlackboardRow(propNameField, propValField);

            var deleteButton = new Button(() => DeleteNodeProperty(node, container, prop)) { text = "X" };
            propNameField.Add(deleteButton);

            node.mainContainer.Add(container);

            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        private void DeleteNodeProperty(DialogueNode node, VisualElement container, NodeProperty prop)
        {
            node.mainContainer.Remove(container);
            node.Properties.Remove(prop);
        }

        public void AddChoicePort(DialogueNode node, string overridenPortName = "", string overridenPortGuid = null, string overridenCondName = "", string overridenCondVal = "")
        {
            var port = GeneratePort(node, Direction.Output);

            overridenPortGuid = overridenPortGuid == null ? Guid.NewGuid().ToString() : overridenPortGuid;

            var oldLabel = port.contentContainer.Q<Label>("type");
            port.contentContainer.Remove(oldLabel);

            var outputPortCount = node.outputContainer.Query("connector").ToList().Count;

            var choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choise {outputPortCount + 1}" : overridenPortName;

            var choiseText = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            choiseText.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
            port.contentContainer.Add(new Label("  "));
            port.contentContainer.Add(choiseText);
            port.contentContainer.Add(new Label(" Choise Text:"));

            var choiseCondition = new ChoiseCondition
            {
                ConditionName = overridenCondName,
                ConditionValue = overridenCondVal
            };

            var conditionVal = new TextField("")
            {
                value = overridenCondVal
            };
            conditionVal.RegisterValueChangedCallback(evt =>
            {
                choiseCondition.ConditionValue = evt.newValue;
            });

            var conditionName = new TextField("")
            {
                value = overridenCondName
            };
            conditionName.RegisterValueChangedCallback(evt =>
            {
                choiseCondition.ConditionName = evt.newValue;
            });

            port.contentContainer.Add(conditionVal);
            port.contentContainer.Add(conditionName);
            port.contentContainer.Add(new Label("Condition, Val:"));

            var portContainer = new DialoguePortContainer
            {
                GUID = overridenPortGuid,
                NodeGuid = node.GUID,
                ChoiseCondition = choiseCondition
            };
            portContainer.WrapPort(port);

            var deleteButton = new Button(() => RemovePort(node, port, portContainer, choiseCondition))
            {
                text = "X"
            };
            port.contentContainer.Add(deleteButton);

            port.portName = choicePortName;

            choiseCondition.ChoiseGuid = portContainer.GUID;

            node.AddPort(portContainer);

            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        private void RemovePort(DialogueNode node, Port port, DialoguePortContainer portContainer, ChoiseCondition condition)
        {
            if (port.connections.Count() != 0)
            {
                var targetEdge = edges.ToList().Where(x => x.output.portName == port.portName && x.output.node == port.node);
                if (!targetEdge.Any()) { return; }

                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }
            
            node.RemovePort(portContainer);

            node.RefreshExpandedState();
            node.RefreshPorts();
        }
    }
}
