using UnityEngine;

namespace SwingShift
{
    /// Orbit camera that follows the load. Mouse wheel zooms, holding the right mouse button orbits.
    /// By default the view turns with the jib, so W is always "away from the tower" on screen.
    /// Presentation only: runs in LateUpdate and reads interpolated transforms.
    public class CameraRig : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private CableController cable;
        [SerializeField] private CraneController crane;

        [Header("Follow")]
        [Tooltip("0 looks at the rope head, 1 looks at the spreader or load.")]
        [Range(0f, 1f)]
        [SerializeField] private float focusBias = 0.75f;

        [Tooltip("Time the focus point takes to catch up, in seconds. Keeps the view steady while the load swings.")]
        [SerializeField] private float followSmoothTime = 0.35f;

        [Tooltip("Turn the view with the jib.")]
        [SerializeField] private bool followJibYaw = true;

        [Header("Zoom")]
        [SerializeField] private float distance = 32f;
        [SerializeField] private float minDistance = 8f;
        [SerializeField] private float maxDistance = 80f;

        [Tooltip("Fraction of the current distance moved per wheel notch.")]
        [SerializeField] private float zoomPerNotch = 0.12f;

        [Header("Orbit")]
        [Tooltip("Yaw offset from the jib direction (or from world +Z when not following), in degrees.")]
        [SerializeField] private float yawOffsetDeg = 0f;

        [SerializeField] private float pitchDeg = 40f;
        [SerializeField] private float minPitchDeg = 5f;
        [SerializeField] private float maxPitchDeg = 88f;

        [Tooltip("Degrees of orbit per pixel of mouse movement.")]
        [SerializeField] private float orbitSensitivity = 0.2f;

        private Vector3 focus;
        private Vector3 focusVelocity;
        private bool hasFocus;

        private void LateUpdate()
        {
            if (cable == null) return;

            if (input != null)
            {
                // One wheel notch reports about 120 units on Windows; normalise to notches.
                float notches = input.ZoomDelta / 120f;
                if (Mathf.Abs(notches) > 0f)
                {
                    distance = Mathf.Clamp(distance * Mathf.Pow(1f - zoomPerNotch, notches), minDistance, maxDistance);
                }

                if (input.OrbitHeld)
                {
                    yawOffsetDeg += input.LookDelta.x * orbitSensitivity;
                    pitchDeg = Mathf.Clamp(pitchDeg - input.LookDelta.y * orbitSensitivity, minPitchDeg, maxPitchDeg);
                }
            }

            Vector3 target = Vector3.Lerp(cable.transform.position, cable.SpreaderPosition, focusBias);
            if (!hasFocus)
            {
                focus = target;
                hasFocus = true;
            }
            focus = Vector3.SmoothDamp(focus, target, ref focusVelocity, followSmoothTime);

            float yaw = yawOffsetDeg + (followJibYaw && crane != null ? crane.SlewAngleDeg : 0f);
            Quaternion rotation = Quaternion.Euler(pitchDeg, yaw, 0f);
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
        }
    }
}
