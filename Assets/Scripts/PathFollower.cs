using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Path path;
    public float travelSpeed;
    public float distAlong;
    // Update is called once per frame
    void Update()
    {
        if (path.nodes.Count > 1)
        {
            transform.position = path.GetPointAlong(distAlong);
            transform.forward = path.GetDerivativeAlong(distAlong);
            if (travelSpeed < 0)
                transform.forward *= -1;
            distAlong += travelSpeed * Time.deltaTime;
            while(distAlong > path.bakedLength)
                distAlong -= path.bakedLength;
            while (distAlong < 0)
                distAlong += path.bakedLength;
        }
    }
}
