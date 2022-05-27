using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExerciseController : MonoBehaviour
{
    public GameObject waypointPrefab;
    public PathController pathController;
    public RhythmGenerator rhythmGenerator;
    public GaitAnalyser gaitAnalyser;

    public UnityEvent onPositioned = new UnityEvent();
    public UnityEvent onPathBegun = new UnityEvent();
    public UnityEvent onEndReached = new UnityEvent();

    Waypoint startWaypoint;
    Waypoint endWaypoint;

    private bool positioned = false;
    private bool exercising = false;

    List<float> strideLengths = new List<float>();
    List<float> stridePeriods = new List<float>();


    public void Awake()
    {
        gaitAnalyser.onStepDetected.AddListener(RegisterStep);
    }

    void RegisterStep()
    {
        if (exercising)
        {
            strideLengths.Add(gaitAnalyser.MeasuredStrideLength);
            stridePeriods.Add(gaitAnalyser.MeasuredStridePeriod);
        }
    }
    public bool Exercising
    {
        get { return exercising; }
        set
        {
            if (value)
            {
                StopAllCoroutines();
                StartCoroutine(StartExercise());
            }
            else
            {
                StopAllCoroutines();
                if (startWaypoint != null) Destroy(startWaypoint);
                if (endWaypoint != null) Destroy(endWaypoint);
                rhythmGenerator.gameObject.SetActive(false);
                exercising = false;
            }
        }
    }

    IEnumerator StartExercise()
    {
        positioned = false;
        Transform closestNode = pathController.path.firstNode;
        Path path = pathController.path;
        if (Vector3.Distance(Camera.main.transform.position, path.lastNode.position) < Vector3.Distance(Camera.main.transform.position, closestNode.position))
        {
            closestNode = path.lastNode;
        }
        if ((closestNode == path.lastNode && pathController.Direction == 1) || (closestNode == path.firstNode && pathController.Direction == -1))
        {
            pathController.ChangePathDirection();
        }
        startWaypoint = Instantiate(waypointPrefab, WorldFloor.instance.transform).GetComponent<Waypoint>();
        startWaypoint.transform.position = closestNode.position;
        startWaypoint.onApproached.AddListener(() => positioned = true);
        yield return new WaitUntil(() => positioned);
        print("positioned");
        onPositioned.Invoke();

        UnityAction setExercisingTrue = () => exercising = true;
        rhythmGenerator.gameObject.SetActive(true);
        gaitAnalyser.onStepDetected.AddListener(setExercisingTrue);
        strideLengths.Clear();
        stridePeriods.Clear();

        yield return new WaitUntil(() => exercising);
        print("exercising");
        onPathBegun.Invoke();
        gaitAnalyser.onStepDetected.RemoveListener(setExercisingTrue);
        gaitAnalyser.LastStepTime = Time.time;
        endWaypoint = Instantiate(waypointPrefab, WorldFloor.instance.transform).GetComponent<Waypoint>();
        endWaypoint.transform.position = closestNode == path.firstNode ? path.lastNode.position : path.firstNode.position;
        //endWaypoint.transform.GetChild(0).gameObject.SetActive(false);
        endWaypoint.onApproached.AddListener(() => { Exercising = false; print("DONE"); onEndReached.Invoke(); });

        //yield return new WaitUntil(() => !exercising);
        //print("done");
        //onEndReached.Invoke();

    }
    public void DisplayResults(TMPro.TextMeshPro label)
    {
        float targStrideLength = pathController.LeftStrideLength;
        float targStridePeriod = rhythmGenerator.BeatPeriod;

        float aveStrideLength = 0;
        float aveStridePeriod = 0;

        for (int i = 0; i < strideLengths.Count; i++)
        {
            aveStrideLength += strideLengths[i] / strideLengths.Count;
            aveStridePeriod += stridePeriods[i] / stridePeriods.Count;
        }

        string text = "Exercise complete, well done! Here's how you did:";
        text += $"\n\nTarget stride length: {targStrideLength:F2}m\nYour average: {aveStrideLength:F2}m";
        text += $"\n\nTarget stride period: {targStridePeriod:F2}s\nYour average: {aveStridePeriod:F2}s";
        label.text = text;
    }
}
