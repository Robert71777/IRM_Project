using System;
using UnityEngine;
using UnityEngine.XR;

namespace SplintIt
{
    /// <summary>Detects headset removal / head-tracking loss / app focus loss so the flow can auto-pause.
    /// The round only resumes after tracking is back AND the player confirms.</summary>
    public class TrackingGuard : MonoBehaviour
    {
        public bool Present { get; private set; } = true;
        public event Action<bool> PresenceChanged;
        public event Action FocusLost;

        void Update()
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (!head.isValid) return;

            bool present = true;
            if (head.TryGetFeatureValue(CommonUsages.userPresence, out bool worn)) present &= worn;
            if (head.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked)) present &= tracked;

            if (present != Present)
            {
                Present = present;
                PresenceChanged?.Invoke(present);
            }
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus) FocusLost?.Invoke();
        }
    }
}
