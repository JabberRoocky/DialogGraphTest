using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility 
{
    private DialogGraphView _targetGraphView;
    private GrafItem _containerCache;
    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<ExecuteNode> ExecuteNodes => _targetGraphView.nodes.ToList().Where(x => x.GetType() == typeof(ExecuteNode)).Cast<ExecuteNode>().ToList();
    private List<DialogNode> DialogNodes => _targetGraphView.nodes.ToList().Where(x => x.GetType() == typeof(DialogNode)).Cast<DialogNode>().ToList();
    private List<CheckNode> CheckNodes => _targetGraphView.nodes.ToList().Where(x => x.GetType() == typeof(CheckNode)).Cast<CheckNode>().ToList();
    public static GraphSaveUtility GetInstance(DialogGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };

    }
    public void SaveGraph(string fileName)
    {
        if (!Edges.Any()) return;

        var dialogeContainer = ScriptableObject.CreateInstance<GrafItem>();
        var conectedEdges = Edges.Where(x => x.input.node != null).ToArray();
        dialogeContainer.blackboard = Resources.Load<Blackboard>($"Blackboard");
        

        for (var i = 0; i< conectedEdges.Length;i++)
        {

            var output = conectedEdges[i].output.node as GuidNode;
            var inputNode = conectedEdges[i].input.node as GuidNode;
            var de_Text = "";
            var eg_Text = "";




            foreach (var temp in conectedEdges[i].output.Children())
            {
                //Debug.Log(temp.GetType());
                if (temp.GetType().Equals(typeof(Box))) 
                {
                    Debug.Log(((TextField)temp.Children().ElementAt(0)).text);
                    Debug.Log(((TextField)temp.Children().ElementAt(1)).text);
                    de_Text = ((TextField)temp.Children().ElementAt(0)).text;
                    eg_Text = ((TextField)temp.Children().ElementAt(1)).text;
                }
            }
            Debug.Log(dialogeContainer);
            Debug.Log(dialogeContainer.nodeLinks.Count);
            dialogeContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = output.GUID,
                DE_Text = de_Text,
                EG_Text = eg_Text,
                targetNodeGuid = inputNode.GUID
                

            }); 
        }
        foreach(var dialogNode in DialogNodes.Where(node => !node.Enter))
        {
            dialogeContainer.dialogNodeDatas.Add(new DialogNodeData
            {
                nodeGuid = dialogNode.GUID,
                DE_DialogueText = dialogNode.DE_DialogText,
                EG_DialogueText = dialogNode.EG_DialogText,
                position = dialogNode.GetPosition().position,
                titel = dialogNode.title
            });


        }
        foreach(var checkNode in CheckNodes)
        {
            dialogeContainer.checkNodeDatas.Add(new CheckNodeData
            {
                nodeGuid = checkNode.GUID,
                position = checkNode.GetPosition().position,
                titel = checkNode.title,
                boolConstrains = checkNode.boolConstrains,
                floatConstrains = checkNode.floatConstrains,
                set = checkNode.set
            }) ;

        }
        foreach(var exNode in ExecuteNodes)
        {
            dialogeContainer.executeNodeDatas.Add(new ExecuteNodeData
            {
                nodeGuid = exNode.GUID,
                position = exNode.GetPosition().position,
                beschreibung = exNode.beschreibung,
                Event = exNode.Event

            });
            
        }


        DialogNode stard = DialogNodes.Where(node => node.Enter).First();
        dialogeContainer.stardNodeGuid = stard.GUID;


        if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogs"))
        {
            AssetDatabase.CreateFolder("Resources", "Dialogs");
        }
        GrafItem item = null;
        try
        {
            item = Resources.Load<GrafItem>($"Dialogs/{fileName}");
        }
        catch
        {
            
        }
        if(item != null)
        {
            item.checkNodeDatas = dialogeContainer.checkNodeDatas;
            item.dialogNodeDatas = dialogeContainer.dialogNodeDatas;
            item.executeNodeDatas = dialogeContainer.executeNodeDatas;
            item.nodeLinks = dialogeContainer.nodeLinks;
            item.stardNodeGuid = dialogeContainer.stardNodeGuid;
            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        else
        {
        AssetDatabase.CreateAsset(dialogeContainer, $"Assets/Resources/Dialogs/{fileName}.asset");
        AssetDatabase.SaveAssets();

        }


    }

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<GrafItem>($"Dialogs/{fileName}");
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found ", "Target dialoug Graph file dos not exist ", " ok ");
            return;
        }

        ClearGraph();
        CreateDialogNodes();
        CreateCheckNodes();
        CreateExecutNodes();
        ConnectNodes();

        
    }
    private void CreateExecutNodes()
    {
        foreach(var nodeData in _containerCache.executeNodeDatas)
        {
            var tempNode = _targetGraphView.CreateExecutNode(nodeData.position, nodeData.beschreibung, nodeData.Event);
            tempNode.GUID = nodeData.nodeGuid;
            _targetGraphView.AddElement(tempNode);

        }

    }
    private void CreateDialogNodes()
    {
        foreach(var nodeData in _containerCache.dialogNodeDatas)
        {
            var tempNode = _targetGraphView.CreateDialogNode(nodeData.titel, nodeData.DE_DialogueText,nodeData.EG_DialogueText,nodeData.position);
            tempNode.GUID = nodeData.nodeGuid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.nodeGuid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.DE_Text,x.EG_Text));
            
        }
    }
    private void CreateCheckNodes()
    {
        foreach(var node in _containerCache.checkNodeDatas)
        {
            var tempNode = _targetGraphView.CreateCheckNode(node.position , node);
            tempNode.GUID = node.nodeGuid;

            foreach(BoolConstrain boolConstrain in node.boolConstrains)
            {
                _targetGraphView.AddBoolCheckConstrain(ref tempNode, boolConstrain);
            }
            foreach(FloatConstrain floatConstrain in node.floatConstrains)
            {
                _targetGraphView.AddFloatCheckConstrain(ref tempNode, floatConstrain);
            }


            _targetGraphView.AddElement(tempNode);


        }

    }
    private void ClearGraph()
    {


        //Setzt dehn GUID der Start Node Zurück
        // Fals sich diese Ändert würde Es Zu Problemen Bei der Verkättung bei laden kommen
        DialogNodes.Find(x => x.Enter).GUID = _containerCache.nodeLinks[0].baseNodeGuid;

        foreach(var node in DialogNodes)
        {
            if (node.Enter) continue;
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            _targetGraphView.RemoveElement(node);
        }
        foreach (var node in CheckNodes)
        {
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            _targetGraphView.RemoveElement(node);
        }
        foreach(var node in ExecuteNodes)
        {
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            _targetGraphView.RemoveElement(node);
        }
    }
    private void ConnectNodes()
    {
            List<GuidNode> allNodes = new List<GuidNode>();
            _targetGraphView.nodes.ForEach(x => allNodes.Add(x as GuidNode));
            Port output;
            Port input;


        for(int i = 0; i < allNodes.Count;i++)
        {
            var connections = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == allNodes[i].GUID).ToList();
            for(int j = 0; j<connections.Count;j++)
            {
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = allNodes.First(x => x.GUID == targetNodeGuid);
                output = allNodes[i].outputContainer[j].Q<Port>();
                input = (Port)targetNode.inputContainer[0];
                LinkNodes(output, input);
            }
        }
    }
    private void LinkNodes(Port output,Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };
        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

    

  
}
