using UnityEngine;

namespace SwingShift
{
    public enum CableState { Unattached, Attached, Released, Broken }

    /// Owns the ropes between the crane's rope head and whatever load is hooked.
    /// Lives on the rope head (a kinematic rigidbody). Each rope is one distance-limited joint from a
    /// point on the head to a point on the load, so four ropes to four corners hold a container level.
    /// Responsibilities: attach, release, permitted length, tension, overload. It does not decide game outcomes.
    [RequireComponent(typeof(Rigidbody))]
    public class CableController : MonoBehaviour
    {
        [Header("Scene references")]
        [Tooltip("Points on this rope head where each rope leaves. Four for a container rig, laid out like the container's corners.")]
        [SerializeField] private Transform[] ropeOrigins;

        [Tooltip("Optional spreader frame visual. Presentation only: it follows the rope ends and never affects physics.")]
        [SerializeField] private Transform spreaderVisual;

        [Tooltip("Optional lines used only to draw the ropes, one per rope origin. They never affect physics.")]
        [SerializeField] private LineRenderer[] ropeLines;

        [Header("Rope limits")]
        [Tooltip("Shortest permitted rope length in metres.")]
        [SerializeField] private float minLength = 3f;

        [Tooltip("Longest permitted rope length in metres.")]
        [SerializeField] private float maxLength = 22f;

        [Tooltip("Rope length paid out when the scene starts, in metres. Sets where the empty spreader begins.")]
        [SerializeField] private float startLength = 10f;

        [Tooltip("Rated load of the whole rig in newtons. When the net rope force on the load exceeds this in a physics step, every rope fails.")]
        [SerializeField] private float breakForceN = 100000f;

        [Tooltip("How close the spreader must be to a load's anchor centre to attach, in metres.")]
        [SerializeField] private float attachRange = 0.8f;

        [Header("Rope elasticity (whole rig)")]
        [Tooltip("Combined axial stiffness of all ropes in newtons per metre. Each rope gets an equal share. Static stretch is weight / stiffness.")]
        [SerializeField] private float stiffnessNPerM = 1000000f;

        [Tooltip("Combined axial damping of all ropes in newton-seconds per metre. Each rope gets an equal share.")]
        [SerializeField] private float dampingNsPerM = 60000f;

        [Header("Anti-sway")]
        [Tooltip("Damping ratio of the anti-sway system acting on the load's swing. 0 turns it off; 0.3 to 0.5 settles a swing in about one cycle; 1 is critical damping.")]
        [Range(0f, 1f)]
        [SerializeField] private float swayDampingRatio = 0.35f;

        private ConfigurableJoint[] joints = new ConfigurableJoint[0];
        private Transform[] ropeEnds = new Transform[0];
        private float[] ropeTensionsN = new float[0];
        private Rigidbody head;
        private Vector3 lastHeadPosition;

        /// True while ropes connect the head to a load.
        public bool IsAttached => joints.Length > 0;

        /// Unattached, Attached, Released, or Broken. Only this component changes it.
        public CableState State { get; private set; } = CableState.Unattached;

        /// The load on the ropes, or null.
        public CableLoad Load { get; private set; }

        /// While unattached: the load within reach of the spreader, or null. Space would attach to this one.
        public CableLoad Candidate { get; private set; }

        /// Magnitude of the net force the ropes applied to the load in the last physics step, in newtons.
        /// This is what a load cell at the rope head would read. Zero when slack or detached.
        public float TensionN { get; private set; }

        /// Highest net tension seen since the last attach, in newtons.
        public float PeakTensionN { get; private set; }

        /// Force in each rope in the last physics step, in newtons. Uneven values mean the load is off-centre or tilted.
        public System.Collections.Generic.IReadOnlyList<float> RopeTensionsN => ropeTensionsN;

        /// True when attached but every rope is shorter than the permitted length (the ropes carry no load).
        public bool IsSlack => IsAttached && ActualSeparation < PermittedLength - 0.02f;

