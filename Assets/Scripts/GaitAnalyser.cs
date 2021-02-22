﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class GaitAnalyser : MonoBehaviour
{
    [SerializeField]
    private MotionProcessor motionProcessor;
    [SerializeField]
    private RhythmGenerator rhythmGenerator;
    public UnityEvent onStepDetected;

    //Number of samples over which velocity is averaged
    private readonly int windowSize = 10;
    //Alg
    private readonly int delay = 5;
    // Velocity threshold for forward motion
    private readonly float forwardVelocityThreshold = 0.5f;
    // Minimum period between two steps that will still trigger a step detection
    private readonly float minStepTime = 0.2f;

    public float UpperThreshold { get; set; } = 0f;
    public float LowerThreshold { get; set; } = 0f;

    public int StepsTaken { get; set; } = 0;

    public float StepCombo { get; set; } = 0;

    private bool readyForStep = true;
    private float lastStepTime;
    private Vector3 lastStepPosition;
    private float lastBeatTime;
    private List<float> timeErrorBuffer = new List<float>();

    public void Awake()
    {
        lastBeatTime = Time.time;
        lastStepPosition = motionProcessor.trackingTransform.position;
    }

    public void Update()
    {

        //Simulate step taking
        if (Input.GetKeyDown(KeyCode.F))
        {
            onStepDetected.Invoke();
        }

        // Do not start gait analysis before enough data is gathered in the motion buffers
        if (motionProcessor.allBuffers.Last().Count < Math.Max(windowSize, delay))
        {
            return;
        }

        // Check if cahnge in vertical acceleration surpasses threshold
        bool _sufficientAccelDelta = false;

        if (motionProcessor.deltaReading.Value < LowerThreshold)
        {
            readyForStep = true;
        }
        if (motionProcessor.deltaReading.Value > UpperThreshold && readyForStep)
        {
            _sufficientAccelDelta = true;
            readyForStep = false;
        }


        float _velocitySum = 0f;
        foreach (Vector3 vel in motionProcessor.VelocityBuffer.GetRange(0, windowSize))
        {
            _velocitySum += new Vector2(vel.x, vel.z).magnitude;
        }
        float _velocityAverage = _velocitySum / windowSize;
        bool _sufficientVelocity = _velocityAverage >= forwardVelocityThreshold;

        if (_sufficientVelocity && _sufficientAccelDelta)
        {
            if (Time.time - lastStepTime >= minStepTime)
            {
                onStepDetected.Invoke();
                RegisterStep();
                lastStepTime = Time.time;
            }
        }


    }


    public void RegisterBeat()
    {
        lastBeatTime = Time.time;
    }
    public void RegisterStep()
    {
        StepsTaken++;

        Vector3 stepPosition = motionProcessor.trackingTransform.position;
        Vector3 stride = stepPosition - lastStepPosition;

        float measuredStrideLength = new Vector2(stride.x, stride.z).magnitude;
        lastStepTime = Time.time;
        lastStepPosition = stepPosition;
        float timeError = Time.time - lastBeatTime;
        if (timeError > rhythmGenerator.BeatPeriod / 2)
        {
            timeError -= rhythmGenerator.BeatPeriod;
        }
        timeErrorBuffer.Add(timeError);
        if (timeErrorBuffer.Count > 0)
        {
            if (timeErrorBuffer.Count > 5)
            {
                timeErrorBuffer.RemoveAt(0);
            }
            float meanError = 0;
            foreach (float val in timeErrorBuffer)
            {
                meanError += val;
            }
            meanError /= timeErrorBuffer.Count;
            float errorVariance = 0;
            foreach (float val in timeErrorBuffer)
            {
                errorVariance += Mathf.Pow(val - meanError, 2);
            }
            errorVariance /= timeErrorBuffer.Count;
            float errorStdDeviation = Mathf.Sqrt(errorVariance);

            if (errorVariance < 0.0015)
            {
                StepCombo++;
            }
            else
            {
                StepCombo = 0;
            }
        }
    }
}
