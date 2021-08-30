using System.Collections;

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class Blackboard : ScriptableObject{
    [SerializeField]
    public List<BoolConstrain> boolConstrains = new List<BoolConstrain>();
    public List<FloatConstrain> floatConstrains = new List<FloatConstrain>();

    public BoolConstrain getboolcon(string name){
        foreach(BoolConstrain b in boolConstrains){
            if (b.name == name) return b;
        }
        return null;
    }
    public void setboolcon(string name,bool valu){
        var con = getboolcon(name);
        if(con != null){
            con.valu = valu;
        }

    }
    public FloatConstrain getfloatcon(string name)
    {
        foreach (FloatConstrain b in floatConstrains)
        {
            if (b.name == name) return b;
        }
        return null;
    }
    public void setfloatcon(string name, float valu)
    {
        var con = getfloatcon(name);
        if (con != null)
        {
            con.valu = valu;
        }

    }

}
[System.Serializable]
public class BoolConstrain
{
    public bool valu;
    public string name;
    public string beschreibung;

}
[System.Serializable]
public class FloatConstrain
{
    public float valu;
    public string name;
    public string beschreibung;
    //Operator
    public string op;


}