        /// Horizontal anti-sway force applied to the load in the last physics step, in newtons.
        public float AntiSwayForceN { get; private set; }

        /// Rated load of the rig in newtons.
        public float BreakForceN => breakForceN;

        /// Shortest and longest permitted rope length in metres.
        public float MinLength => minLength;
        public float MaxLength => maxLength;

        /// Raised after a load is hooked.
        public event System.Action<CableLoad> Attached;

        /// Raised after the player releases a load. The load keeps its velocity.
        public event System.Action<CableLoad> Released;

        /// Raised once when the rig fails under load. The argument is the load that was dropped.
        public event System.Action<CableLoad> Broke;

        /// The permitted rope length in metres (every rope joint's distance limit).
        public float PermittedLength { get; private set; }

        /// Centre of the rope ends: the load's anchor centre when attached, otherwise straight below the
        /// head at the permitted length. The empty spreader is a marker, not a simulated body.
        public Vector3 SpreaderPosition
        {
            get
            {
                if (IsAttached && Load != null) return Load.AnchorCentre;
                return OriginCentre + Vector3.down * PermittedLength;
            }
        }

        /// Length of the longest rope right now (origin to its end), in metres.
        public float ActualSeparation
        {
            get
            {
                if (!IsAttached) return PermittedLength;
                float longest = 0f;
                for (int i = 0; i < ropeEnds.Length; i++)
                    longest = Mathf.Max(longest, Vector3.Distance(ropeOrigins[i].position, ropeEnds[i].position));
                return longest;
            }
        }

        private Vector3 OriginCentre
        {
            get
            {
                if (ropeOrigins == null || ropeOrigins.Length == 0) return transform.position;
                Vector3 sum = Vector3.zero;
                for (int i = 0; i < ropeOrigins.Length; i++) sum += ropeOrigins[i].position;
                return sum / ropeOrigins.Length;
            }
        }

        private void Awake()
        {
            PermittedLength = Mathf.Clamp(startLength, minLength, maxLength);
            CableLoad.WarnIfScaled(ropeOrigins, this);
            head = GetComponent<Rigidbody>();
            lastHeadPosition = head.position;
        }

        /// Hooks the load whose anchor centre is within attachRange of the spreader. Each rope origin is
        /// paired with its nearest free anchor. The permitted length starts at the longest of those ropes,
        /// so every joint starts satisfied and applies no impulse.
        /// Returns false if no load is in reach or the ropes would be outside the permitted range.
        public bool Attach()
        {
            if (IsAttached) return true;
            if (ropeOrigins == null || ropeOrigins.Length == 0)
            {
                Debug.LogError("[Cable] No rope origins assigned.", this);
                return false;
            }

            CableLoad target = FindLoadInRange();
            if (target == null) return false;

            int count = Mathf.Min(ropeOrigins.Length, target.Anchors.Count);
            if (count == 0) return false;

            // Pair each origin with the nearest anchor that is still free, judged in plan view.
            var ends = new Transform[count];
            var taken = new bool[target.Anchors.Count];
            float longest = 0f;
            for (int i = 0; i < count; i++)
            {
                int best = -1;
                float bestSqr = float.MaxValue;
                for (int a = 0; a < target.Anchors.Count; a++)
                {
                    if (taken[a]) continue;
                    Vector3 d = target.Anchors[a].position - ropeOrigins[i].position;
                    float sqr = d.x * d.x + d.z * d.z;
                    if (sqr < bestSqr) { bestSqr = sqr; best = a; }
                }
                taken[best] = true;
                ends[i] = target.Anchors[best];
                longest = Mathf.Max(longest, Vector3.Distance(ropeOrigins[i].position, ends[i].position));
            }

            if (longest < minLength - 0.01f || longest > maxLength + 0.01f)
            {
                Debug.LogWarning($"[Cable] Cannot attach: rope length {longest:F2} m is outside {minLength}-{maxLength} m.", this);
                return false;
            }

            joints = new ConfigurableJoint[count];
            ropeTensionsN = new float[count];
            ropeEnds = ends;
            for (int i = 0; i < count; i++)
            {
                joints[i] = CreateRope(target, ropeOrigins[i], ends[i], count);
            }

            Load = target;
            Candidate = null;
            SetPermittedLength(longest);
            State = CableState.Attached;
            TensionN = 0f;
            PeakTensionN = 0f;
            Attached?.Invoke(Load);
            return true;
        }

