using System.Collections.Generic;
using UnityEngine;

namespace SplintIt
{
    /// <summary>Turns the gray placeholder NPC into a simple physics ragdoll for the "friend collapses" cutscene.
    ///
    /// Design constraints (see SKILL.md):
    /// - The splint placement zones are parented to the injured forearm and the game snaps items to their fixed
    ///   positions. So the injured forearm must NOT move while the player is building the splint. During the
    ///   active round it is PINNED (kinematic). The rest of the body (torso, head, legs, good arm) stays soft
    ///   and reacts to pokes/pushes.
    /// - No stored gameplay flags live here; this only drives visuals/physics. SplintBuildController still
    ///   derives completion from zone occupancy.
    ///
    /// Lifecycle:
    ///   Build()            - wired by the scene builder: collects limb parts, adds Rigidbody + collider + joints,
    ///                        starts kinematic so the NPC stands in the authored pose.
    ///   StandUp()          - freeze everything back to the standing pose (menu / before the fall).
    ///   Collapse()         - release the body to physics so the friend falls; optional shove direction.
    ///   PinInjuredArm(on)  - lock/unlock just the injured forearm (and hand) so the splint stays put.
    /// </summary>
    public class RagdollController : MonoBehaviour
    {
        [System.Serializable]
        public class Part
        {
            public Transform transform;
            public Rigidbody body;
            public bool isInjuredArm;   // the one limb that gets pinned during splinting

            // authored standing pose, captured at Build() so StandUp() can restore it exactly
            public Vector3 localPos;
            public Quaternion localRot;
        }

        [Tooltip("Limbs driven by physics. Filled by HikingDemoSceneBuilder via AddPart.")]
        public List<Part> parts = new List<Part>();

        [Tooltip("Root the ragdoll parts hang under (the NPC visual root).")]
        public Transform visualRoot;

        [Header("Tuning")]
        [Tooltip("Velocity-change impulse applied to each limb on collapse, so the fall reads as a stumble.")]
        public float collapseShove = 1.6f;

        public bool Collapsed { get; private set; }

        /// <summary>Register one limb. Called by the scene builder for each primitive body part.</summary>
        public Part AddPart(Transform t, bool injuredArm, float mass)
        {
            if (t == null) return null;

            var rb = t.GetComponent<Rigidbody>();
            if (rb == null) rb = t.gameObject.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = true;   // stands still until Collapse()

            var p = new Part
            {
                transform = t,
                body = rb,
                isInjuredArm = injuredArm,
                localPos = t.localPosition,
                localRot = t.localRotation
            };
            parts.Add(p);
            return p;
        }

        /// <summary>Freeze every limb back to the authored standing pose.</summary>
        public void StandUp()
        {
            Collapsed = false;
            foreach (var p in parts)
            {
                if (p == null || p.transform == null) continue;
                if (p.body != null)
                {
                    p.body.isKinematic = true;
#if UNITY_6000_0_OR_NEWER
                    p.body.linearVelocity = Vector3.zero;
#else
                    p.body.velocity = Vector3.zero;
#endif
                    p.body.angularVelocity = Vector3.zero;
                }
                p.transform.localPosition = p.localPos;
                p.transform.localRotation = p.localRot;
            }
        }

        /// <summary>Release the body to physics so the friend falls. shoveDir is a world-space nudge.</summary>
        public void Collapse(Vector3 shoveDir)
        {
            Collapsed = true;
            foreach (var p in parts)
            {
                if (p == null || p.body == null) continue;
                p.body.isKinematic = false;
            }
            // a small shove so the fall reads as a stumble, not a straight drop
            foreach (var p in parts)
            {
                if (p == null || p.body == null || p.body.isKinematic) continue;
                p.body.AddForce(shoveDir.normalized * collapseShove, ForceMode.VelocityChange);
            }
        }

        /// <summary>Pin (true) or release (false) just the injured forearm + hand so the splint zones stay fixed
        /// while the rest of the body can still be poked.</summary>
        public void PinInjuredArm(bool pinned)
        {
            foreach (var p in parts)
            {
                if (p == null || p.body == null || !p.isInjuredArm) continue;
                p.body.isKinematic = pinned;
                if (pinned)
                {
#if UNITY_6000_0_OR_NEWER
                    p.body.linearVelocity = Vector3.zero;
#else
                    p.body.velocity = Vector3.zero;
#endif
                    p.body.angularVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>Re-stabilise soft limbs so nothing drifts while the round is paused / frozen.</summary>
        public void Stabilize()
        {
            foreach (var p in parts)
            {
                if (p == null || p.body == null || p.body.isKinematic) continue;
#if UNITY_6000_0_OR_NEWER
                p.body.linearVelocity = Vector3.zero;
#else
                p.body.velocity = Vector3.zero;
#endif
                p.body.angularVelocity = Vector3.zero;
            }
        }
    }
}
