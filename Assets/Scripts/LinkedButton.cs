using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class LinkedButton : MonoBehaviour
{
    public List<LinkedButton> linkedButtons = new List<LinkedButton>();
    void Start()
    {
        GetComponent<Interactable>().OnClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (GetComponent<Interactable>().IsToggled)
        {
            foreach(LinkedButton button in linkedButtons)
            {
                if (button.GetComponent<Interactable>().CurrentDimension == 1)
                {
                    button.GetComponent<PressableButtonHoloLens2>().ButtonPressed.Invoke();
                }
            }
        }
    }
}
