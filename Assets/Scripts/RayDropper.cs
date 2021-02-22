using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayDropper : MonoBehaviour
{
    Transform marker;
    LineRenderer line;
    
    void Awake()
    {
        marker = transform.Find("Marker");
        line = GetComponent<LineRenderer>();
    }
    void Update()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 2, 1 << 31))
        {
            marker.position = hitInfo.point;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, marker.position);
        }
    }
}
