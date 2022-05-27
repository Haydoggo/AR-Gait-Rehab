using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Periodic event generator
/// </summary>
public class RhythmGenerator : MonoBehaviour
{
    public UnityEvent onBeat;
    public RhythmGenerator nextBeat;
    [SerializeField]
    private float beatPeriod = 1.0f;

    public float BeatPeriod
    {
        get => beatPeriod;
        set => beatPeriod = value;
    }

    public float BeatFrequency
    {
        get { return 1f / BeatPeriod; }
        set { BeatPeriod = 1f / value; }
    }

    private float timeSinceBeat;

    void Update()
    {
        timeSinceBeat += Time.deltaTime;
        while (timeSinceBeat >= 1f / BeatFrequency)
        {
            onBeat.Invoke();
            timeSinceBeat -= 1f / BeatFrequency;
            if (nextBeat != null)
            {
                nextBeat.enabled = true;
                nextBeat.timeSinceBeat = timeSinceBeat;
                enabled = false;
            }
        }
    }
}