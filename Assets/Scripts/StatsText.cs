using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsText : MonoBehaviour
{
    public GaitAnalyser gaitAnalyser;
    public TMPro.TextMeshPro label;
    void Start()
    {
        label = GetComponent<TMPro.TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        label.text = "";
        label.text += $"Steps taken: {gaitAnalyser.StepsTaken}\n";
        label.text += $"Combo : {gaitAnalyser.StepCombo}\n";
    }
}
