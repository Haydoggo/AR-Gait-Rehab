using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Path : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> nodes = new List<Transform>();
    [HideInInspector]
    public Transform firstNode, lastNode;
    public GameObject nodePrefab;
    public List<Pose> bakedPoses;
    public float bakeResolution = 0.1f;
    public float bakeAccuracy = 0.01f;
    public UnityEvent onUpdate = new UnityEvent();

    public List<float> nodeDistances = new List<float>();

    public List<QuadCurve> curves = new List<QuadCurve>();
    private bool bakedPosesStale = true;
    [HideInInspector]
    public float bakedLength = 0;

    public float NodePathLength
    {
        get
        {
            if (nodeDistances.Count == 0)
            {
                return 0;
            }
            return nodeDistances[nodeDistances.Count - 1];
        }
        set { }
    }

    public Transform AddNode(Vector3 position)
    {
        Transform node;
        if (nodePrefab == null)
        {
            node = new GameObject().transform;
        }
        else
        {
            node = Instantiate(nodePrefab, transform).transform;
        }
        node.transform.position = position;

        curves.Add(new QuadCurve(position));
        if (nodes.Count == 0)
        {
            firstNode = node;
            nodeDistances.Add(0);
        }
        else
        {
            nodeDistances.Add(nodeDistances[nodeDistances.Count - 1] + (position - lastNode.position).magnitude);
        }
        nodes.Add(node);
        lastNode = node;
        bakedPosesStale = true;
        return node;
    }


    public void UpdateCurves()
    {
        if (nodes.Count > 0)
        {
            QuadCurve firstCurve = curves[0];
            Transform secondNode = nodes[Mathf.Min(nodes.Count - 1, 1)];

            firstCurve.P0 = firstNode.position + (firstNode.position - secondNode.position) * 0.5f;
            firstCurve.P1 = firstNode.position;
            firstCurve.P2 = (firstNode.position + secondNode.position) * 0.5f;

            QuadCurve lastCurve = curves[curves.Count - 1];
            Transform secondLastNode = nodes[Mathf.Max(0, nodes.Count - 2)];
            lastCurve.P0 = (lastNode.position + secondLastNode.position) * 0.5f;
            lastCurve.P1 = lastNode.position;
            lastCurve.P2 = lastNode.position + (lastNode.position - secondLastNode.position) * 0.5f;
        }
        for (int i = 1; i < curves.Count - 1; i++)
        {
            curves[i].P0 = (nodes[i - 1].position + nodes[i].position) * 0.5f;
            curves[i].P1 = nodes[i].position;
            curves[i].P2 = (nodes[i].position + nodes[i + 1].position) * 0.5f;
        }
        nodeDistances.Clear();
        nodeDistances.Add(0);
        for (int i = 1; i < nodes.Count; i++)
        {
            nodeDistances.Add(nodeDistances[nodeDistances.Count - 1] + (nodes[i].position - nodes[i - 1].position).magnitude);
        }
        BakePath();
    }

    // Travels distAlong meters along the node path and returns the corresponding point on the curve
    public Vector3 GetPointAlongNodePath(float distAlong)
    {
        Debug.Assert(nodes.Count > 0, "Cannot get point on empty path");
        if (distAlong <= 0)
            return firstNode.position;
        else if (distAlong >= NodePathLength)
            return lastNode.position;

        // i is the index of the last node along the node path before distAlong
        int i = GetSegmentAlong(distAlong);

        float distAlongSegment = (distAlong - nodeDistances[i]) / (nodeDistances[i + 1] - nodeDistances[i]);
        QuadCurve curve = distAlongSegment < 0.5f ? curves[i] : curves[i + 1];
        return curve.GetPointAlong((distAlongSegment + 0.5f) % 1f);
    }

    // Travels distAlong meters along the node path and returns the derivative of the corresponding point on the curve
    public Vector3 GetDerivativeAlongNodePath(float distAlong, bool normalized = false)
    {
        Debug.Assert(nodes.Count > 0, "Cannot get derivative of empty path");
        if (distAlong <= 0)
            return curves[0].GetDerivativeAlong(0.5f);
        else if (distAlong >= NodePathLength)
            return curves[curves.Count - 1].GetDerivativeAlong(0.5f);

        // i is the index of the last node along the node path before distAlong
        int i = GetSegmentAlong(distAlong);

        float distAlongSegment = (distAlong - nodeDistances[i]) / (nodeDistances[i + 1] - nodeDistances[i]);
        QuadCurve curve = distAlongSegment < 0.5f ? curves[i] : curves[i + 1];
        Vector3 retVal = curve.GetDerivativeAlong((distAlongSegment + 0.5f) % 1f);
        if (normalized)
            retVal = retVal.normalized;
        return retVal;
    }


    private void BakePath()
    {
        bakedPoses.Clear();
        Vector3 lastPos = firstNode.position;
        Vector3 forward = curves[0].GetDerivativeAlong(0.5f);
        Quaternion rotation;
        if (forward == Vector3.zero)
        {
            forward = Vector3.forward;
        }
        rotation = Quaternion.LookRotation(forward);
        bakedPoses.Add(new Pose(lastPos, rotation));
        float distAlong = bakeResolution;

        int iterations = 0;
        float distanceAdjustment = 0;
        while (distAlong < NodePathLength)
        {
            Vector3 pos = GetPointAlongNodePath(distAlong);
            float error = (pos - lastPos).magnitude - bakeResolution;
            int i = 0;
            while (error < 0 && distAlong < NodePathLength)
            {
                distAlong += bakeAccuracy;
                pos = GetPointAlongNodePath(distAlong);
                error = (pos - lastPos).magnitude - (bakeResolution - distanceAdjustment);
                if (++i > 10000)
                    break;
            }
            distanceAdjustment = error;
            rotation = Quaternion.LookRotation(GetDerivativeAlongNodePath(distAlong));
            bakedPoses.Add(new Pose(pos, rotation));
            lastPos = pos;
            if (iterations++ > NodePathLength / bakeResolution)
            {
                print("Looks like the path baking got stuck"); 
                break;
            }
        }
        bakedLength = (bakedPoses.Count - 1) * bakeResolution;
        bakedPosesStale = false;
    }


    public Vector3 GetPointAlong(float distAlong)
    {
        if (bakedPosesStale)
            BakePath();

        if (distAlong <= 0)
            return bakedPoses[0].position;

        if (distAlong >= bakedLength)
            return bakedPoses[bakedPoses.Count - 1].position;

        int subSegment = Mathf.FloorToInt(distAlong / bakeResolution);
        float distAlongSubSeg = distAlong % bakeResolution;
        return Vector3.Lerp(bakedPoses[subSegment].position, bakedPoses[subSegment + 1].position, distAlongSubSeg / bakeResolution);
    }


    public Vector3 GetDerivativeAlong(float distAlong, bool normalized = false)
    {
        if (bakedPosesStale)
            BakePath();

        if (distAlong <= 0)
            return bakedPoses[0].forward;

        if (distAlong >= bakedLength)
            return bakedPoses[bakedPoses.Count - 1].forward;

        int subSegment = (int)(distAlong / bakeResolution);
        float distAlongSubSeg = distAlong % bakeResolution;
        Vector3 retVal = Vector3.Slerp(bakedPoses[subSegment].forward, bakedPoses[subSegment + 1].forward, distAlongSubSeg / bakeResolution);
        if (normalized)
            retVal = retVal.normalized;
        return retVal;
    }

    public Pose GetPoseAlong(float distAlong)
    {
        Vector3 position = GetPointAlong(distAlong);
        Vector3 forward = GetDerivativeAlong(distAlong).normalized;
        Quaternion rotation = Quaternion.LookRotation(forward);
        return new Pose(position, rotation);
    }

    public int GetSegmentAlong(float distAlong)
    {
        if (nodes.Count < 2)
            return 0;

        int i = nodeDistances.BinarySearch(distAlong);
        if (i < 0)
            i = ~i - 1;
        return i;
    }

    public QuadCurve GetCurveAlong(float distAlong)
    {
        if (distAlong <= 0) return curves[0];
        if (distAlong >= NodePathLength) return curves[curves.Count - 1];
        int i = GetSegmentAlong(distAlong);
        float distAlongSegment = (distAlong - nodeDistances[i]) / (nodeDistances[i + 1] - nodeDistances[i]);
        QuadCurve curve = distAlongSegment < 0.5f ? curves[i] : curves[i + 1];
        return curve;
    }

    public float GetDistAlong(Vector3 point)
    {
        if (bakedPosesStale)
            BakePath();

        if (bakedPoses.Count < 2)
            return 0;

        float nearestDist = -1;
        float nearestDistAlong = 0;
        for (int i = 0; i < bakedPoses.Count - 2; i++)
        {
            Vector3 pathPoint = bakedPoses[i].position;
            Vector3 direction = bakedPoses[i].forward;

            float d = Mathf.Clamp(Vector3.Dot((point - pathPoint), direction), 0, bakeResolution);
            Vector3 proj = pathPoint + direction * d;

            float dist = (point - proj).sqrMagnitude;
            if (nearestDist < 0 || dist < nearestDist)
            {
                nearestDist = dist;
                nearestDistAlong = i * bakeResolution + d;
            }
        }
        return nearestDistAlong;
    }


    public Vector3 GetNearestPoint(Vector3 point)
    {
        if (bakedPosesStale)
            BakePath();

        if (bakedPoses.Count == 0)
            return Vector3.zero;
        if (bakedPoses.Count < 2)
            return firstNode.position;

        float nearestDist = -1;
        Vector3 nearest = Vector3.zero;
        for (int i = 0; i < bakedPoses.Count - 2; i++)
        {
            Vector3 pathPoint = bakedPoses[i].position;
            Vector3 direction = bakedPoses[i].forward;

            float d = Mathf.Clamp(Vector3.Dot((point - pathPoint), direction), 0, bakeResolution);
            Vector3 proj = pathPoint + direction * d;

            float dist = (point - proj).sqrMagnitude;
            if (nearestDist < 0 || dist < nearestDist)
            {
                nearestDist = dist;
                nearest = proj;
            }
        }
        return nearest;
    }


    public void Clear()
    {
        firstNode = null;
        lastNode = null;
        curves.Clear();
        foreach (Transform transform in nodes)
        {
            Destroy(transform.gameObject);
        }
        nodes.Clear();
    }
}
