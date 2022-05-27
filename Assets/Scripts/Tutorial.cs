using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
    public UnityEvent onAnyStepCompleted = new UnityEvent();

    public UnityEvent[] events;
    
    public int CurrentStep { set; get; } = 0;
    int currentStepProgress = 0;

    bool repeating = false;

    public void RepeatStep(int count)
    {
        repeating = true;
        currentStepProgress++;
        if (currentStepProgress >= count)
        {
            repeating = false;
            events[++CurrentStep].Invoke();
            currentStepProgress = 0;
        }
    }

    public void CompleteStep(int step)
    {
        if (CurrentStep != step)
            return;
        print($"Step {step} registered");
        
        events[CurrentStep].Invoke();
        if (!repeating)
        {
            onAnyStepCompleted.Invoke();
            CurrentStep++;
        }
    }
}
