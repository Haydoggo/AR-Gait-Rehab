using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MRTK.Tutorials.AzureSpatialAnchors
{
    public class DebugWindow : MonoBehaviour
    {
        [Tooltip("If a debug message starts with a string in this list, it will be excluded")]
        public List<string> excludedStrings = new List<string>();

        [SerializeField] private TextMeshProUGUI debugText = default;

        private ScrollRect scrollRect;

        private void Awake()
        {
            // Cache references
            scrollRect = GetComponentInChildren<ScrollRect>();

            // Subscribe to log message events
            Application.logMessageReceived += HandleLog;

            // Set the starting text
            debugText.text = "Debug messages will appear here.\n\n";
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            foreach (string str in excludedStrings)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                if (message.StartsWith(str))
                    return;
            }
            debugText.text += message + " \n";
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
