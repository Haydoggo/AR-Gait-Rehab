 using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WorldFloor : MonoBehaviour
{
    public Utility.FloatEvent onFloorHeightFound;
    public static WorldFloor instance;
    bool observersEnabled = true;
    bool meshVisible = true;
    [HideInInspector]
    public float floorHeight = 0;

    public IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem;
    IMixedRealitySpatialAwarenessMeshObserver observer;

    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        spatialAwarenessSystem = CoreServices.SpatialAwarenessSystem;
        observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        FindFloorHeight();
    }

    public void FindFloorHeight()
    {
        StartCoroutine(FindFloorHeightRoutine());
    }
    private IEnumerator FindFloorHeightRoutine()
    {
        spatialAwarenessSystem.Enable();
        yield return null;
        if (!observer.IsRunning)
        {
            spatialAwarenessSystem.ResumeObservers();
        }
        spatialAwarenessSystem.ClearObservations();
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;


        floorHeight = Mathf.NegativeInfinity;
        int meshLayer = observer.DefaultPhysicsLayer;
        while (floorHeight < -10)
        {
            if (Physics.Raycast(Camera.main.transform.position, Vector3.down, out RaycastHit hitInfo, 20, 1 << meshLayer))
            {
                floorHeight = hitInfo.point.y;
                break;
            }
            yield return null;
        }

        Vector3 pos = transform.position;
        pos.y = floorHeight;
        transform.position = pos;

        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
        onFloorHeightFound.Invoke(floorHeight);
    }

    public void Recenter()
    {
        StartCoroutine(RecenterRoutine());
    }

    private IEnumerator RecenterRoutine()
    {
        Vector3 camPos = Camera.main.transform.localPosition;
        Vector3 targetPosition = new Vector3(camPos.x, transform.position.y, camPos.z);
        Vector3 targetForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        while ((transform.forward - targetForward).magnitude > 0.01 || (transform.position - targetPosition).magnitude > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);
            transform.forward = Vector3.Lerp(transform.forward, targetForward, 0.1f);
            yield return null;
        }
    }

    public void ClearMesh()
    {
        spatialAwarenessSystem.ClearObservations();
    }

    public void ToggleVisibility()
    {
        meshVisible = !meshVisible;
        if (meshVisible)
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
        }
        else
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
        }
    }

    public void ToggleObservers()
    {
        observersEnabled = !observersEnabled;
        if (observersEnabled)
        {
            spatialAwarenessSystem.ResumeObservers();
        }
        else
        {
            spatialAwarenessSystem.SuspendObservers();
        }
    }
}