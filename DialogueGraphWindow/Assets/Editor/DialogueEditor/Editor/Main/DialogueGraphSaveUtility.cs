using Frameblade.DialogueSystem.Container;
using Frameblade.DialogueSystem.Container.Internal;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Frameblade.DialogueSystem.Editor
{
    public class DialogueGraphSaveUtility
    {
        private DialogueGraphView targetGraphView;

        public static DialogueGraphSaveUtility GetInatance(DialogueGraphView dialogueGraphView)
        {
            return new DialogueGraphSaveUtility
            {
                targetGraphView = dialogueGraphView
            };
        }

        public void Save(string fileName)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

            if (!SaveNodes(dialogueContainer)) { return; }
            SaveEdges(dialogueContainer);

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogues"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogues");
            }

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        private bool SaveNodes(DialogueContainer container)
        {
            var nodes = targetGraphView.nodes.ToList().Where(x => !((DialogueNode)x).EntryPoint).ToList();
            if (nodes.Count == 0) { return false; }

            foreach (var node in nodes)
            {
                var dNode = (DialogueNode)node;
                var nodeModel = new DialogueNodeModel
                {
                    GUID = dNode.GUID,
                    DialogueText = dNode.DialogueText,
                    Position = dNode.GetPosition().position
                };

                foreach (var prop in dNode.Properties)
                {
                    nodeModel.Properties.Add(prop);
                }

                foreach (var port in dNode.Ports)
                {
                    var portModel = new ChoisePortModel
                    {
                        Guid = port.GUID,
                        NodeGuid = dNode.GUID,
                        ChoiseText = ((Port)port.Children().ToList()[0]).portName,
                        ChoiseCondition = port.ChoiseCondition
                    };
                    nodeModel.Ports.Add(portModel);
                }

                //Debug.Log($"{nodeModel.GUID}\n{nodeModel.DialogueText}\n{nodeModel.Properties.Count}\n{nodeModel.Ports.Count}");
                container.Nodes.Add(nodeModel);
            }
            //Debug.Log(container.Nodes.Count);

            return true;
        }

        private void SaveEdges(DialogueContainer container)
        {
            var edges = targetGraphView.edges.ToList();
            if (edges.Count == 0) { return; }

            foreach (var edge in edges)
            {
                var baseNode = edge.output.node as DialogueNode;
                var targetNode = edge.input.node as DialogueNode;

                if (!baseNode.EntryPoint)
                {
                    var outputPort = baseNode.Ports.Where(x => x.WrappedPort == edge.output).ToList()[0];

                    var edgeModel = new DialogueEdgeModel
                    {
                        BaseNodeGuid = baseNode.GUID,
                        TargetNodeGuid = targetNode.GUID,
                        PortGuid = outputPort.GUID
                    };
                    container.NodeLinks.Add(edgeModel);
                }
                else
                {
                    var edgeModel = new DialogueEdgeModel
                    {
                        BaseNodeGuid = baseNode.GUID,
                        TargetNodeGuid = targetNode.GUID
                    };
                    container.NodeLinks.Add(edgeModel);
                }
            }
        }        

        public void Load(string fileName)
        {
            var container = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");
            if (container == null)
            {
                EditorUtility.DisplayDialog("File Not Found!", "Target dialogue data file is not exists.", "OK");
                return;
            }

            foreach (var node in targetGraphView.nodes.ToList().Cast<DialogueNode>())
            {
                targetGraphView.RemoveElement(node);
            }
            foreach (var edge in targetGraphView.edges.ToList())
            {
                targetGraphView.RemoveElement(edge);
            }
            targetGraphView.GenerateEntryPointNode();

            CreateNodes(container);
            LinkNodes(container);
        }

        private void CreateNodes(DialogueContainer container)
        {
            if (container.NodeLinks.Count > 0)
            {
                targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList().Find(x => x.EntryPoint).GUID
                    = container.NodeLinks[0].BaseNodeGuid;
            }

            foreach (var nodeModel in container.Nodes)
            {
                var node = targetGraphView.CreateNode(nodeModel.DialogueText, nodeModel.Position, nodeModel.GUID);

                foreach (var prop in nodeModel.Properties)
                {
                    targetGraphView.AddPropToNode(node, prop.PropertyName, prop.PropertyValue);
                }

                foreach (var choise in nodeModel.Ports)
                {
                    targetGraphView.AddChoicePort(node, choise.ChoiseText, choise.Guid,
                        choise.ChoiseCondition.ConditionName, choise.ChoiseCondition.ConditionValue);
                }
            }
        }

        private void LinkNodes(DialogueContainer container)
        {
            if (container.NodeLinks.Count == 0) { return; }

            foreach (var edge in container.NodeLinks)
            {
                var baseNode = targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList().Find(x => x.GUID == edge.BaseNodeGuid);
                Port basePort = null;
                if (baseNode.EntryPoint)
                {
                    basePort = (Port)baseNode.outputContainer[0];
                }
                else
                {
                    for (int i = 0; i < baseNode.outputContainer.childCount; i++)
                    {
                        var portContainer = (DialoguePortContainer)baseNode.outputContainer[i];
                        if (portContainer.GUID == edge.PortGuid)
                        {
                            basePort = (Port)portContainer.Children().ToList()[0];
                            break;
                        }
                    }
                }

                var targetNode = targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList().Find(x => x.GUID == edge.TargetNodeGuid);
                Port targetPort = (Port)targetNode.inputContainer[0];

                LinkNodes(basePort, targetPort);
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };

            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);

            targetGraphView.Add(tempEdge);
        }
    }
}
