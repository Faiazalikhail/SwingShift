using UnityEngine;

namespace SwingShift
{
    /// Owns the tower crane's commands: jib slew, trolley travel, hoist, and the attach/release request.
    /// Reads intent from PlayerInputRouter and drives the rope head body and CableController on fixed physics steps.
    ///
    /// The rope head's position is held in polar form about the tower axis: slew angle and trolley radius.
    /// Only the rope head is a physics body. The jib and trolley models are posed from it each frame.
    public class CraneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private CableController cable;

        [Tooltip("The rope head's kinematic rigidbody. It is moved with MovePosition so PhysX knows its velocity and the rope joints react to it. Its rotation is never changed: the spreader keeps its heading while the jib slews.")]
        [SerializeField] private Rigidbody ropeHead;

        [Tooltip("Transform on the tower axis that carries the jib model. Its local +Z points along the jib. Rotated about Y for presentation.")]
        [SerializeField] private Transform jibPivot;

        [Tooltip("Optional trolley model, a child of the jib pivot. Its local Z is set to the trolley radius.")]
        [SerializeField] private Transform trolleyVisual;

        [Header("Slew")]
        [Tooltip("Maximum slew rate in degrees per second.")]
        [SerializeField] private float slewSpeedDeg = 10f;

        [Tooltip("Slew angular acceleration in degrees per second squared.")]
        [SerializeField] private float slewAccelerationDeg = 5f;

        [Tooltip("Slew limits in degrees of yaw. 0 is world +Z, 90 is world +X.")]
        [SerializeField] private float slewMinDeg = -30f;
        [SerializeField] private float slewMaxDeg = 135f;

        [Header("Trolley")]
        [Tooltip("Maximum trolley speed along the jib, in metres per second.")]
        [SerializeField] private float trolleySpeed = 2.5f;

        [Tooltip("Trolley acceleration in metres per second squared.")]
        [SerializeField] private float trolleyAcceleration = 1.2f;

        [Tooltip("Closest and farthest trolley positions from the tower axis, in metres.")]
        [SerializeField] private float minRadius = 6f;
        [SerializeField] private float maxRadius = 28f;

        [Header("Hoist")]
        [Tooltip("Maximum rate at which the permitted rope length changes, in metres per second.")]
        [SerializeField] private float hoistSpeed = 3.0f;

        [Tooltip("How quickly the winch reaches full speed, in metres per second squared. Adds m*a to the tension while it ramps.")]
        [SerializeField] private float hoistAcceleration = 1.0f;

        [Tooltip("Raise speed while the ropes are not yet carrying the load, in metres per second. Taking up slack slowly keeps the snatch load small.")]
        [SerializeField] private float takeUpSpeed = 0.15f;

