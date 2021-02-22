using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PathTracker : MonoBehaviour
{
    // Snaps gameobject's transform to the path.
    // Can be set to follow a transform while still sticking to the path.
    public Path path;
    public float distAlong;
    public Transform followTarget;
    public bool colorNodes = false;

    private Vector3 targetPosition;
    private Transform backNode;
    private Transform middleNode;
    private Transform frontNode;

    private void Start()
    {
        path.onUpdate.AddListener(UpdateNodes);
    }
    private void Update()
    {
        if (path.nodes.Count < 3)
        {
            return;
        }
        else
        {
            if (middleNode == null)
            {
                UpdateNodes();
            }
            float distToFrontNode = FlattenVector(followTarget.position - frontNode.position).magnitude;
            float distToMiddleNode = FlattenVector(followTarget.position - middleNode.position).magnitude;
            float distToBackNode = FlattenVector(followTarget.position - backNode.position).magnitude;

            if (colorNodes)
            {
                frontNode.GetComponent<PathNode>().SetColor("white");
                middleNode.GetComponent<PathNode>().SetColor("white");
                backNode.GetComponent<PathNode>().SetColor("white");
            }

            if (distToFrontNode < distToMiddleNode && frontNode != path.lastNode)
            {
                backNode = middleNode;
                middleNode = frontNode;
                frontNode = path.nodes[path.nodes.IndexOf(frontNode) + 1];
                StopAllCoroutines();
                StartCoroutine(SmoothFollowRoutine(path.nodeDistances[path.nodes.IndexOf(middleNode)]));
            }

            else if (distToBackNode < distToMiddleNode && backNode != path.firstNode)
            {
                frontNode = middleNode;
                middleNode = backNode;
                backNode = path.nodes[path.nodes.IndexOf(backNode) - 1];
                StopAllCoroutines();
                StartCoroutine(SmoothFollowRoutine(path.nodeDistances[path.nodes.IndexOf(middleNode)]));
            }
            if (colorNodes)
            {
                frontNode.GetComponent<PathNode>().SetColor("orange");
                middleNode.GetComponent<PathNode>().SetColor("cyan");
                backNode.GetComponent<PathNode>().SetColor("magenta");
            }
        }
    }

    IEnumerator SmoothFollowRoutine(float targetDistAlong, float followTime = 1f)
    {
        float originalDistAlong = distAlong;
        for(float d = 0f; d < followTime; d += Time.deltaTime)
        {
            distAlong = Mathf.Lerp(originalDistAlong, targetDistAlong, d);
            transform.position = path.GetPointAlong(distAlong);
            yield return null;
        }
    }
    
    void UpdateNodes()
    {
        UpdateNodes(false);
    }

    // Check that all referenced nodes still exist in the path being followed
    public void UpdateNodes(bool forceRelocate = false)
    {
        if (path.nodes.Count < 3)
        {
            return;
        }
        if (!path.nodes.Contains(backNode) || !path.nodes.Contains(middleNode) || !path.nodes.Contains(backNode) || forceRelocate)
        {
            float nearestDist = -1f;
            int nearestIndex = 0;
            for(int i = 0; i < path.nodes.Count; i++)
            {
                Transform node = path.nodes[i];
                float dist = (targetPosition - node.position).sqrMagnitude;
                if(nearestDist < 0f || dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestIndex = i;
                }
            }
            nearestIndex = Mathf.Clamp(nearestIndex, 1, path.nodes.Count - 2);
            backNode = path.nodes[nearestIndex - 1];
            middleNode = path.nodes[nearestIndex];
            frontNode = path.nodes[nearestIndex + 1];
            targetPosition = middleNode.position;
        }
    }


    Vector3 FlattenVector(Vector3 vec)
    {
        Vector3 flatVec = vec;
        flatVec.y = 0;
        return flatVec;
    }

    public void SendToStart()
    {
        distAlong = 0;
        targetPosition = path.GetPointAlong(distAlong);
        UpdateNodes(true);
    }

    public void SendToEnd()
    {
        distAlong = path.bakedLength;
        targetPosition = path.GetPointAlong(distAlong);
        UpdateNodes(true);
    }
}
