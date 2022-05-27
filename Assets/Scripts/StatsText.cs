using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsText : MonoBehaviour
{
    GaitAnalyser gaitAnalyser;
    ExerciseController exerciseController;
    TMPro.TextMeshPro label;
    void Start()
    {
        label = GetComponent<TMPro.TextMeshPro>();
        gaitAnalyser = FindObjectOfType<GaitAnalyser>();
        exerciseController = FindObjectOfType<ExerciseController>();
    }

    // Update is called once per frame
    void Update()
    {
        label.text = "";
        if (gaitAnalyser != null)
        {
            label.text += $"Steps taken: {gaitAnalyser.StepsTaken}\n";
            label.text += $"Measured stride length: {gaitAnalyser.MeasuredStrideLength:F2}m\n";
            label.text += $"Measured stride period: {gaitAnalyser.MeasuredStridePeriod:F2}s\n";
        }
        if (gaitAnalyser != null && exerciseController != null && exerciseController.Exercising)
        {
            label.text += $"Good step combo: {gaitAnalyser.StepCombo}\n";
            label.text += $"Step error: {gaitAnalyser.TimeError:G2}s\n";
            label.text += $"Step error Std Dev: {gaitAnalyser.ErrorStdDeviation:G2}s\n";
        }
    }
}
