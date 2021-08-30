using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogEvent : ScriptableObject
{
    public GameObject obj;
    public abstract void Execute();

}

