using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class MoveEvent : DialogEvent
{
    public string richtung;
    public Vector3 richtungsVec;
    public override void Execute()
    {
        obj.transform.position += richtungsVec;
        Debug.Log(richtung);
    }
}