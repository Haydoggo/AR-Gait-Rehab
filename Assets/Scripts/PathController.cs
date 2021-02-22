using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
public class PathController : MonoBehaviour
{
    public Path path;
    public Material lineMaterial;
    public float nodeSpacing;
    public GameObject leftFootprint;
    public GameObject rightFootprint;
    public GameObject directionIndicatorPrefab;

    public enum Side
    {
        left,
        right
    }

    public UnityEvent onNodePlaced = new UnityEvent();
    [HideInInspector]
    public UnityEvent onFootprintsUpdated = new UnityEvent();

    private List<GameObject> directionIndicators = new List<GameObject>();
    private int direction = 1;

    private StrideLengthPair currentStrideLengths = new StrideLengthPair(0, 0);

    private float leftStrideLength;
    private float rightStrideLength;
    public float LeftStrideLength
    {
        set
        {
            currentStrideLengths.left = value;
            leftStrideLength = value;
        }
        get { return leftStrideLength; }
    }
    public float RightStrideLength
    {
        set
        {
            currentStrideLengths.right = value;
            rightStrideLength = value;
        }
        get { return rightStrideLength; }
    }

    public struct StrideLengthPair
    {
        public float left;
        public float right;
        public StrideLengthPair(float left, float right)
        {
            this.left = left;
            this.right = right;
        }

        public static bool operator ==(StrideLengthPair slp1, StrideLengthPair slp2)
        {
            return (slp1.left == slp2.left && slp1.right == slp2.right);
        }

