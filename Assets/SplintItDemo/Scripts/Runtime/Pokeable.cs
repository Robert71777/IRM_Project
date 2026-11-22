using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SplintIt
{
    /// <summary>Makes a physics body react to a poke/push from an XR interactor. Attach to any object that has a
    /// Rigidbody and a Collider (NPC limbs, loose props). When an interactor hovers/touches the body we push the
    /// Rigidbody away from the fingertip and fire light feedback.
    ///
    /// It drives whichever interactable is already on the object:
    ///  - NPC limbs get an XRSimpleInteractable (they are not grabbable), so poke/direct hovers push them.
    ///  - Grabbable items keep their XRGrabInteractable; a poke that is not a grab still nudges them, and the
    ///    push is skipped while the item is actually held so grabbing stays clean.
    ///
    /// Pinned/kinematic bodies simply absorb the poke (no movement), which is what the injured forearm wants
    /// while the splint is being built.</summary>
    public class Pokeable : MonoBehaviour
    {
        [Tooltip("Rigidbody that receives the push. Defaults to the one on this object or a parent.")]
        public Rigidbody body;

        [Header("Tuning")]
        [Tooltip("Impulse strength applied along the poke direction.")]
        public float pushStrength = 2.2f;
        [Tooltip("Minimum seconds between pokes, so holding a finger in does not machine-gun impulses.")]
        public float cooldown = 0.18f;
        [Tooltip("Play a soft tick + haptic when poked.")]
        public bool feedback = true;

        XRBaseInteractable interactable;
        float nextPokeTime;

        void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            if (body == null) body = GetComponentInParent<Rigidbody>();
            if (interactable == null) return;

            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDestroy()
        {
            if (interactable == null) return;
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            Poke(args.interactorObject);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            // On a grabbable item, select = pick-up: let the grab win, don't shove it out of the hand.
            if (interactable is XRGrabInteractable) return;
            // On a non-grabbable limb, a grip/trigger reads as a firmer push.
            Poke(args.interactorObject, 1.6f);
        }

        void Poke(IXRInteractor interactor, float scale = 1f)
        {
            // A controller ray can hover from metres away. Only physical touch should shove objects;
            // otherwise aiming at a supply knocks it away before the player can press grip.
            if (!(interactor is XRPokeInteractor) && !(interactor is XRDirectInteractor)) return;
            if (body == null || body.isKinematic) return;           // pinned limb absorbs it
            if (interactable != null && interactable.isSelected) return;   // held item: no shove
            if (Time.time < nextPokeTime) return;
            nextPokeTime = Time.time + cooldown;

            Transform from = interactor != null ? interactor.GetAttachTransform(interactable) : null;
            Vector3 dir = from != null
                ? (body.worldCenterOfMass - from.position)
                : -transform.forward;
            dir.y = Mathf.Max(dir.y, 0.05f);                        // push slightly up so it doesn't just grind down

            body.AddForceAtPosition(dir.normalized * pushStrength * scale, transform.position, ForceMode.Impulse);

            if (feedback)
            {
                Feedback.Play(Sfx.Tick);
                Feedback.Haptic(0.25f, 0.05f);
            }
        }
    }
}
