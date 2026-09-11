using UnityEngine;

namespace SwingShift
{
    public enum CableState { Unattached, Attached, Released, Broken }

    /// --- summary
    /// Owns the physical cable between the hook and the beam.
    
    /// Responsibilities: attach, release, permitted length. It does not decide game outcomes.
 
    [RequireComponent(typeof(Rigidbody))]
    public class CableController : MonoBehaviour
    {
        [Header("Scene references")]
        [Tooltip("The body the cable hangs from (the lab support now, the trolley later). Kinematic or dynamic.")]
        [SerializeField] private Rigidbody support;

        [Tooltip("Point on the support where the cable leaves the hook.")]
        [SerializeField] private Transform hookPoint;

        [Tooltip("Point on this beam where the cable is fixed (top-centre).")]
        [SerializeField] private Transform cableAnchor;

        [Tooltip("Optional line used only to draw the cable. It never affects physics.")]
        [SerializeField] private LineRenderer cableLine;

        [Header("Cable limits")]
        [Tooltip("Shortest permitted cable length in metres.")]
        [SerializeField] private float minLength = 4f;

        [Tooltip("Longest permitted cable length in metres.")]
        [SerializeField] private float maxLength = 10f;

        [Tooltip("Cable break rating in newtons. GDD: 10,000 N. The joint breaks when its constraint force exceeds this in a physics step.")]
        [SerializeField] private float breakForceN = 10000f;

        [Header("Lab options")]
        [Tooltip("Attach automatically when the scene starts (used in PhysicsLab).")]
        [SerializeField] private bool attachOnStart = true;

        private ConfigurableJoint joint;

        /// True while a joint exists between the beam and the support.
        public bool IsAttached => joint != null;

        /// Unattached, Attached, Released, or Broken. Only this component changes it.
        public CableState State { get; private set; } = CableState.Unattached;

        /// Force the cable constraint applied in the last physics step, in newtons. Zero when slack or detached.
        public float TensionN { get; private set; }

        /// Highest tension seen since the last attach, in newtons.
        public float PeakTensionN { get; private set; }

        /// True when attached but the anchors are closer than the permitted length (the cable carries no load).
        public bool IsSlack => IsAttached && ActualSeparation < PermittedLength - 0.02f;

        /// Cable break rating in newtons.
        public float BreakForceN => breakForceN;

        /// Raised once when the joint breaks under load.
        public event System.Action Broke;

        /// The permitted cable length in metres (the joint's distance limit).
        public float PermittedLength { get; private set; }

        /// The actual straight-line distance between the anchors right now, in metres.
        public float ActualSeparation =>
            hookPoint != null && cableAnchor != null
                ? Vector3.Distance(hookPoint.position, cableAnchor.position)
                : 0f;

        private void Start()
        {
            if (attachOnStart)
            {
                Attach();
            }
        }

        
        /// Connects the beam to the support. The permitted length is initialised from the current
        /// separation of the two anchors, so the joint starts satisfied and applies no impulse.
        /// Returns false if the separation is outside the allowed cable range.
 
        public bool Attach()
        {
            if (IsAttached) return true;
            if (support == null || hookPoint == null || cableAnchor == null)
            {
                Debug.LogError("[Cable] Missing support, hook point, or cable anchor reference.", this);
                return false;
            }

            float separation = ActualSeparation;
            if (separation < minLength - 0.01f || separation > maxLength + 0.01f)
            {
                Debug.LogWarning($"[Cable] Cannot attach: separation {separation:F2} m is outside {minLength}-{maxLength} m.", this);
                return false;
            }

            joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = support;

            // Anchors are given in each body's own local space. Auto-configure is OFF on purpose:
            // with it on, Unity would centre the distance limit on the beam's starting position
            // instead of on the hook, which is the wrong geometry for a cable.

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = transform.InverseTransformPoint(cableAnchor.position);
            joint.connectedAnchor = support.transform.InverseTransformPoint(hookPoint.position);

            // Distance limit on all three linear axes, no rotational constraint.
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            // Hard limit: spring 0 means the solver treats the boundary as a rigid constraint.
            joint.linearLimitSpring = new SoftJointLimitSpring { spring = 0f, damper = 0f };

            // Beam and support never collide through the joint; the cable, not contact, couples them.
            joint.enableCollision = false;

            // Failure comes from the same constraint that produces the tension reading.
            // Torque is unlimited: the angular axes are free, so the joint never carries torque.
            joint.breakForce = breakForceN;
            joint.breakTorque = Mathf.Infinity;

            SetPermittedLength(Mathf.Clamp(separation, minLength, maxLength));
            State = CableState.Attached;
            TensionN = 0f;
            PeakTensionN = 0f;
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsAttached)
            {
                TensionN = 0f;
                return;
            }

            // currentForce is the constraint force from the previous step. Its magnitude is the
            // same quantity Unity compares against breakForce, so gauge and failure agree by construction.
            TensionN = joint.currentForce.magnitude;
            if (TensionN > PeakTensionN) PeakTensionN = TensionN;
        }

        
        /// Removes the joint. The beam keeps whatever velocity it had; nothing is re-posed or zeroed.
        
        public void Release()
        {
            if (!IsAttached) return;
            Destroy(joint);
            joint = null;
            State = CableState.Released;
            TensionN = 0f;
        }

        /// Sets the distance limit directly, clamped to the cable range.
        public void SetPermittedLength(float metres)
        {
            PermittedLength = Mathf.Clamp(metres, minLength, maxLength);
            if (!IsAttached) return;

            var limit = joint.linearLimit;
            limit.limit = PermittedLength;
            limit.bounciness = 0f;
            limit.contactDistance = 0f;
            joint.linearLimit = limit;

            // A beam resting on the floor may be asleep; a changing limit must be able to lift it.
            GetComponent<Rigidbody>().WakeUp();
        }

        private void LateUpdate()
        {
            if (cableLine == null) return;

            bool visible = IsAttached && hookPoint != null && cableAnchor != null;
            cableLine.enabled = visible;
            if (!visible) return;

            cableLine.positionCount = 2;
            cableLine.SetPosition(0, hookPoint.position);
            cableLine.SetPosition(1, cableAnchor.position);
        }

        private void OnJointBreak(float breakForce)
        {
            // Unity destroys the joint itself; drop our reference so IsAttached becomes false.
            joint = null;
            State = CableState.Broken;
            if (breakForce > PeakTensionN) PeakTensionN = breakForce;
            TensionN = 0f;
            Debug.Log($"[Cable] Cable broke at {breakForce:F0} N (rating {breakForceN:F0} N).", this);
            Broke?.Invoke();
        }
    }
}