        [Tooltip("The ropes count as carrying the load once tension reaches this fraction of the load's weight.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float carryingFraction = 0.8f;

        /// Jib yaw in degrees. 0 is world +Z, positive turns toward world +X.
        public float SlewAngleDeg { get; private set; }

        /// Current slew rate in degrees per second.
        public float SlewRateDeg { get; private set; }

        /// Trolley distance from the tower axis in metres.
        public float Radius { get; private set; }

        /// Current trolley velocity along the jib, in metres per second. Positive is outward.
        public float TrolleyVelocity { get; private set; }

        /// Speed of the rope head over the ground from slewing alone: v = omega * r, in metres per second.
        public float TangentialSpeed => Mathf.Abs(SlewRateDeg) * Mathf.Deg2Rad * Radius;

        /// Current winch rate in metres per second. Positive shortens the ropes (raising).
        public float HoistVelocity { get; private set; }

        /// True while the winch is limited to the take-up speed.
        public bool IsTakingUpSlack { get; private set; }

        private float headHeight;

        private void Start()
        {
            if (ropeHead == null || jibPivot == null) return;

            // Start from wherever the rope head was placed in the scene.
            Vector3 offset = ropeHead.position - jibPivot.position;
            SlewAngleDeg = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            Radius = Mathf.Clamp(new Vector2(offset.x, offset.z).magnitude, minRadius, maxRadius);
            headHeight = ropeHead.position.y;
        }

        private void FixedUpdate()
        {
            if (input == null || cable == null) return;

            if (input.ConsumeAttachRelease())
            {
                if (cable.IsAttached) cable.Release();
                else cable.Attach();
            }

            StepHead(Time.fixedDeltaTime);
            StepHoist(Time.fixedDeltaTime);
        }

        private void StepHead(float dt)
        {
            if (ropeHead == null || jibPivot == null) return;

            // Ramp toward the requested rates instead of stepping them.
            SlewRateDeg = Mathf.MoveTowards(SlewRateDeg, input.SlewAxis * slewSpeedDeg, slewAccelerationDeg * dt);
            TrolleyVelocity = Mathf.MoveTowards(TrolleyVelocity, input.TrolleyAxis * trolleySpeed, trolleyAcceleration * dt);

            // End stops halt the motion at once. The load keeps its momentum and swings, as against a real buffer.
            float angle = SlewAngleDeg + SlewRateDeg * dt;
            if (angle <= slewMinDeg || angle >= slewMaxDeg)
            {
                angle = Mathf.Clamp(angle, slewMinDeg, slewMaxDeg);
                SlewRateDeg = 0f;
            }
            SlewAngleDeg = angle;

            float radius = Radius + TrolleyVelocity * dt;
            if (radius <= minRadius || radius >= maxRadius)
            {
                radius = Mathf.Clamp(radius, minRadius, maxRadius);
                TrolleyVelocity = 0f;
            }
            Radius = radius;

            // Polar to Cartesian about the tower axis: x = r sin(yaw), z = r cos(yaw).
            float yaw = SlewAngleDeg * Mathf.Deg2Rad;
            Vector3 axis = jibPivot.position;
            Vector3 target = new Vector3(axis.x + Radius * Mathf.Sin(yaw), headHeight, axis.z + Radius * Mathf.Cos(yaw));
            ropeHead.MovePosition(target);
        }

        private void StepHoist(float dt)
        {
            float targetVelocity = input.HoistAxis * hoistSpeed;

            // Brake before the end of the drum: from v^2 = 2*a*s, the fastest speed that can still
            // stop within the remaining travel s at the winch's own deceleration a.
            float remaining = targetVelocity > 0f
                ? cable.PermittedLength - cable.MinLength
                : cable.MaxLength - cable.PermittedLength;
            float stoppable = Mathf.Sqrt(2f * hoistAcceleration * Mathf.Max(remaining, 0f));
            targetVelocity = Mathf.Clamp(targetVelocity, -stoppable, stoppable);

            // Until the ropes carry the load's weight, raise only at the take-up speed.
            IsTakingUpSlack = cable.IsAttached
                && targetVelocity > 0f
                && cable.TensionN < carryingFraction * cable.Load.WeightN;
            if (IsTakingUpSlack)
            {
                targetVelocity = Mathf.Min(targetVelocity, takeUpSpeed);
                // Slack ropes carry nothing, so the winch can drop to take-up speed at once.
                // When taut, the ramp below slows it instead, so the load is never left to fall back onto the ropes.
                if (cable.IsSlack) HoistVelocity = Mathf.Min(HoistVelocity, takeUpSpeed);
            }

            HoistVelocity = Mathf.MoveTowards(HoistVelocity, targetVelocity, hoistAcceleration * dt);

            if (Mathf.Abs(HoistVelocity) > 0f)
            {
                // Raising means a shorter permitted length. Unattached, this moves the empty spreader.
                float before = cable.PermittedLength;
                cable.SetPermittedLength(before - HoistVelocity * dt);
                if (Mathf.Approximately(cable.PermittedLength, before)) HoistVelocity = 0f;
            }
        }

        private void LateUpdate()
        {
            if (ropeHead == null || jibPivot == null) return;

            // Pose the models from the rope head's interpolated transform so they never lag the physics body.
            Vector3 offset = ropeHead.transform.position - jibPivot.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.0001f) return;

            jibPivot.rotation = Quaternion.Euler(0f, Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg, 0f);
            if (trolleyVisual != null)
            {
                Vector3 local = trolleyVisual.localPosition;
                local.z = offset.magnitude;
                trolleyVisual.localPosition = local;
            }
        }
    }
}
