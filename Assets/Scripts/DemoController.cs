using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{
    GaitAnalyser gaitAnalyser;
    PathController pathController;
    TMPro.TextMeshPro label;
    void Start()
    {
        label = GetComponent<TMPro.TextMeshPro>();
        gaitAnalyser = FindObjectOfType<GaitAnalyser>();
        pathController = FindObjectOfType<PathController>();
    }

    // Update is called once per frame
    void Update()
    {
        label.text = "";
        if (pathController.LayingTrack)
        {
            float length = pathController.path.bakedLength;
            Vector3 lastNodePos = pathController.path.lastNode.position;
            Vector3 d = pathController.pathLayingTransform.position - lastNodePos;
            length += new Vector2(d.x, d.z).magnitude;
            label.text += $"Path length: {length:F2}m\n";
        }
    }
}
