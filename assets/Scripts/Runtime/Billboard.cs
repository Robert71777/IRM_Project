using UnityEngine;

namespace SplintIt
{
    /// <summary>Keeps a world-space canvas (speech bubble, injury badge) turned toward the player's camera,
    /// so diegetic UI stays readable from any angle. Added in code by HikingDemoSceneBuilder.</summary>
    [DefaultExecutionOrder(100)]
    public class Billboard : MonoBehaviour
    {
        [Tooltip("Keep the object upright (only yaw toward the camera) instead of fully facing it.")]
        public bool keepUpright = false;

        Transform cam;

        void LateUpdate()
        {
            if (cam == null)
            {
                var main = Camera.main;
                if (main == null) return;
                cam = main.transform;
            }

            Vector3 toCam = transform.position - cam.position;
            if (keepUpright) toCam.y = 0f;
            if (toCam.sqrMagnitude < 1e-6f) return;

            // text/UI faces the camera when its local +Z points away from the viewer
            transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }
    }
}
