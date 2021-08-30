using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;


public class GuidNode : Node
{
    public string GUID;
}
public class DialogNode : GuidNode
{
    public string DE_DialogText;
    public string EG_DialogText;
    public bool Enter;
}
public class CheckNode : GuidNode
{
    public List<BoolConstrain> boolConstrains;
    public List<FloatConstrain> floatConstrains;
    public bool set = false;
}
public class ExecuteNode : GuidNode
{
    public string beschreibung;
    public DialogEvent Event;

}

