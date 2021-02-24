using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SlingCup : MonoBehaviour, IMixedRealityPointerHandler
{
    public bool grabbed = false;
    [SerializeField]
    private Transform fingerTip;
    [SerializeField]
    private Transform restPosition;

    IEnumerator SpringBackRoutine(float returnTime)
    {
        Vector3 startPos = transform.position;
        for(float t = 0; t < returnTime; t += Time.deltaTime)
        {
            float T = 1 - Mathf.Cos(2 * Mathf.PI * t / returnTime) * (1 - t / returnTime);
            transform.position = Vector3.LerpUnclamped(startPos, restPosition.position, T);
            yield return null;
        }
        transform.position = restPosition.position;
    }

    #region IMixedRealityPointerHandler
    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        transform.position = Vector3.Lerp(fingerTip.position, restPosition.position, 0.5f);
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) { grabbed = true; }

    public void OnPointerUp(MixedRealityPointerEventData eventData) { 
        grabbed = false;
        StartCoroutine(SpringBackRoutine(0.2f));
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    #endregion
}
