using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwingShift
{
    public enum RunState { Waiting, Running, ShipLoaded, TimeUp, RigFailed }

    /// Owns the run: timer, delivered count, score, the single outcome, and restart.
    /// It reads the physics (where containers are, how fast they move, whether the rig failed)
    /// and never changes it.
    public class RunManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private CableController cable;

        [Tooltip("Trigger box that fills the ship's hold. Only its bounds are used.")]
        [SerializeField] private BoxCollider holdZone;

        [Header("Rules")]
        [Tooltip("Length of the shift in seconds.")]
        [SerializeField] private float shiftSeconds = 90f;

        [Tooltip("Containers that must be resting in the hold to finish the shift early.")]
        [SerializeField] private int targetCount = 4;

        [Tooltip("A container counts as resting below this linear speed, in metres per second.")]
        [SerializeField] private float restSpeed = 0.1f;

        [Tooltip("A container counts as resting below this angular speed, in radians per second.")]
        [SerializeField] private float restAngularSpeed = 0.1f;

        [SerializeField] private int pointsPerContainer = 100;
        [SerializeField] private int pointsPerSecondLeft = 10;

        public RunState State { get; private set; } = RunState.Waiting;

        /// Seconds left in the shift.
        public float TimeLeft { get; private set; }

        /// Containers resting in the hold right now. A container knocked back out stops counting.
        public int Delivered { get; private set; }

        public int Score { get; private set; }

        /// True once the run has reached its one outcome.
        public bool IsOver => State == RunState.ShipLoaded || State == RunState.TimeUp || State == RunState.RigFailed;

        private float failForceN;

        private void Awake()
        {
            TimeLeft = shiftSeconds;
        }

        private void OnEnable()
        {
            if (cable != null) cable.Broke += OnRigFailed;
        }

        private void OnDisable()
        {
            if (cable != null) cable.Broke -= OnRigFailed;
        }

        private void Update()
        {
            if (input == null) return;

            // R reloads the scene: every body returns to its authored pose and the static load list refills.
            if (input.ConsumeRestart())
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (IsOver) return;

            // The shift starts on the first crane command, so the player has time to read the screen.
            if (State == RunState.Waiting)
            {
                bool commanded = input.HoistAxis != 0f || input.SlewAxis != 0f || input.TrolleyAxis != 0f;
                if (commanded) State = RunState.Running;
                return;
            }

            TimeLeft = Mathf.Max(0f, TimeLeft - Time.deltaTime);
            Delivered = CountDelivered();
            Score = Delivered * pointsPerContainer;

            if (Delivered >= targetCount)
            {
                Score += Mathf.FloorToInt(TimeLeft) * pointsPerSecondLeft;
                End(RunState.ShipLoaded);
            }
            else if (TimeLeft <= 0f)
            {
                End(RunState.TimeUp);
            }
        }

        /// A container is delivered when it is off the ropes, its centre of mass is inside the hold,
        /// and it is at rest: both linear and angular speed under their thresholds.
        private int CountDelivered()
        {
            if (holdZone == null) return 0;

            Bounds hold = holdZone.bounds;
            int count = 0;
            var loads = CableLoad.Active;
            for (int i = 0; i < loads.Count; i++)
            {
                CableLoad load = loads[i];
                if (cable != null && load == cable.Load) continue;
                if (!hold.Contains(load.Body.worldCenterOfMass)) continue;
                if (load.Body.linearVelocity.magnitude > restSpeed) continue;
                if (load.Body.angularVelocity.magnitude > restAngularSpeed) continue;
                count++;
            }
            return count;
        }

        private void OnRigFailed(CableLoad dropped)
        {
            if (IsOver) return;
            failForceN = cable.PeakTensionN;
            End(RunState.RigFailed);
        }

        private void End(RunState outcome)
        {
            State = outcome;
            input.SetCraneControlEnabled(false);
        }

        /// Text for the HUD: one status line while playing, the outcome when the run is over.
        public string StatusText()
        {
            string line = $"Time {TimeLeft:F0} s   Delivered {Delivered}/{targetCount}   Score {Score}";
            switch (State)
            {
                case RunState.Waiting:
                    return line + "   -  move the crane to start the shift";
                case RunState.ShipLoaded:
                    return line + "\nSHIP LOADED  -  press R to play again";
                case RunState.TimeUp:
                    return line + "\nTIME UP  -  press R to try again";
                case RunState.RigFailed:
                    return line + $"\nRIG OVERLOADED at {failForceN / 1000f:F0} kN (rating {cable.BreakForceN / 1000f:F0} kN)  -  press R to try again";
                default:
                    return line;
            }
        }
    }
}
