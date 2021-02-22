using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour
{
    // properties defining graph appearance
    public GameObject barPrefab;
    public float yScale = 0.5f;
    public Vector2 size = new Vector2(300, 300);
    public int resolution = 100;
    public string desc = "";
    public bool barGraphMode = false;

    // Smoothing coeficient for SmoothedValue. Uses Exponential smoothing.
    [Range(0, 1)]
    public float alpha = 0f;

    // Ring buffers storing the values and bar game objects
    public List<GameObject> bars = new List<GameObject>();
    public List<float> values = new List<float>();

    private float barWidth;
    private int lastBarIndex = 0;
    private float lastVal = 0f;
    public TextMeshPro label;
    private FloatSmoothed smoothedValue = new FloatSmoothed();
    public void Awake()
    {
        smoothedValue.Alpha = alpha;
        barWidth = size.x / resolution;
        for (int i = 0; i < resolution; i++)
        {
            values.Add(0f);
            GameObject bar = Instantiate(barPrefab);
            bars.Add(bar);
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.position = new Vector2(barWidth * i, 0);
            rect.sizeDelta = new Vector2(barWidth, 0);
            bar.transform.SetParent(transform, false);
        }
    }

    public void Update()
    {
        if (smoothedValue != null)
        {
            smoothedValue.Alpha = alpha;
        }
    }

    // Write new value to data ring buffer
    public float WriteValue(float val)
    {
        // Add new value to ring buffer
        smoothedValue.Value = val;
        float delta;
        float y;
        if (barGraphMode)
        {
            delta = Mathf.Abs(smoothedValue.Value);
            y = Mathf.Min(0, smoothedValue.Value);
        }
        else
        {
            delta = smoothedValue.Value - lastVal;
            y = (delta >= 0) ? lastVal : smoothedValue.Value;
        }
        values[lastBarIndex] = smoothedValue.Value;

        GameObject bar = bars[lastBarIndex];
        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barWidth, Mathf.Abs(delta) * size.y * yScale + barWidth);
        rect.localPosition = new Vector2(barWidth * lastBarIndex, y * size.y * yScale);


        float _maxVal = val;
        float _minVal = val;
        float aveVal = 0;
        foreach (float value in values)
        {
            aveVal += value;
            _maxVal = Mathf.Max(value, _maxVal);
            _minVal = Mathf.Min(value, _minVal);
        }
        aveVal /= values.Count;
        label.text = $"{desc}\nMax: {_maxVal:F3}\nMin: {_minVal:F3}\nave: {aveVal:F3}";

        lastBarIndex++;
        if (lastBarIndex >= resolution)
        {
            lastBarIndex = 0;
        }

        lastVal = smoothedValue.Value;

        return smoothedValue.Value;
    }

    public void SetBarMaterial(Material material)
    {
        foreach (GameObject bar in bars)
        {
            bar.GetComponent<Image>().material = material;
        }
    }
}