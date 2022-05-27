using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class GameController : MonoBehaviour
{
    public UnityEvent onStepEffect;

    private bool effectsEnabled = false;

    public void Awake()
    {
        foreach (Graph graph in FindObjectsOfType<Graph>()) graph.Awake();
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