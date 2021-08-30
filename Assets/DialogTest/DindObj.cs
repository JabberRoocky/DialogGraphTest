using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DindObj : MonoBehaviour
{
    public MoveEvent[] events;
    void Start()
    {
        events.ToList().ForEach(e => { e.obj = this.gameObject; });

    }

    
}