        public static bool operator !=(StrideLengthPair slp1, StrideLengthPair slp2)
        {
            return (slp1.left != slp2.left || slp1.right != slp2.right);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    private float strideWidth;
    public float StrideWidth
    {
        set
        {
            strideWidth = value;
            GenerateFootprints();
        }
        get
        {
            return strideWidth;
        }
    }

    public class Footprint
    {
        public GameObject gameObject;
        public float distAlong;
    }
    private PathNode.SelectionModes selectionMode = PathNode.SelectionModes.Move;
    private AnchorStoreManager anchorStoreManager;
    public List<Footprint> footprints = new List<Footprint>();

    public List<StrideLengthPair> strideLengthPairs = new List<StrideLengthPair>();

    private bool layingTrack = false;
    private LineRenderer line;
    private bool pathVisible = false;
    private Transform selectedNode = null;
    void Start()
    {
        anchorStoreManager = FindObjectOfType<AnchorStoreManager>();
        //anchorStoreManager.onAnchorStoreInitialised.AddListener(LoadPath);
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.startWidth = line.endWidth = 0.05f;
        if (lineMaterial != null)
        {
            line.material = lineMaterial;
        }
    }

    void Update()
    {
        if (layingTrack)
        {
            Vector3 dist = Camera.main.transform.position - path.lastNode.position;
            float flatDist = new Vector2(dist.x, dist.z).magnitude;
            if (flatDist >= nodeSpacing)
            {
                Vector3 pos = Camera.main.transform.position;
                pos.y = path.transform.position.y;
                PlaceNode(pos);
                UpdatePath();
            }
        }
    }

    public void HighlightKeyNodes()
    {
        StrideLengthPair lastStrideLengths = new StrideLengthPair(0, 0);
        for (int i = 0; i < path.nodes.Count; i++)
        {
            PathNode node = path.nodes[i].GetComponent<PathNode>();
            bool keyNode;
            if (i < path.nodes.Count - 1)
            {
                keyNode = strideLengthPairs[i] != lastStrideLengths;
            }
            else
            {
                keyNode = true;
            }
            node.SetColor(keyNode ? "yellow" : "gray");
            lastStrideLengths = strideLengthPairs[i];
        }
    }

    void SelectNode(Transform node)
    {
        Transform oldNode = selectedNode;
        selectedNode = node;
        if (oldNode != null)
        {
            int oldNodeIndex = path.nodes.IndexOf(oldNode);
            int newNodeIndex = path.nodes.IndexOf(selectedNode);
            int firstNodeIndex = Mathf.Min(oldNodeIndex, newNodeIndex);
            int lastNodeIndex = Mathf.Max(newNodeIndex, oldNodeIndex);
            for (int i = firstNodeIndex; i < lastNodeIndex; i++)
            {
                strideLengthPairs[i] = new StrideLengthPair(LeftStrideLength, RightStrideLength);
            }
            GenerateFootprints();
            HighlightKeyNodes();
            selectedNode = null;
        }
    }

    public void ClearTrack()
    {
        path.Clear();
        strideLengthPairs.Clear();
        foreach (Footprint footprint in footprints)
        {
            Destroy(footprint.gameObject);
        }
        footprints.Clear();
        line.positionCount = 0;
    }

    public void StartTrack()
    {
        ClearTrack();
        layingTrack = true;
        Vector3 pos = Camera.main.transform.position;
        pos.y = path.transform.position.y;
        PlaceNode(pos);
        UpdatePath();
    }

    public void SetSelectionMode(PathNode.SelectionModes mode)
    {
        selectionMode = mode;
        foreach (Transform node in path.nodes)
        {
            node.GetComponent<PathNode>().SelectionMode = mode;
        }
    }

    public void SetSelectionMode(bool select)
    {
        PathNode.SelectionModes mode = select ? PathNode.SelectionModes.Select : PathNode.SelectionModes.Move;
        SetSelectionMode(mode);
    }

    public void EndTrack()
    {
        layingTrack = false;
    }
    public void GenerateFootprints()
    {
        if (path.nodes.Count == 0)
        {
            return;
        }
        foreach (Footprint footprint in footprints)
        {
            Destroy(footprint.gameObject);
        }
        footprints.Clear();
        float strideLength;
        int side = -1;
        for (float distAlong = 0; distAlong < path.bakedLength; distAlong += strideLength)
        {
            int segment = path.GetSegmentAlong(distAlong);
            StrideLengthPair strideLengthPair = strideLengthPairs[Mathf.Min(segment + 1, strideLengthPairs.Count - 1)];
            strideLength = footprints.Count % 2 == 0 ? strideLengthPair.left : strideLengthPair.right;

            Pose pose = path.GetPoseAlong(distAlong);
            Vector3 footprintPos = pose.position + pose.right * side * strideWidth * 0.5f;
            GameObject footprintObj = Instantiate(side == 1 ? rightFootprint : leftFootprint);
            footprintObj.transform.position = footprintPos;
            Footprint footprint = new Footprint
            {
                gameObject = footprintObj,
                distAlong = distAlong
            };
            footprints.Add(footprint);
            side *= -1;
        }
        onFootprintsUpdated.Invoke();
    }

    public void SavePath()
    {
        anchorStoreManager.AnchorStore.Clear();
        PlayerPrefs.DeleteAll();
        for (int i = 0; i < path.nodes.Count; i++)
        {
            AnchorableObject anchorable = path.nodes[i].GetComponent<AnchorableObject>();
            anchorable._worldAnchorName = $"path_node_{i}";
            anchorable.SaveAnchor();

            PlayerPrefs.SetFloat($"left_stride_length_{i}", strideLengthPairs[i].left);
            PlayerPrefs.SetFloat($"right_stride_length_{i}", strideLengthPairs[i].right);
        }
    }

    private class NameSorter : IComparer<string>
    {
        int GetIntFromString(string str)
        {
            string num = "";
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (char.IsDigit(c))
                {
                    num += c;
                }
            }
            if (num.Length > 0)
            {
                return int.Parse(num);
            }
            else
            {
                return -1;
            }
        }
        int IComparer<string>.Compare(string a, string b)
        {
            return GetIntFromString(a).CompareTo(GetIntFromString(b));
        }
    }

