using UnityEngine;

public class FootprintFader : MonoBehaviour
{
    public float fadeStartDist = 3f;
    public float fadeEndDist = 5f;

    public PathController pathController;
    public PathTracker pathTracker;

    private Vector3 pathFollowerLastPosition = new Vector3();

    private void Start()
    {
        pathController.onFootprintsUpdated.AddListener(FadeFootprints);
    }
    private void Update()
    {
        if (pathTracker.transform.position != pathFollowerLastPosition)
        {
            FadeFootprints();
        }
        pathFollowerLastPosition = pathTracker.transform.position;
    }

    private void OnEnable()
    {
        FadeFootprints();
    }
    private void FadeFootprints()
    {
        foreach (PathController.Footprint footprint in pathController.footprints)
        {
            float distToFootprint = Mathf.Abs(pathTracker.distAlong - footprint.distAlong);
            float alpha = Mathf.InverseLerp(fadeEndDist, fadeStartDist, distToFootprint);
            alpha = Mathf.Clamp01(alpha);
            MeshRenderer meshRenderer = footprint.gameObject.GetComponent<MeshRenderer>();
            if (alpha == 0)
            {
                meshRenderer.enabled = false;
            }
            else
            {
                meshRenderer.enabled = true;
                Color color = meshRenderer.material.color;
                color.a = alpha;
                meshRenderer.material.color = color;
            }
        }
    }

    private void OnDisable()
    {
        foreach (PathController.Footprint footprint in pathController.footprints)
        {
            if (footprint.gameObject != null) {
                MeshRenderer meshRenderer = footprint.gameObject.GetComponent<MeshRenderer>();
                Color color = meshRenderer.material.color;
                meshRenderer.enabled = true;
                color.a = 1f;
                meshRenderer.material.color = color;
            }
        }
    }
}