        private ConfigurableJoint CreateRope(CableLoad target, Transform origin, Transform end, int ropeCount)
        {
            // The joint sits on the rope head and connects to the load.
            var joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = target.Body;

            // Anchors are given in each body's own local space. Auto-configure is OFF on purpose:
            // with it on, Unity would derive the connected anchor from the starting pose
            // instead of from the load's marked corner.
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = transform.InverseTransformPoint(origin.position);
            joint.connectedAnchor = target.transform.InverseTransformPoint(end.position);

            // Distance limit on all three linear axes, no rotational constraint. One rope alone lets the
            // load turn freely; four ropes to four corners resist tilt and yaw through their geometry.
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            // Soft limit: beyond the permitted length a rope pulls back like a stiff damped spring,
            // F = k * stretch + c * stretch rate. Inside the limit it applies nothing, so it can never push.
            // Ropes act in parallel, so each carries an equal share of the rig's stiffness and damping.
            joint.linearLimitSpring = new SoftJointLimitSpring
            {
                spring = stiffnessNPerM / ropeCount,
                damper = dampingNsPerM / ropeCount,
            };

            // Head and load never collide through the joint; the ropes, not contact, couple them.
            joint.enableCollision = false;

            // Overload is judged on the whole rig in FixedUpdate, not per joint.
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            return joint;
        }

        private void FixedUpdate()
        {
            // The head is kinematic, so its velocity is taken from its own displacement per step: v = dx / dt.
            Vector3 headVelocity = (head.position - lastHeadPosition) / Time.fixedDeltaTime;
            lastHeadPosition = head.position;
            AntiSwayForceN = 0f;

            if (!IsAttached)
            {
                TensionN = 0f;
                Candidate = FindLoadInRange();
                return;
            }

            // currentForce is each joint's constraint force from the previous step. The ropes pull in
            // slightly different directions, so the load on the rig is the magnitude of their vector sum.
            Vector3 net = Vector3.zero;
            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 force = joints[i].currentForce;
                ropeTensionsN[i] = force.magnitude;
                net += force;
            }
            TensionN = net.magnitude;
            if (TensionN > PeakTensionN) PeakTensionN = TensionN;

            if (TensionN > breakForceN)
            {
                Break();
                return;
            }

            ApplyAntiSway(headVelocity);
        }

        /// Anti-sway: a viscous damper on the load's horizontal velocity relative to the rope head.
        ///
        /// A load on ropes of length L is a pendulum with natural frequency w = sqrt(g / L). A damped
        /// oscillator m*x'' + c*x' + k*x = 0 has damping ratio z = c / (2*m*w), so the coefficient that
        /// gives a chosen ratio is c = 2*z*m*w. The force each step is F = -c * v_rel, horizontal only.
        /// It is scaled by how much of the load's weight the ropes carry, so it fades out as the load
        /// is set down and never drags a container that is resting on the ground.
        private void ApplyAntiSway(Vector3 headVelocity)
        {
            if (swayDampingRatio <= 0f || Load == null) return;

            Rigidbody body = Load.Body;
            float length = Mathf.Max(ActualSeparation, 1f);
            float omega = Mathf.Sqrt(Physics.gravity.magnitude / length);
            float coefficient = 2f * swayDampingRatio * body.mass * omega;

            Vector3 relative = body.linearVelocity - headVelocity;
            relative.y = 0f;

            float carried = Mathf.Clamp01(TensionN / Load.WeightN);
            Vector3 force = -coefficient * carried * relative;

            body.AddForce(force, ForceMode.Force);
            AntiSwayForceN = force.magnitude;
        }

