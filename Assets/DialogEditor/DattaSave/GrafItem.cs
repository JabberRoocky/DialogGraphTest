using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class GrafItem : ScriptableObject
{
    public string stardNodeGuid;
    public DialogNodeData curendNode;
    public Blackboard blackboard;
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<DialogNodeData> dialogNodeDatas = new List<DialogNodeData>();
    public List<CheckNodeData> checkNodeDatas = new List<CheckNodeData>();
    public List<ExecuteNodeData> executeNodeDatas = new List<ExecuteNodeData>();

    public DialogNodeData getCurrendNode()
    {
        return curendNode;
    }
    public void StardDialog()
    {
        Debug.Log(FindeNextNodeLinkData(stardNodeGuid).targetNodeGuid);
        curendNode = FindDialogNodeData(FindeNextNodeLinkData(stardNodeGuid).targetNodeGuid);
    }
    public CheckNodeData FindCheckNodeData(string guid)
    {
        CheckNodeData checkNode = null;
        try 
        {
            checkNode = checkNodeDatas.Where(x => (x.nodeGuid == guid)).First();
        }
        catch { }
        return checkNode;

    }
    public DialogNodeData FindDialogNodeData(string guid)
    {
        DialogNodeData checkNode = null;
        try
        {
            checkNode = dialogNodeDatas.Where(x => (x.nodeGuid == guid)).First();
        }
        catch { }
        return checkNode;

    }
    public ExecuteNodeData FindExecuteNodeData(string guid)
    {
        ExecuteNodeData checkNode = null;
        try
        {
            checkNode = executeNodeDatas.Where(x => (x.nodeGuid == guid)).First();
        }
        catch { }
        return checkNode;

    }
    // Funktioniert nicht bei DialogNodes da diese mehrrere links haben
    public NodeLinkData FindeNextNodeLinkData(NodeLinkData targetNodeLinkData)
    {
        return nodeLinks.Where(x => (x.baseNodeGuid == targetNodeLinkData.targetNodeGuid)).First();
    }
    public NodeLinkData FindeNextNodeLinkData(string guid)
    {
        return nodeLinks.Where(x => (x.baseNodeGuid == guid)).First();
    }
    public bool CheckPath(NodeLinkData nodeLinkData,int i = 0)
    {
        // verhindert Endlos Rekrusion fals ein Path im greis fürt und keinen Dialog beinhaltet i  = max Path länge
        if(i == 10)
        {
            return false;
        }
        else if (FindDialogNodeData(nodeLinkData.targetNodeGuid) != null)
        {
            return true;
        }
        else if (FindExecuteNodeData(nodeLinkData.targetNodeGuid) != null)
        {
            return CheckPath(FindeNextNodeLinkData(nodeLinkData),i++);
        }
        else if (FindCheckNodeData(nodeLinkData.targetNodeGuid) != null)
        {
            if (ValidateCheckNode(FindCheckNodeData(nodeLinkData.targetNodeGuid)))
            {
                return CheckPath(FindeNextNodeLinkData(nodeLinkData),i++);
            }
            else return false;

        }
        else
            return false;
    }
    public void GoPath(NodeLinkData nodeLinkData)
    {
        if (FindDialogNodeData(nodeLinkData.targetNodeGuid) != null)
        {
            curendNode = FindDialogNodeData(nodeLinkData.targetNodeGuid);
        }
        else if (FindExecuteNodeData(nodeLinkData.targetNodeGuid) != null)
        {
            if (FindExecuteNodeData(nodeLinkData.targetNodeGuid).Event != null)
            {
                FindExecuteNodeData(nodeLinkData.targetNodeGuid).Event.Execute();
            }
            GoPath(FindeNextNodeLinkData(nodeLinkData));

        }
        else if (FindCheckNodeData(nodeLinkData.targetNodeGuid) != null)
        {
            SetBlackBoard(FindCheckNodeData(nodeLinkData.targetNodeGuid));
            GoPath(FindeNextNodeLinkData(nodeLinkData));

        }
    }

    public void SetBlackBoard(CheckNodeData checkNode)
    {
        if (checkNode.set)
        {
            foreach (var boolconst in checkNode.boolConstrains)
            {
                var vergleichObj = blackboard.getboolcon(boolconst.name);
                if (vergleichObj != null)
                {
                    vergleichObj.valu = boolconst.valu;
                }
                else
                {
                    Debug.Log("constrain" + boolconst.name + " konnt nicht gefunden werden");
                }
            }
            foreach (var floatconst in checkNode.floatConstrains)
            {
                var vergleichObj = blackboard.getfloatcon(floatconst.name);
                if (vergleichObj != null)
                {
                    vergleichObj.valu = floatconst.valu;
                }
                else
                {
                    Debug.Log("constrain" + floatconst.name + " konnt nicht gefunden werden");
                }
            }
        }
    }  
    public bool ValidateCheckNode(CheckNodeData checkNode)
    {
        if (!checkNode.set)
        {
            foreach (var boolconst in checkNode.boolConstrains)
            {
                var vergleichObj = blackboard.getboolcon(boolconst.name);
                if (vergleichObj != null)
                {
                    if (vergleichObj.valu == boolconst.valu)
                    {

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Debug.Log("constrain" + boolconst.name + " konnt nicht gefunden werden");
                }
            }
            foreach (var floatconst in checkNode.floatConstrains)
            {
                var vergleichObj = blackboard.getfloatcon(floatconst.name);
                if (vergleichObj != null)
                {
                    if (floatconst.op == "<")
                    {
                        if (floatconst.valu >= vergleichObj.valu) return false;
                    }
                    else if (floatconst.op == ">")
                    {
                        if (floatconst.valu <= vergleichObj.valu) return false;
                    }
                    else if (floatconst.op == "=")
                    {
                        if (floatconst.valu != vergleichObj.valu) return false;
                    }
                }
                else
                {
                    Debug.Log("constrain" + floatconst.name + " konnt nicht gefunden werden");
                }

            }
        }
        return true;
    }
    public List<NodeLinkData> getAllAnswers()
    {
        return nodeLinks.Where(x => (x.baseNodeGuid == curendNode.nodeGuid)).ToList();
    }
    public List<NodeLinkData> getValidAnswers()
    {
        List<NodeLinkData> list = nodeLinks.Where(x => (x.baseNodeGuid == curendNode.nodeGuid)).ToList();
        List<NodeLinkData> list2 = new List<NodeLinkData>();
        foreach(var nodelinkdata in list){
            if(CheckPath(nodelinkdata))
            {
                list2.Add(nodelinkdata);
            }
        }
        return list2;
    }

}
[System.Serializable]
public class NodeLinkData
{
    
    public string baseNodeGuid;
    public string DE_Text;
    public string EG_Text;
    public string targetNodeGuid;
}

public class GuidData
{
    public string nodeGuid;
    public Vector2 position;
}
[System.Serializable]
public class DialogNodeData : GuidData
{
    public string titel;
    public string nodeType;
    public string DE_DialogueText; 
    public string EG_DialogueText;
}
[System.Serializable]
public class CheckNodeData : GuidData
{
    public string titel;
    public List<BoolConstrain> boolConstrains;
    public List<FloatConstrain> floatConstrains;
    public bool set;

}
[System.Serializable]
public class ExecuteNodeData : GuidData
{
    public DialogEvent Event;
    public string beschreibung;

}


