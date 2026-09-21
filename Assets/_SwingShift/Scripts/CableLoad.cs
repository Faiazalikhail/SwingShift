using System.Collections.Generic;
using UnityEngine;

namespace SwingShift
{
    /// Marks a rigidbody as something the crane can lift and says where the ropes fix to it.
    /// It holds no physics of its own: mass, inertia, and collisions all come from the Rigidbody.
    [RequireComponent(typeof(Rigidbody))]
    public class CableLoad : MonoBehaviour
    {
        [Tooltip("Points on this load where ropes are fixed. Four top corners for a container; one top-centre point for a simple test load.")]
        [SerializeField] private Transform[] cableAnchors;

        private static readonly List<CableLoad> active = new List<CableLoad>();

        /// Every enabled load in the scene. The cable searches this list for one within reach of the spreader.
        public static IReadOnlyList<CableLoad> Active => active;

        public Rigidbody Body { get; private set; }

        public IReadOnlyList<Transform> Anchors => cableAnchors;

        /// Weight in newtons: mass times the magnitude of gravity.
        public float WeightN => Body.mass * Physics.gravity.magnitude;

        /// Mean position of the anchors: where the spreader has to be to pick this load up.
        public Vector3 AnchorCentre
        {
            get
            {
                if (cableAnchors == null || cableAnchors.Length == 0) return transform.position;
                Vector3 sum = Vector3.zero;
                for (int i = 0; i < cableAnchors.Length; i++) sum += cableAnchors[i].position;
                return sum / cableAnchors.Length;
            }
        }

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            WarnIfScaled(cableAnchors, this);
        }

        /// Marker transforms must sit under unscaled parents. A scaled parent multiplies the marker's
        /// local position, so the rope would fix to a point that is not where the Inspector says it is.
        public static void WarnIfScaled(Transform[] markers, Object context)
        {
            if (markers == null) return;
            for (int i = 0; i < markers.Length; i++)
            {
                Transform marker = markers[i];
                if (marker == null || marker.parent == null) continue;
                Vector3 scale = marker.parent.lossyScale;
                if (Vector3.Distance(scale, Vector3.one) > 0.001f)
                {
                    Debug.LogWarning($"[Cable] '{marker.name}' is a child of '{marker.parent.name}', which is scaled {scale}. Move it under an unscaled parent.", context);
                }
            }
        }

        private void OnEnable()
        {
            active.Add(this);
        }

        private void OnDisable()
        {
            active.Remove(this);
        }
    }
}
