using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PathNode : MonoBehaviour
{
    static float lastFloorHeight = 0;
    static bool floorHeightInitialised = false;
    private float floorHeight;
    private class TransformEvent : UnityEvent<Transform> { }
    public enum SelectionModes
    {
        Select,
        Move
    }

    private SelectionModes selectionMode = SelectionModes.Move;
    public SelectionModes SelectionMode
    {
        get { return selectionMode; }
        set
        {
            selectionMode = value;
            GetComponent<ObjectManipulator>().enabled = value == SelectionModes.Move;
            GetComponent<PointerHandler>().enabled = value == SelectionModes.Select;
        }
    }

    Vector3 cubeRestPosition;
    public UnityEvent<Transform> OnSelected = new TransformEvent();
    private Color targetColor;
    public void Start()
    {
        GetComponent<PointerHandler>().OnPointerDown.AddListener(Selected);
        FindObjectOfType<WorldFloor>().onFloorHeightFound.AddListener(InitialiseFloorHeight);

        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        int meshLayer = observer.DefaultPhysicsLayer;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hitInfo, 20, 1 << meshLayer))
        {
            lastFloorHeight = hitInfo.point.y;
        }
        floorHeight = lastFloorHeight;
    }

    void InitialiseFloorHeight(float height)
    {
        if (!floorHeightInitialised)
        {
            lastFloorHeight = height;
            floorHeightInitialised = true;
        }
    }

    public void Update()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        Vector3 ground = transform.Find("Cube/Mesh").position;
        ground.y = transform.parent.position.y;
        line.SetPosition(0, ground);
        line.SetPosition(1, transform.Find("Cube/Mesh").position);
        line.widthMultiplier = Mathf.Min((transform.Find("Cube").position.y - ground.y)*5f, 1.0f);
    }

    public void Selected(MixedRealityPointerEventData eventData)
    {
        OnSelected.Invoke(transform);
    }

    public void SetColor(string colourText)
    {
        ColorUtility.TryParseHtmlString(colourText, out Color color);
        color.a = 0.5f;
        if (gameObject.activeSelf)
        {
            StartCoroutine(FadeColorRoutine(color, 0.2f));
        } else
        {
            transform.Find("Cube/Mesh").GetComponent<MeshRenderer>().material.color = color;
        }
    }
    public void BounceBack()
    {
        StartCoroutine("BounceBackRoutine");
    }

    IEnumerator BounceBackRoutine()
    {
        //Store cube's transformation
        Transform cube = transform.Find("Cube");
        Vector3 cubePos = cube.position;
        Vector3 cubeUp = cube.up;

        //Move self back to ground
        //Vector3 targetPosition = transform.localPosition;
        //targetPosition.y = 0;
        //transform.localPosition = targetPosition;
        //transform.up = Vector3.up;

        Vector3 targetPosition = transform.position;
        targetPosition.y = floorHeight;
        transform.position = targetPosition;
        transform.up = Vector3.up;

        //Restore cube's transformation
        cube.position = cubePos;
        cube.up = cubeUp;
        //Slowly bring cube back to self
        Vector3 cubeTargetPosition = Vector3.zero;
        Vector3 cubeTargetUp = Vector3.up;
        while ((cube.up - cubeTargetUp).magnitude > 0.01 || (cube.localPosition - cubeTargetPosition).magnitude > 0.001)
        {
            cube.localPosition = Vector3.Lerp(cube.localPosition, cubeTargetPosition, 0.03f);
            cube.up = Vector3.Lerp(cube.up, cubeTargetUp, 0.05f);
            yield return null;
        }
        cube.localPosition = cubeTargetPosition;
        cube.up = cubeTargetUp;
    }

    IEnumerator FadeColorRoutine(Color newColor, float fadeTime)
    {
        Color oldColor = targetColor;
        targetColor = newColor;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            Color color = Color.Lerp(oldColor, newColor, t / fadeTime);
            transform.Find("Cube/Mesh").GetComponent<MeshRenderer>().material.color = color;
            yield return null;
        }
    }
}
