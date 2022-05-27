using TMPro;
using UnityEngine;

public class Label : MonoBehaviour
{
    public string title = "Value";
    public string format = "F2";
    public string units = "";
    public void UpdateText(float value)
    {
        GetComponent<TextMeshPro>().text = string.Format("{0}: {1:" + format + "}", title, value);
    }

    public void UpdateText(int value)
    {
        GetComponent<TextMeshPro>().text = string.Format("{0}: {1:" + format + "}", title, value);
    }

    public void UpdateTextWithoutTitle(string text)
    {
        GetComponent<TextMeshPro>().text = text;
    }
    public void UpdateText(string text)
    {
        GetComponent<TextMeshPro>().text = $"{title}: {text}{units}";
    }
}
