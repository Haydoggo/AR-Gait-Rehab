using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class PinchSliderWrapper : MonoBehaviour
{
    public Utility.FloatEvent OnValueUpdated = new Utility.FloatEvent();
    public string title;
    public float minVal = 0f;
    public float maxVal = 1f;
    public float startingValue = 0.5f;
    public float exponent = 1f;
    public bool rounded = false;
    public float roundTo = 1f;

    private float lastVal = 0f;
    private PinchSlider pinchSlider;
    private GameObject label;
    private bool initialised = false;

    void Awake()
    {
        pinchSlider = GetComponent<PinchSlider>();
        pinchSlider.OnValueUpdated.AddListener(OnPinchSliderUpdate);
        label = transform.Find("Label").gameObject;
        if (label != null)
        {
            OnValueUpdated.AddListener(label.GetComponent<Label>().UpdateText);
            label.GetComponent<Label>().title = title;
        }
    }


    void OnPinchSliderUpdate(SliderEventData eventData)
    {
        float newVal = eventData.NewValue * (maxVal - minVal) + minVal;
        //Intercept the first call from Pinch slider and overide its starting value
        if (!initialised)
        {
            initialised = true;
            SetValue(startingValue);
            return;
        }
        float newValRaised = Mathf.Pow(Mathf.Abs(newVal), exponent);
        newValRaised = newValRaised * Mathf.Sign(newVal);
        if (rounded)
        {
            newValRaised = Mathf.Round(newValRaised/roundTo)*roundTo;
            if (newValRaised != lastVal)
            {
                OnValueUpdated.Invoke(newValRaised);
            }
        }
        else
        {
            OnValueUpdated.Invoke(newValRaised);
        }
        lastVal = newValRaised;
    }


    public void SetValue(float value)
    {
        float valNormalised = (value - minVal) / (maxVal - minVal);
        valNormalised = Mathf.Pow(Mathf.Abs(valNormalised), 1 / exponent);
        valNormalised *= Mathf.Sign(value);
        pinchSlider.SliderValue = valNormalised;
    }
}