        /// Removes the ropes. The load keeps whatever velocity it had; nothing is re-posed or zeroed.
        public void Release()
        {
            if (!IsAttached) return;
            CableLoad dropped = Load;
            DestroyRopes();
            State = CableState.Released;
            Released?.Invoke(dropped);
        }

        private void Break()
        {
            CableLoad dropped = Load;
            float force = TensionN;
            DestroyRopes();
            State = CableState.Broken;
            Debug.Log($"[Cable] Rig failed at {force:F0} N (rating {breakForceN:F0} N).", this);
            Broke?.Invoke(dropped);
        }

        private void DestroyRopes()
        {
            // Keep the spreader where the load's top was, so it does not jump when the ropes go.
            PermittedLength = Mathf.Clamp(OriginCentre.y - SpreaderPosition.y, minLength, maxLength);

            for (int i = 0; i < joints.Length; i++) Destroy(joints[i]);
            joints = new ConfigurableJoint[0];
            ropeEnds = new Transform[0];
            for (int i = 0; i < ropeTensionsN.Length; i++) ropeTensionsN[i] = 0f;
            Load = null;
            TensionN = 0f;
        }

        /// Sets every rope's distance limit, clamped to the permitted range.
        public void SetPermittedLength(float metres)
        {
            PermittedLength = Mathf.Clamp(metres, minLength, maxLength);
            if (!IsAttached) return;

            for (int i = 0; i < joints.Length; i++)
            {
                var limit = joints[i].linearLimit;
                limit.limit = PermittedLength;
                limit.bounciness = 0f;
                limit.contactDistance = 0f;
                joints[i].linearLimit = limit;
            }

            // A load resting on the ground may be asleep; a changing limit must be able to lift it.
            if (Load != null) Load.Body.WakeUp();
        }

        /// Nearest enabled load whose anchor centre is within attachRange of the spreader, or null.
        private CableLoad FindLoadInRange()
        {
            if (ropeOrigins == null || ropeOrigins.Length == 0) return null;

            Vector3 spreader = SpreaderPosition;
            CableLoad best = null;
            float bestSqr = attachRange * attachRange;
            var loads = CableLoad.Active;
            for (int i = 0; i < loads.Count; i++)
            {
                CableLoad candidate = loads[i];
                if (candidate.Anchors == null || candidate.Anchors.Count == 0) continue;
                float sqr = (candidate.AnchorCentre - spreader).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    best = candidate;
                    bestSqr = sqr;
                }
            }
            return best;
        }

        private void LateUpdate()
        {
            if (ropeOrigins == null || ropeOrigins.Length == 0) return;

            // A failed rig has no ropes left to draw: hide the lines and the spreader.
            bool intact = State != CableState.Broken;
            if (spreaderVisual != null && spreaderVisual.gameObject.activeSelf != intact)
            {
                spreaderVisual.gameObject.SetActive(intact);
            }
            if (ropeLines != null)
            {
                for (int i = 0; i < ropeLines.Length; i++)
                {
                    if (ropeLines[i] != null) ropeLines[i].enabled = intact;
                }
            }
            if (!intact) return;

            if (spreaderVisual != null)
            {
                spreaderVisual.position = SpreaderPosition;
                spreaderVisual.rotation = IsAttached && Load != null ? Load.transform.rotation : transform.rotation;
            }

            if (ropeLines == null) return;
            for (int i = 0; i < ropeLines.Length && i < ropeOrigins.Length; i++)
            {
                if (ropeLines[i] == null) continue;
                Vector3 start = ropeOrigins[i].position;
                Vector3 end = i < ropeEnds.Length ? ropeEnds[i].position : start + Vector3.down * PermittedLength;
                ropeLines[i].positionCount = 2;
                ropeLines[i].SetPosition(0, start);
                ropeLines[i].SetPosition(1, end);
            }
        }
    }
}
