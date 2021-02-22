using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDebug : MonoBehaviour
{
    public static bool stop = false;
    public bool Stop {
        get { return stop; }
        set { stop = value;
            print(stop);
        } 
    }

    public void LateUpdate()
    {
        stop = false;
    }
}
