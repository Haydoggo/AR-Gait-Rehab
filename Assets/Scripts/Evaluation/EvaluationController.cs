using UnityEngine;
using System;
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.Storage;
#endif

public class EvaluationController : MonoBehaviour
{
    public PathController pathController;
    public GaitAnalyser gaitAnalyser;

    public float PathLength { set; get; } = 10;

    public class StepDatum
    {
        public float length;
        public float period;
        public StepDatum(float l, float p)
        {
            length = l;
            period = p;
        }
    }

    List<StepDatum> stepData = new List<StepDatum>();
    private bool recording = false;
    public bool Recording { get => recording; set
        {
            recording = value;
            GetComponent<RhythmGenerator>().enabled = recording;
            if (recording)
            {
                stepData.Clear();
            } 
            else
            {
                SaveResults();
            }
        }
    }

    public Pose pathOrigin;

    public void UpdatePathPose()
    {
        pathOrigin = new Pose(Camera.main.transform.position, Camera.main.transform.rotation);
    }

    public void LayPath()
    {
        pathController.ClearTrack();
        for (float d = 0; d <= PathLength; d += 0.5f)
        {
            Vector3 nodePos = pathOrigin.position + pathOrigin.forward * d;
            nodePos.y = pathController.path.transform.position.y;
            pathController.PlaceNode(nodePos);
        }
        pathController.UpdatePath();
        pathController.SetPathVisible(false);
    }

    public void RecordStep()
    {
        stepData.Add(new StepDatum(gaitAnalyser.MeasuredStrideLength, gaitAnalyser.MeasuredStridePeriod));
    }

    private void OnEnable()
    {
        pathController.LeftStrideLength = 0.6f;
        pathController.RightStrideLength = 0.6f;
    }

    async void SaveResults()
    {
        List<string> data = new List<string>();
        data.Add("\"Length\", Period");
        foreach (StepDatum datum in stepData)
        {
            data.Add($"{datum.length}, {datum.period}");
        }
        data.Add($"Targets showing: {pathController.path.nodes.Count > 0}");
        data.Add($"Target step length: {pathController.LeftStrideLength}");
        data.Add($"Target step period: {GetComponent<RhythmGenerator>().BeatPeriod}");
#if WINDOWS_UWP
        StorageFolder folder = KnownFolders.PicturesLibrary;
        StorageFile file = await folder.CreateFileAsync($"{System.DateTime.Now.TimeOfDay:g}.csv".Replace(':', '_'));
        await FileIO.WriteLinesAsync(file, data);
#else
        foreach(string datum in data){
            print(datum);
        }
#endif
    }
}
