using UnityEngine;

namespace SwingShift
{
    /// Shows where the load would land if it went straight down: a flat footprint on whatever surface
    /// is below it. Presentation only. It reads the scene with a raycast and never changes physics.
    public class DropMarker : MonoBehaviour
    {
        [SerializeField] private CableController cable;

        [Tooltip("A flat quad sized like the container footprint, with its collider removed.")]
        [SerializeField] private Transform marker;

        [Tooltip("Which layers count as surfaces to land on.")]
        [SerializeField] private LayerMask surfaceMask = ~0;

        [Tooltip("Lift above the surface in metres so the quad does not z-fight with it.")]
        [SerializeField] private float surfaceOffset = 0.03f;

        private readonly RaycastHit[] hits = new RaycastHit[16];

        private void LateUpdate()
        {
            if (cable == null || marker == null) return;

            CableLoad load = cable.Load;
            Vector3 origin = load != null ? load.Body.worldCenterOfMass : cable.SpreaderPosition;
            float yaw = load != null ? load.transform.eulerAngles.y : cable.transform.eulerAngles.y;

            // Nearest surface straight down that is not the carried load itself.
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, hits, 200f, surfaceMask, QueryTriggerInteraction.Ignore);
            float nearest = float.MaxValue;
            Vector3 point = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                if (load != null && hits[i].rigidbody == load.Body) continue;
                if (hits[i].distance < nearest)
                {
                    nearest = hits[i].distance;
                    point = hits[i].point;
                }
            }

            bool found = nearest < float.MaxValue;
            marker.gameObject.SetActive(found);
            if (!found) return;

            // A quad faces along its local -Z; pitching it 90 degrees lays it flat, facing up.
            marker.SetPositionAndRotation(point + Vector3.up * surfaceOffset, Quaternion.Euler(90f, yaw, 0f));
        }
    }
}
