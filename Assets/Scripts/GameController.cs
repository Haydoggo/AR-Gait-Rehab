using UnityEngine;
using UnityEngine.Events;
public class GameController : MonoBehaviour
{
    public UnityEvent onStepEffect;

    private bool effectsEnabled = false;

    public void Awake()
    {
        RecursivelyInitialiseGraphs(transform);
    }

    public void RecursivelyInitialiseGraphs(Transform t)
    {
        foreach (Transform transform in t)
        {
            RecursivelyInitialiseGraphs(transform);
        }
        Graph graph = t.GetComponent<Graph>();
        if (graph != null)
        {
            graph.Awake();
        }
    }

    public void RegisterStep()
    {
        if (effectsEnabled)
        {
            onStepEffect.Invoke();
        }
    }

    public void ToggleEffects()
    {
        effectsEnabled = !effectsEnabled;
    }
}