    public void LoadPath()
    {
        print("Loading Path");
        ClearTrack();
        List<string> sortedNames = new List<string>();
        foreach (string name in anchorStoreManager.AnchorStore.PersistedAnchorNames)
        {
            sortedNames.Add(name);
        }
        if (sortedNames.Count == 0) return;
        sortedNames.Sort(new NameSorter());
        foreach (string name in sortedNames)
        {
            if (name.StartsWith("path_node"))
            {
                Transform node = PlaceNode(Vector3.zero);
                AnchorableObject anchorable = node.GetComponent<AnchorableObject>();
                anchorable._worldAnchorName = name;
                anchorable.LoadAnchor();
            }
        }
        strideLengthPairs.Clear();
        for (int i = 0; i < path.nodes.Count; i++)
        {
            StrideLengthPair strideLengthPair = new StrideLengthPair();
            strideLengthPair.left = PlayerPrefs.GetFloat($"left_stride_length_{i}");
            strideLengthPair.right = PlayerPrefs.GetFloat($"right_stride_length_{i}");
            strideLengthPairs.Add(strideLengthPair);
        }
        UpdatePath();
        SetPathVisible(true);
        HighlightKeyNodes();
    }

    Transform PlaceNode(Vector3 pos)
    {
        if (strideLengthPairs.Count > 0)
        {
            strideLengthPairs[strideLengthPairs.Count - 1] = currentStrideLengths;
        }
        strideLengthPairs.Add(currentStrideLengths);
        Transform node = path.AddNode(pos);
        node.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UpdatePath);
        node.GetComponent<PathNode>().OnSelected.AddListener(SelectNode);
        node.GetComponent<PathNode>().SelectionMode = selectionMode;
        onNodePlaced.Invoke();
        return node;
    }

    void UpdatePath(ManipulationEventData eventData)
    {
        UpdatePath();
    }

    void UpdatePath()
    {
        path.UpdateCurves();
        if (path.nodes.Count > 1)
        {
            line.positionCount = 0;
            foreach (Pose pose in path.bakedPoses)
            {
                line.positionCount += 1;
                line.SetPosition(line.positionCount - 1, pose.position + Vector3.up * 0.1f);
            }
        }
        GenerateFootprints();
        HighlightKeyNodes();
        //GetComponent<AudioSource>().Play();
    }

    public void SetPathVisible(bool isVisible)
    {
        pathVisible = isVisible;
        //line.enabled = isVisible;
        Color color = line.material.color;
        color.a = isVisible ? 1f : 0.2f;
        line.material.color = color;
        foreach (Transform node in path.nodes)
        {
            node.gameObject.SetActive(isVisible);
        }
    }

    public void TogglePath()
    {
        SetPathVisible(!pathVisible);
    }

    public void ChangePathDirection()
    {
        direction *= -1;
        for (int i =0; i < strideLengthPairs.Count; i++)
        {
            float oldLeft = strideLengthPairs[i].left;
            float oldRight = strideLengthPairs[i].right;
            StrideLengthPair strideLengthPair = new StrideLengthPair(oldRight, oldLeft);
            strideLengthPairs[i] = strideLengthPair;
        }
        GenerateFootprints();
        foreach (GameObject indicator in directionIndicators)
            Destroy(indicator);
        directionIndicators.Clear();
        for(float d = 0; d < path.bakedLength; d += 0.5f)
        {
            GameObject indicator = Instantiate(directionIndicatorPrefab);
            directionIndicators.Add(indicator);
            indicator.GetComponent<PathFollower>().path = path;
            indicator.GetComponent<PathFollower>().distAlong = d;
            indicator.GetComponent<PathFollower>().travelSpeed *= direction;
        }
        StopAllCoroutines();
        StartCoroutine(fadeIndicators(1f));
    }

    IEnumerator fadeIndicators(float fadeTime)
    {
        for(float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            foreach(GameObject indicator in directionIndicators)
            {
                float brightness = 1 - (t / fadeTime);
                indicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(brightness, brightness, brightness);
            }
            yield return null;
        }
        foreach (GameObject indicator in directionIndicators)
            Destroy(indicator);
        directionIndicators.Clear();
    }
}
