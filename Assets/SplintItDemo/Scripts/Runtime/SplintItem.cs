using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SplintIt
{
    /// <summary>One grabbable everyday object. Colliders live on child meshes; the root holds the Rigidbody,
    /// XRGrabInteractable and this component.</summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SplintItem : MonoBehaviour
    {
        [Header("Identity")]
        public string displayName = "Item";
        public ItemRole role = ItemRole.Distractor;
        [TextArea] public string whyGood = "";
        [TextArea] public string whyNotSuitable = "";

        [Tooltip("Local axis running along the object's length (used for orientation checks).")]
        public Vector3 lengthAxis = Vector3.forward;
        public float length = 0.4f;

        [Header("Visuals")]
        public GameObject hintMarker;       // floating arrow, shown by Guided mode
        public GameObject wrappedVisual;    // straps only: band around the arm
        public Renderer[] looseRenderers;   // hidden while wrapped

        public ItemState State { get; set; } = ItemState.Available;
        public PlacementZone CurrentZone { get; set; }
        public int PlacementToken { get; private set; }
        public bool IsHeld { get; private set; }

        public System.Action<SplintItem> Released;
        public System.Action<SplintItem> PickedUp;

        Rigidbody rb;
        XRGrabInteractable grab;
        Vector3 startPos;
        Quaternion startRot;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            grab = GetComponent<XRGrabInteractable>();
            startPos = transform.position;
            startRot = transform.rotation;

            grab.throwOnDetach = false;   // placing should never fling the object
            grab.selectEntered.AddListener(_ => OnGrab());
            grab.selectExited.AddListener(_ => OnRelease());

            SetHint(false);
            if (wrappedVisual != null) wrappedVisual.SetActive(false);
        }

        void OnGrab()
        {
            IsHeld = true;
            PlacementToken++;                         // cancels any wrapping in progress
            if (CurrentZone != null) CurrentZone.Remove(this);
            SetWrapped(false);
            State = ItemState.Grabbed;
            SetHint(false);
            PickedUp?.Invoke(this);
        }

        void OnRelease()
        {
            IsHeld = false;
            Released?.Invoke(this);
        }

        public void SetInteractable(bool on) { if (grab != null) grab.enabled = on; }

        public void SetKinematic(bool kinematic)
        {
            if (rb == null) return;
            rb.isKinematic = kinematic;
            if (!kinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        }

        public void Stabilize()
        {
            if (rb != null && !rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        }

        public void SnapTo(Vector3 position, Quaternion rotation)
        {
            SetKinematic(true);
            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>Rotates so the long axis matches worldDir (either direction counts).</summary>
        public void AlignLengthTo(Vector3 worldDir)
        {
            Vector3 current = transform.TransformDirection(lengthAxis);
            if (Vector3.Dot(current, worldDir) < 0f) worldDir = -worldDir;
            transform.rotation = Quaternion.FromToRotation(current, worldDir) * transform.rotation;
        }

        public void ReturnTo(Vector3 position)
        {
            PlacementToken++;
            if (CurrentZone != null) CurrentZone.Remove(this);
            SetWrapped(false);
            transform.position = position;
            SetKinematic(false);
            State = ItemState.Available;
        }

        public void ResetToStart()
        {
            PlacementToken++;
            if (CurrentZone != null) CurrentZone.Remove(this);
            SetWrapped(false);
            SetKinematic(true);
            transform.SetPositionAndRotation(startPos, startRot);
            SetKinematic(false);
            State = ItemState.Available;
            SetHint(false);
        }

        public void SetWrapped(bool on)
        {
            if (wrappedVisual == null) return;
            wrappedVisual.SetActive(on);
            if (looseRenderers != null) foreach (var r in looseRenderers) if (r != null) r.enabled = !on;
        }

        public void SetHint(bool on) { if (hintMarker != null) hintMarker.SetActive(on); }
    }
}
