using TMPro;
using UnityEngine;

namespace SwingShift
{
    /// Displays current cable and crane state. Read-only: it never changes gameplay state.
    /// Physics values are kept in newtons; this component only converts to kN for display.
    public class PrototypeHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CableController cable;
        [SerializeField] private CraneController crane;
        [SerializeField] private TMP_Text text;

        [Header("Display")]
        [Tooltip("Time constant for smoothing the displayed tension, in seconds. Presentation only; the break check uses the raw value.")]
        [SerializeField] private float gaugeSmoothing = 0.08f;

        private float displayedTensionN;

        private void Update()
        {
            if (cable == null || text == null) return;

            // Exponential smoothing so the number is readable at 50 Hz physics updates.
            float k = gaugeSmoothing > 0f ? 1f - Mathf.Exp(-Time.deltaTime / gaugeSmoothing) : 1f;
            displayedTensionN = Mathf.Lerp(displayedTensionN, cable.TensionN, k);

            string state = cable.State switch
            {
                CableState.Attached => cable.IsSlack ? "ATTACHED (slack)" : "ATTACHED (taut)",
                CableState.Released => "RELEASED",
                CableState.Broken => "BROKEN",
                _ => "UNATTACHED",
            };

            float hoist = crane != null ? crane.HoistVelocity : 0f;

            text.text =
                $"Cable: {state}\n" +
                $"Length: {cable.PermittedLength:F2} m  (actual {cable.ActualSeparation:F2} m)\n" +
                $"Tension: {displayedTensionN / 1000f:F2} kN   peak {cable.PeakTensionN / 1000f:F2} kN   rating {cable.BreakForceN / 1000f:F1} kN\n" +
                $"Hoist: {hoist:+0.00;-0.00;0.00} m/s   [Q raise  E lower  Space attach/release]";
        }
    }
}
