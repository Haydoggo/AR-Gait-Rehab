using System.Collections;
using UnityEngine;
public class Hand : MonoBehaviour
{
    Vector3 menuPosition;
    Vector3 offsetMenuPosition;
    private void OnEnable()
    {
        menuPosition = transform.parent.localPosition;
        offsetMenuPosition = menuPosition;
        offsetMenuPosition.x -= transform.localPosition.x;
        transform.parent.localPosition = offsetMenuPosition;
    }
    public void ShrinkAway()
    {
        if (gameObject.activeSelf)
            StartCoroutine(ShrinkRoutine(0.5f));
    }

    IEnumerator ShrinkRoutine(float shrinkTime)
    {
        for (float t = 0; t < shrinkTime; t += Time.deltaTime)
        {
            float T = Mathf.Sin(0.5f * (t / shrinkTime) * Mathf.PI);
            transform.localScale = Vector3.one * (1 - T);
            transform.parent.localPosition = Vector3.Lerp(offsetMenuPosition, menuPosition, T);
            yield return null;
        }
        transform.parent.localPosition = menuPosition;
        gameObject.SetActive(false);
    }
}