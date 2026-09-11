using UnityEngine;

namespace SwingShift
{
    /// Owns the crane's hoist command and the attach/release request.
    /// Reads intent from PlayerInputRouter and drives CableController on fixed physics steps.
    /// Arm and trolley motors are added here in Session 2.
    public class CraneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private CableController cable;

        [Header("Hoist")]
        [Tooltip("Maximum rate at which the permitted cable length changes, in metres per second.")]
        [SerializeField] private float hoistSpeed = 1.0f;

        [Tooltip("How quickly the winch reaches full speed, in metres per second squared. Keeps length changes smooth so the solver never sees a jump.")]
        [SerializeField] private float hoistAcceleration = 4.0f;

        /// Current winch rate in metres per second. Positive shortens the cable (raising).
        public float HoistVelocity { get; private set; }

        private void FixedUpdate()
        {
            if (input == null || cable == null) return;

            if (input.ConsumeAttachRelease())
            {
                if (cable.IsAttached) cable.Release();
                else cable.Attach();
            }

            // Ramp the winch rate toward the requested rate instead of stepping it.
            float targetVelocity = cable.IsAttached ? input.HoistAxis * hoistSpeed : 0f;
            HoistVelocity = Mathf.MoveTowards(HoistVelocity, targetVelocity, hoistAcceleration * Time.fixedDeltaTime);

            if (Mathf.Abs(HoistVelocity) > 0f)
            {
                // Raising means a shorter permitted length.
                cable.SetPermittedLength(cable.PermittedLength - HoistVelocity * Time.fixedDeltaTime);
            }
        }
    }
}
