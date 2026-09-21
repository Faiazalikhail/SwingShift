using UnityEngine;
using UnityEngine.InputSystem;

namespace SwingShift
{
    /// Turns raw keyboard and mouse input into intent for the crane and the camera.
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
        private bool restartLatched;

        /// Hoist intent from -1 (lower, E) to +1 (raise, Q).
        public float HoistAxis { get; private set; }

        /// Slew intent from -1 (jib left, A) to +1 (jib right, D).
        public float SlewAxis { get; private set; }

        /// Trolley intent from -1 (toward the tower, S) to +1 (out along the jib, W).
        public float TrolleyAxis { get; private set; }

        /// Mouse wheel movement this frame. Positive is wheel forward (zoom in).
        public float ZoomDelta { get; private set; }

        /// Mouse movement this frame in pixels. Only meaningful while OrbitHeld is true.
        public Vector2 LookDelta { get; private set; }

        /// True while the right mouse button is held.
        public bool OrbitHeld { get; private set; }

        private void Awake()
        {
            actions = new SwingShiftActions();
            actions.Crane.AttachRelease.performed += OnAttachRelease;
            actions.Crane.Restart.performed += OnRestart;
        }

        private void OnEnable()
        {
            actions.Crane.Enable();
            actions.Camera.Enable();
        }

        private void OnDisable()
        {
            actions.Crane.Disable();
            actions.Camera.Disable();
            HoistAxis = 0f;
            SlewAxis = 0f;
            TrolleyAxis = 0f;
            ZoomDelta = 0f;
            LookDelta = Vector2.zero;
            OrbitHeld = false;
            attachReleaseLatched = false;
            restartLatched = false;
        }

        private void OnDestroy()
        {
            actions.Crane.AttachRelease.performed -= OnAttachRelease;
            actions.Crane.Restart.performed -= OnRestart;
            actions.Dispose();
        }

        private void Update()
        {
            HoistAxis = actions.Crane.Hoist.ReadValue<float>();
            SlewAxis = actions.Crane.Slew.ReadValue<float>();
            TrolleyAxis = actions.Crane.Trolley.ReadValue<float>();

            ZoomDelta = actions.Camera.Zoom.ReadValue<float>();
            LookDelta = actions.Camera.Look.ReadValue<Vector2>();
            OrbitHeld = actions.Camera.Orbit.IsPressed();
        }

        /// Stops crane commands (used when a run ends). Camera and restart keep working.
        public void SetCraneControlEnabled(bool enabled)
        {
            if (enabled)
            {
                actions.Crane.Hoist.Enable();
                actions.Crane.Slew.Enable();
                actions.Crane.Trolley.Enable();
                actions.Crane.AttachRelease.Enable();
            }
            else
            {
                actions.Crane.Hoist.Disable();
                actions.Crane.Slew.Disable();
                actions.Crane.Trolley.Disable();
                actions.Crane.AttachRelease.Disable();
                attachReleaseLatched = false;
            }
        }

        private void OnAttachRelease(InputAction.CallbackContext context)
        {
            attachReleaseLatched = true;
        }

        private void OnRestart(InputAction.CallbackContext context)
        {
            restartLatched = true;
        }

        /// Returns true once per R press, then clears the latch.
        public bool ConsumeRestart()
        {
            if (!restartLatched) return false;
            restartLatched = false;
            return true;
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
