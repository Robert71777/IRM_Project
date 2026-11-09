using Unity.XR.CoreUtils;
using UnityEngine;

namespace SplintIt
{
    /// <summary>Seated/standing toggle and a manual height offset, applied to the XR Origin and stored
    /// locally through SaveSystem. Changed from the pause screen; DemoFlow calls ToggleSeated/AdjustHeight
    /// and shows Describe() on the button label.</summary>
    public class ComfortSettings : MonoBehaviour
    {
        [Tooltip("Extra floor height added in seated mode so a seated player reaches the arm and tables.")]
        public float seatedBoost = 0.35f;
        public float minOffset = -0.4f;
        public float maxOffset = 0.8f;

        XROrigin origin;

        // Baseline floor-offset height captured at startup. In Floor tracking mode the runtime supplies the
        // real standing height and this is ~0; in the editor / XR Device Simulator the scene builder seeds it
        // with CameraYOffset (~1.6 m) so the eyes start above the ground. Comfort offsets are applied ON TOP
        // of this baseline so toggling Seated/Height never drops the camera back under the map.
        float baseHeight;

        void Awake()
        {
            origin = FindFirstObjectByType<XROrigin>();
            if (origin != null)
            {
                var offsetObject = origin.CameraFloorOffsetObject != null
                    ? origin.CameraFloorOffsetObject.transform
                    : origin.transform;
                baseHeight = offsetObject.localPosition.y;
            }
            SaveSystem.Load();
            Apply();
        }

        public bool Seated
        {
            get => SaveSystem.Data.seated;
            private set => SaveSystem.Data.seated = value;
        }

        public float HeightOffset
        {
            get => SaveSystem.Data.heightOffset;
            private set => SaveSystem.Data.heightOffset = Mathf.Clamp(value, minOffset, maxOffset);
        }

        public void ToggleSeated()
        {
            Seated = !Seated;
            Apply();
            SaveSystem.SavePreferences();
        }

        public void AdjustHeight(float delta)
        {
            HeightOffset = HeightOffset + delta;
            Apply();
            SaveSystem.SavePreferences();
        }

        /// <summary>Button label text: current mode + offset in centimetres.</summary>
        public string Describe()
        {
            int cm = Mathf.RoundToInt(HeightOffset * 100f);
            string sign = cm >= 0 ? "+" : "";
            return (Seated ? "Seated" : "Standing") + "  (" + sign + cm + " cm)";
        }

        void Apply()
        {
            if (origin == null) return;
            float total = baseHeight + HeightOffset + (Seated ? seatedBoost : 0f);

            // Move the XR Origin's camera-floor offset object (the documented way to raise/lower the view).
            // Fall back to the origin transform if no dedicated offset object is set.
            var offsetObject = origin.CameraFloorOffsetObject != null
                ? origin.CameraFloorOffsetObject.transform
                : origin.transform;
            var p = offsetObject.localPosition;
            offsetObject.localPosition = new Vector3(p.x, total, p.z);
        }
    }
}
