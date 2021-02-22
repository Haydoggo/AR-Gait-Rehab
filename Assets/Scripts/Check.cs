using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check : MonoBehaviour
{
    public Color startColor = Color.green;
    public Color endColor = Color.clear;
    public float fadeTime = 2f;

    private GameObject icon;
    public List<string> registeredIDs = new List<string>();
    void OnEnable()
    {
        icon = transform.GetChild(0).gameObject;
        icon.GetComponent<MeshRenderer>().enabled = false;
        icon.GetComponent<MeshRenderer>().material.color = startColor;
    }

    public void Trigger(bool disableOnEnd)
    {
        if (!gameObject.activeSelf) return;
        GetComponent<ParticleSystem>().Play();
        icon.GetComponent<MeshRenderer>().enabled = true;
        StopAllCoroutines();
        StartCoroutine(FadeAndRise(disableOnEnd));
    }

    // Triggers from a source that supplies a unique ID.
    // Will trigger only once per source
    public void TriggerOneOff(string ID)
    {
        if (!registeredIDs.Contains(ID))
        {
            registeredIDs.Add(ID);
            Trigger(false);
        }
    }
    IEnumerator FadeAndRise(bool disableOnEnd = false)
    {
        icon.transform.localPosition = Vector3.zero;
        GetComponent<AudioSource>().Play();
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            icon.GetComponent<MeshRenderer>().material.color = Color.Lerp(startColor, endColor, t / fadeTime);
            yield return null;
            icon.transform.localPosition += Vector3.up * Time.deltaTime;
        }
        icon.GetComponent<MeshRenderer>().enabled = false;
        if (disableOnEnd)
            gameObject.SetActive(false);
    }
}
