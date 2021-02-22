using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PathTest
{
    public class PathTester : MonoBehaviour
    {
        Transform bigArrow;
        enum Mode
        {
            NodePath,
            BakedPath,
            Curves,
            BakedPoints,
            ForwardVectors,
            BakedForwardVectors
        }
        public Slider slider;
        public bool Playing { get; set; } = false;
        Mode drawMode = Mode.BakedForwardVectors;
        public Path path;
        public GameObject dotPrefab;
        public Sprite arrowSprite;
        List<GameObject> dots = new List<GameObject>();
        private LineRenderer line;
        public Material armMat;
        public bool ShowPath { get; set; } = true;
        private void Start()
        {
            slider = FindObjectOfType<Slider>();
            line = GetComponent<LineRenderer>();
            path = GetComponent<Path>();
            bigArrow = Instantiate(dotPrefab).transform;
            bigArrow.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = arrowSprite;
            bigArrow.localScale = Vector3.one * 5;
        }

        public void ClearPath()
        {
            line.positionCount = 0;
            foreach (GameObject thisDot in dots)
            {
                Destroy(thisDot);
            }
            dots.Clear();
            path.Clear();
        }

        public void ChangeMode(int mode)
        {
            drawMode = (Mode)mode;
            DrawDots();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                Transform node = path.AddNode(pos);
                node.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
                path.UpdateCurves();
                DrawDots();
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                DrawDots();
            }
            if (Playing && path.nodes.Count > 0)
            {
                slider.value += 0.0005f;
                slider.value %= slider.maxValue;
            }
        }

        void DrawDots()
        {
            line.positionCount = 0;
            foreach (GameObject thisDot in dots)
            {
                Destroy(thisDot);
            }
            dots.Clear();
            switch (drawMode)
            {
                case Mode.NodePath:
                    for (float d = 0; d < path.NodePathLength; d += path.bakeResolution)
                    {
                        PlaceMarker(path.GetPointAlongNodePath(d), Color.white, 0.5f, false);
                    }
                    break;
                case Mode.BakedPoints:
                    foreach (Pose pose in path.bakedPoses)
                    {
                        PlaceMarker(pose.position, Color.white, 1f, false);
                    }
                    break;
                case Mode.BakedPath:
                    for (float d = 0; d < path.bakedLength; d += 100)
                    {
                        PlaceMarker(path.GetPointAlong(d), Color.white, 1f, false);
                    }
                    break;
                case Mode.Curves:
                    foreach (QuadCurve curve in path.curves)
                    {
                        for (float t = 0; t < 1; t += 0.1f)
                        {
                            GameObject arrow = PlaceMarker(curve.GetPointAlong(t), Color.white, 1f);
                            arrow.transform.right = curve.GetDerivativeAlong(t);
                        }
                    }
                    break;
                case Mode.ForwardVectors:
                    for (float d = 0; d < path.NodePathLength; d += path.bakeResolution)
                    {
                        GameObject arrow = PlaceMarker(path.GetPointAlongNodePath(d), Color.white, 1f);
                        arrow.transform.right = path.GetDerivativeAlongNodePath(d);
                    }
                    break;
                case Mode.BakedForwardVectors:
                    foreach (Pose pose in path.bakedPoses)
                    {
                        GameObject arrow = PlaceMarker(pose.position, Color.white, 1f);
                        arrow.transform.right = pose.forward;
                    }
                    break;
            }
            if (ShowPath)
            {
                //for(float d = 0; d < path.NodePathLength; d += 1)
                //{
                //    line.positionCount++;
                //    line.SetPosition(line.positionCount - 1, path.GetPointAlongNodePath(d));
                //}
                foreach (Pose pose in path.bakedPoses)
                {
                    line.positionCount++;
                    line.SetPosition(line.positionCount - 1, pose.position);
                }
                foreach (QuadCurve curve in path.curves)
                {
                    GameObject dot;
                    dot = PlaceMarker(curve.P0, Color.green, 0.5f, false);
                    dot.transform.localScale = new Vector3(.4f, .4f, .4f);
                    dot = PlaceMarker(curve.P1, Color.yellow, 0.5f, false);
                    dot.transform.localScale = new Vector3(.4f, .4f, .4f);
                }
            }
        }

        public void MoveDot(float d)
        {
            if (path.nodes.Count == 0) return;
            switch (drawMode)
            {
                case Mode.NodePath:
                    bigArrow.position = path.GetPointAlongNodePath(d * path.NodePathLength);
                    bigArrow.right = path.GetDerivativeAlongNodePath(d * path.NodePathLength);
                    break;
                case Mode.BakedPath:
                    bigArrow.position = path.GetPointAlong(d * path.bakedLength);
                    bigArrow.right = path.GetDerivativeAlong(d * path.bakedLength, true);
                    break;
                case Mode.BakedPoints:
                    bigArrow.position = path.GetPointAlong(d * path.bakedLength);
                    bigArrow.right = path.GetDerivativeAlong(d * path.bakedLength, true);
                    break;
                case Mode.ForwardVectors:
                    bigArrow.position = path.GetPointAlongNodePath(d * path.NodePathLength);
                    bigArrow.right = path.GetDerivativeAlongNodePath(d * path.NodePathLength);
                    break;
                case Mode.BakedForwardVectors:
                    bigArrow.position = path.GetPointAlong(d * path.bakedLength);
                    bigArrow.right = path.GetDerivativeAlong(d * path.bakedLength, true);
                    break;
                case Mode.Curves:
                    float t = (d * (path.curves.Count - 1)) % 1f;
                    QuadCurve curve = path.GetCurveAlong(d * path.NodePathLength);
                    bigArrow.position = curve.GetPointAlong(t);
                    bigArrow.right = curve.GetDerivativeAlong(t);
                    break;
            }
        }

        public GameObject PlaceMarker(Vector3 position, Color color, float alpha = 0.1f, bool arrow = true)
        {
            color.a = alpha;
            GameObject dot = Instantiate(dotPrefab);
            dot.transform.position = position;
            dot.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
            if (arrow) dot.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = arrowSprite;
            else dot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            dots.Add(dot);
            return dot;
        }
    }
}