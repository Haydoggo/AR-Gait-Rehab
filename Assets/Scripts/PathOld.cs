using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PathOld : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> nodes = new List<Transform>();
    [HideInInspector]
    public Transform firstNode, lastNode;
    public GameObject nodePrefab;
    public List<Pose> bakedPoses;
    public float bakeResolution = 0.1f;
    public UnityEvent onUpdate = new UnityEvent();

    public float Length
    {
        get
        {
            return GetLength();
        }
        set { }
    }
    private class ControlPoint
    {
        public ControlPoint(Vector3 position)
        {
            this.position = this.inArm = this.outArm = position;
        }
        public Vector3 position = Vector3.zero;
        public Vector3 inArm = Vector3.zero;
        public Vector3 outArm = Vector3.zero;
    }
    private readonly List<ControlPoint> controlPoints = new List<ControlPoint>();

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
        if (nodes.Count == 0)
        {
            firstNode = node;
            controlPoints.Add(new ControlPoint(position));
            controlPoints.Add(new ControlPoint(position));
        }
        else
        {
            ControlPoint controlPoint = new ControlPoint((position + lastNode.position) * 0.5f);
            controlPoint.inArm = lastNode.position;
            controlPoint.outArm = position;
            controlPoints.Insert(controlPoints.Count - 1, controlPoint);

            ControlPoint lastControlPoint = controlPoints[controlPoints.Count - 1];
            lastControlPoint.position = position;
            lastControlPoint.inArm = position;
            lastControlPoint.outArm = position;

        }
        nodes.Add(node);
        lastNode = node;
        return node;
    }


    public void UpdateCurves()
    {
        if (nodes.Count > 0)
        {
            ControlPoint firstControlPoint = controlPoints[0];
            firstControlPoint.position = nodes[0].position;
            firstControlPoint.inArm = firstControlPoint.position;
            firstControlPoint.outArm = firstControlPoint.position;
            ControlPoint lastControlPoint = controlPoints[controlPoints.Count - 1];
            lastControlPoint.position = nodes[nodes.Count - 1].position;
            lastControlPoint.inArm = lastControlPoint.position;
            lastControlPoint.outArm = lastControlPoint.position;
        }
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            ControlPoint controlPoint = controlPoints[i + 1];
            controlPoint.position = (nodes[i].position + nodes[i + 1].position) * 0.5f;
            controlPoint.inArm = nodes[i].position;
            controlPoint.outArm = nodes[i + 1].position;
        }
        BakePath();
    }


    public float GetLength()
    {
        if (nodes.Count < 2)
        {
            return 0f;
        }
        else
        {
            float length = 0f;
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                length += (nodes[i + 1].position - nodes[i].position).magnitude;
            }
            return length;
        }
    }


    private Vector3 GetPointAlongSlow(float distAlong)
    {
        if (distAlong <= 0)
        {
            return firstNode.position;
        }
        else if (distAlong >= GetLength())
        {
            return lastNode.position;
        }
        float distChecked = 0f;
        // Iterate through all segments
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float segmentLength = (nodes[i + 1].position - nodes[i].position).magnitude;
            // Check if distAlong lands on the current segment
            if (distAlong > distChecked && distAlong < distChecked + segmentLength)
            {
                float distAlongSegment = distAlong - distChecked;
                bool pastHalfway = (distAlongSegment > segmentLength * 0.5f);
                int controlPointIndex = pastHalfway ? i + 1 : i;
                ControlPoint controlPoint1 = controlPoints[controlPointIndex];
                ControlPoint controlPoint2 = controlPoints[controlPointIndex + 1];
                float t = distAlongSegment / segmentLength; // from 0 to 1, how far distAlong is along the segment it lands on
                float lerpAmount;
                if (controlPointIndex == 0)
                {
                    lerpAmount = t * 2f;
                    return Vector3.Lerp(controlPoint1.position, controlPoint2.position, lerpAmount);
                }
                else if (controlPointIndex == controlPoints.Count - 2)
                {
                    lerpAmount = (t - 0.5f) * 2f;
                    return Vector3.Lerp(controlPoint1.position, controlPoint2.position, lerpAmount);
                }
                else
                    lerpAmount = (t + 0.5f) % 1f;
                Vector3 intermediatePoint1 = Vector3.Lerp(controlPoint1.position, controlPoint1.outArm, lerpAmount);
                Vector3 intermediatePoint2 = Vector3.Lerp(controlPoint2.inArm, controlPoint2.position, lerpAmount);
                return Vector3.Lerp(intermediatePoint1, intermediatePoint2, lerpAmount);
            }
            distChecked += segmentLength;
        }
        return Vector3.zero;
    }


    private Vector3 GetForwardVectorAlongSlow(float distAlong)
    {
        if (nodes.Count < 2)
        {
            return Vector3.forward;
        }
        if (distAlong <= 0)
        {
            return (nodes[1].position - firstNode.position).normalized;
        }
        else if (distAlong >= GetLength())
        {
            return (lastNode.position - nodes[nodes.Count - 2].position).normalized;
        }
        float distChecked = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float segmentLength = (nodes[i + 1].position - nodes[i].position).magnitude;
            if (distAlong < distChecked + segmentLength && distAlong > distChecked)
            {
                bool pastHalfway = ((distAlong - distChecked) > segmentLength * 0.5f);
                int controlPointIndex = pastHalfway ? i + 1 : i;
                ControlPoint controlPoint1 = controlPoints[controlPointIndex];
                ControlPoint controlPoint2 = controlPoints[controlPointIndex + 1];
                float t = (distAlong - distChecked) / segmentLength; // from 0 to 1, how far dist along is along the segment
                float lerpAmount;
                if (controlPointIndex == 0)
                    lerpAmount = t * 2f;
                else if (controlPointIndex == controlPoints.Count - 2)
                    lerpAmount = (t - 0.5f) * 2f;
                else
                    lerpAmount = (t + 0.5f) % 1f;
                Vector3 P0 = controlPoint1.position;
                Vector3 P1 = controlPoint1.outArm;
                Vector3 P2 = controlPoint2.position;
                return Vector3.Lerp(P1 - P0, P2 - P1, lerpAmount).normalized;
            }
            distChecked += segmentLength;
        }
        return Vector3.zero;
    }


    public void BakePath()
    {
        if (nodes.Count == 0)
        {
            return;
        }
        float length = GetLength();
        bakedPoses.Clear();
        for (float d = 0; d < length; d = Mathf.Min(d + bakeResolution, length))
        {
            Vector3 position = GetPointAlongSlow(d);
            Quaternion rotation = Quaternion.LookRotation(GetForwardVectorAlongSlow(d), Vector3.up);
            bakedPoses.Add(new Pose(position, rotation));
        }
        Vector3 lastPosition = lastNode.position;
        Vector3 lastForward;
        if (nodes.Count > 1)
            lastForward = (nodes[nodes.Count - 1].position - nodes[nodes.Count - 2].position).normalized;
        else
            lastForward = Vector3.forward;
        Quaternion lastRotation = Quaternion.LookRotation(lastForward, Vector3.up);
        bakedPoses.Add(new Pose(lastPosition, lastRotation));
    }


    public Vector3 GetPointAlong(float distAlong)
    {
        float length = GetLength();
        if (distAlong <= 0)
            return bakedPoses[0].position;

        if (distAlong >= length)
            return bakedPoses[bakedPoses.Count - 1].position;

        int subSegment = (int)(distAlong / bakeResolution);
        float distAlongSubSeg = distAlong % bakeResolution;
        return Vector3.Lerp(bakedPoses[subSegment].position, bakedPoses[subSegment + 1].position, distAlongSubSeg / bakeResolution);
    }


    public Vector3 GetForwardVectorAlong(float distAlong)
    {
        float length = GetLength();
        if (distAlong <= 0)
            return bakedPoses[0].forward;

        if (distAlong >= length)
            return bakedPoses[bakedPoses.Count - 1].forward;

        int subSegment = (int)(distAlong / bakeResolution);
        float distAlongSubSeg = distAlong % bakeResolution;
        return Vector3.Slerp(bakedPoses[subSegment].forward, bakedPoses[subSegment + 1].forward, distAlongSubSeg / bakeResolution);
    }


    public int GetSegmentAlong(float distAlong)
    {
        if (nodes.Count < 2)
        {
            return 0;
        }
        float distChecked = 0;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            distChecked += (nodes[i + 1].position - nodes[i].position).magnitude;
            if (distChecked > distAlong)
            {
                return i;
            }
        }
        return nodes.Count - 1;
    }

    public float GetDistAlong(Vector3 point)
    {
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
        controlPoints.Clear();
        foreach (Transform transform in nodes)
        {
            Destroy(transform.gameObject);
        }
        nodes.Clear();
    }
}
