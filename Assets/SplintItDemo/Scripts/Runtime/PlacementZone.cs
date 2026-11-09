using UnityEngine;

namespace SplintIt
{
    /// <summary>A spot where items can be placed. Checked on release by distance (no trigger physics needed).
    /// Arm zones hold one item; the Prep zone accepts any number.</summary>
    public class PlacementZone : MonoBehaviour
    {
        public ZoneKind kind;
        public float radius = 0.22f;
        [Tooltip("Support zone only: max angle between the item's length and the arm.")]
        public float maxAngle = 35f;
        public GameObject ghost;     // outline shown by guidance

        public SplintItem Occupant { get; private set; }

        public bool Accepts(SplintItem item)
        {
            switch (kind)
            {
                case ZoneKind.Prep:    return true;
                case ZoneKind.Support: return item.role == ItemRole.RigidSupport;
                case ZoneKind.Padding: return item.role == ItemRole.Padding;
                default:               return item.role == ItemRole.Strap;
            }
        }

        public bool Contains(Vector3 worldPoint)
        {
            return (worldPoint - transform.position).sqrMagnitude <= radius * radius;
        }

        public bool OrientationOk(SplintItem item)
        {
            if (kind != ZoneKind.Support) return true;
            Vector3 a = item.transform.TransformDirection(item.lengthAxis);
            float angle = Vector3.Angle(a, transform.forward);
            return angle <= maxAngle || (180f - angle) <= maxAngle;
        }

        public void Occupy(SplintItem item) { Occupant = item; item.CurrentZone = this; }

        public void Remove(SplintItem item)
        {
            if (Occupant == item) Occupant = null;
            if (item.CurrentZone == this) item.CurrentZone = null;
        }

        public void ShowGhost(bool on) { if (ghost != null) ghost.SetActive(on); }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
