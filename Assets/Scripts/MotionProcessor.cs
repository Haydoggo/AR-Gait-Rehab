using System.Collections.Generic;
using UnityEngine;


public enum motionType
{
    position, velocity, acceleration, jerk
}
/// <summary>
/// Records motion info for a Transform
/// </summary>
public class MotionProcessor : MonoBehaviour
{

    [Tooltip("Transform for which the motion is tracked")]
    public Transform trackingTransform;

    [Tooltip("Maximum number of recorded samples stored")]
    public int maxBufferSize = 60;

    // Buffers to store position, velocity, and acceleration over time.
    // Values towards the start of the buffers are newest.
    public List<Vector3> PositionBuffer { get; set; } = new List<Vector3>();
    public List<Vector3> VelocityBuffer { get; set; } = new List<Vector3>();
    public List<Vector3> AccelerationBuffer { get; set; } = new List<Vector3>();
    public List<Vector3> JerkBuffer { get; set; } = new List<Vector3>();

    private readonly string[] bufferNames = {"position", "velocity", "Acceleration", "Jerk"};

    [HideInInspector]
    public List<List<Vector3>> allBuffers = new List<List<Vector3>>();

    // Delay (in samples) used for deltaReading"
    // Though this is only used as an int, the datatype is float to allow a pinchSliderWrapper event to write to it
    public float Delay { get; set; } = 5;

    public FloatSmoothed Reading = new FloatSmoothed(0.95f);
    public FloatSmoothed ReadingDelayed = new FloatSmoothed(0.95f);
    public FloatSmoothed deltaReading = new FloatSmoothed(0.95f);

    public Label label;
    private List<Vector3> bufferOfInterest;
    public motionType motionOfInterest;
    private int bufferOfInterestIndex;

    public bool absoluteReadingValues;


    public void Awake()
    {
        bufferOfInterestIndex = (int)motionOfInterest;
        if (trackingTransform == null)
        {
            trackingTransform = gameObject.transform;
        }

        allBuffers.Add(PositionBuffer);
        allBuffers.Add(VelocityBuffer);
        allBuffers.Add(AccelerationBuffer);
        allBuffers.Add(JerkBuffer);
        bufferOfInterest = allBuffers[bufferOfInterestIndex];
    }

    public void Start()
    {
        bufferOfInterestIndex -= 1;
        ChangeBuffer();
    }

    private void Update()
    {
        Vector3 _newPos = trackingTransform.position;
        PositionBuffer.Insert(0, _newPos);

        // Update each of the subsequent derivative buffers
        for (int i = 1; i < allBuffers.Count; i++)
        {
            List<Vector3> _sourceBuffer = allBuffers[i - 1];
            List<Vector3> _derivativeBuffer = allBuffers[i];
            if (_sourceBuffer.Count > 1)
            {
                Vector3 _newVal = (_sourceBuffer[0] - _sourceBuffer[1]) / Time.deltaTime;
                _derivativeBuffer.Insert(0, _newVal);

                // Enforce max buffer size
                if (_derivativeBuffer.Count > maxBufferSize)
                {
                    _derivativeBuffer.RemoveAt(_derivativeBuffer.Count - 1);
                }
            }
        }

        if (PositionBuffer.Count > maxBufferSize)
        {
            PositionBuffer.RemoveAt(PositionBuffer.Count - 1);
        }

        if (JerkBuffer.Count > Delay)
        {
            if (absoluteReadingValues)
            {
                Reading.Value = Mathf.Abs(bufferOfInterest[0].y);
                ReadingDelayed.Value = Mathf.Abs(bufferOfInterest[(int)Delay].y);
            }
            else
            {
                Reading.Value = bufferOfInterest[0].y;
                ReadingDelayed.Value = bufferOfInterest[(int)Delay].y;
            }
            deltaReading.Value = Reading.Value - ReadingDelayed.Value;
        }

    }

    public void ChangeBuffer()
    {
        bufferOfInterestIndex++;
        if (bufferOfInterestIndex >= allBuffers.Count)
        {
            bufferOfInterestIndex = 0;
        }
        bufferOfInterest = allBuffers[bufferOfInterestIndex];
        label.UpdateText(bufferNames[bufferOfInterestIndex]);
    }

    public void toggleAbsoluteReadings()
    {
        absoluteReadingValues = !absoluteReadingValues;
    }

    public void setReadingAlpha(float alpha)
    {
        Reading.Alpha = alpha;
        ReadingDelayed.Alpha = alpha;
    }

    public void setDeltaAlpha(float alpha)
    {
        deltaReading.Alpha = alpha;
    }
}