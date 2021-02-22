using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
    public UnityEvent onAnyStepCompleted = new UnityEvent();

    public UnityEvent onOpenMenu = new UnityEvent();
    public UnityEvent onDetachMenu = new UnityEvent();
    public UnityEvent onGoToTrackEditor = new UnityEvent();
    public UnityEvent onStartTrack = new UnityEvent();
    public UnityEvent onTrackNodePlaced = new UnityEvent();
    public UnityEvent onMenuReopened = new UnityEvent();
    public UnityEvent onTrackFinished = new UnityEvent();

    public List<UnityEvent> evonts = new List<UnityEvent>();

    private struct TutorialEvent
    {
        public UnityEvent unityEvent;
        public int requiredActivations;
        public TutorialEvent(UnityEvent unityEvent, int requiredActivations = 1)
        {
            this.unityEvent = unityEvent;
            this.requiredActivations = requiredActivations;
        }
    }

    private Dictionary<TutorialSteps, TutorialEvent> events;
    public enum TutorialSteps
    {
        OpenMenu,
        DetachMenu,
        GoToTrackEditor,
        StartTrack,
        PlaceTrackNodes,
        ReopenMenu,
        FinishTrack,

        SIZE
    }
    TutorialSteps currentStep = (TutorialSteps)0;
    int currentStepProgress;
    private void Awake()
    {
        events = new Dictionary<TutorialSteps, TutorialEvent>
        {
            {TutorialSteps.OpenMenu, new TutorialEvent(onOpenMenu)},
            {TutorialSteps.DetachMenu, new TutorialEvent(onDetachMenu) },
            {TutorialSteps.GoToTrackEditor, new TutorialEvent(onGoToTrackEditor)},
            {TutorialSteps.StartTrack, new TutorialEvent(onStartTrack)},
            {TutorialSteps.PlaceTrackNodes, new TutorialEvent(onTrackNodePlaced, 4)},
            {TutorialSteps.ReopenMenu, new TutorialEvent(onMenuReopened)},
            {TutorialSteps.FinishTrack, new TutorialEvent(onTrackFinished)},
        };
    }

    public void CompleteStep(TutorialSteps step)
    {
        if (currentStep != step)
            return;
        currentStepProgress++;
        print($"Step {step} registered");
        if (currentStepProgress >= events[step].requiredActivations)
        {
            onAnyStepCompleted.Invoke();
            events[step].unityEvent.Invoke();
            currentStep++;
            currentStepProgress = 0;
            print($"Step {step} completed");
        }

    }

    // This method only exists so I can link the CompleteStep method above to the unity
    // editor because for some unknowable reason, unity doesn't support enums as static parameters(??)
    public void CompleteStep(int step)
    {
        CompleteStep((TutorialSteps)step);
    }
}
