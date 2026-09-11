using UnityEngine;
using UnityEngine.InputSystem;

namespace SwingShift
{
    /// Turns raw keyboard input into intent for the active control mode.
    /// Owns the generated SwingShiftActions instance. Gameplay components read intent from here;
    /// they never touch the Input System directly.
    /// 
    /// Input events arrive between rendered frames, but physics commands are applied in FixedUpdate.
    /// Button presses are therefore latched until a physics component consumes them, so a quick tap
    /// is never lost between two physics steps.
    public class PlayerInputRouter : MonoBehaviour
    {
        private SwingShiftActions actions;
        private bool attachReleaseLatched;

        /// Hoist intent from -1 (lower, E) to +1 (raise, Q). Read every frame from the axis.
        public float HoistAxis { get; private set; }

        private void Awake()
        {
            actions = new SwingShiftActions();
            actions.Crane.AttachRelease.performed += OnAttachRelease;
        }

        private void OnEnable()
        {
            actions.Crane.Enable();
        }

        private void OnDisable()
        {
            actions.Crane.Disable();
            HoistAxis = 0f;
            attachReleaseLatched = false;
        }

        private void OnDestroy()
        {
            actions.Crane.AttachRelease.performed -= OnAttachRelease;
            actions.Dispose();
        }

        private void Update()
        {
            HoistAxis = actions.Crane.Hoist.ReadValue<float>();
        }

        private void OnAttachRelease(InputAction.CallbackContext context)
        {
            attachReleaseLatched = true;
        }

        /// Returns true once per Space press, then clears the latch.
        public bool ConsumeAttachRelease()
        {
            if (!attachReleaseLatched) return false;
            attachReleaseLatched = false;
            return true;
        }
    }
}
