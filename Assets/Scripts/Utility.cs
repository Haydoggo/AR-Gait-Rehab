using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;
namespace Utility
{
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    public class Utility : MonoBehaviour
    {

        public void ToggleItem(GameObject item)
        {
            item.SetActive(!item.activeSelf);
        }

        public void SimulatePress(PressableButtonHoloLens2 button)
        {
            button.ButtonPressed.Invoke();
        }

        public void ToggleButtonOn(PressableButtonHoloLens2 button)
        {
            if (button.GetComponent<Interactable>().CurrentDimension == 0)
            {
                SimulatePress(button);
            }
        }

        public void ToggleButtonOff(PressableButtonHoloLens2 button)
        {
            if (button.GetComponent<Interactable>().CurrentDimension == 1)
            {
                SimulatePress(button);
            }
        }

        public void DestroyObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }
    }

}