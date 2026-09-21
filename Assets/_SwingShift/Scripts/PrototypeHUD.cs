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

        [Tooltip("Optional. When assigned, the run status (time, delivered, score, outcome) is shown first.")]
        [SerializeField] private RunManager run;

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

            // The HUD never shows a container's mass. The player weighs a load by lifting it and reading the tension.
            string reach = cable.IsAttached
                ? "carrying"
                : cable.Candidate != null ? "container in reach  [Space to hook]" : "nothing in reach";

            // Per-rope forces: uneven values mean the load is tilted or its ropes are skewed.
            var ropes = cable.RopeTensionsN;
            string ropeText = "";
            for (int i = 0; i < ropes.Count; i++)
            {
                ropeText += (i > 0 ? " / " : "") + (ropes[i] / 1000f).ToString("F1");
            }
            if (ropes.Count == 0) ropeText = "-";

            float hoist = crane != null ? crane.HoistVelocity : 0f;
            string takeUp = crane != null && crane.IsTakingUpSlack ? "  (taking up slack)" : "";
            string motion = crane != null
                ? $"Slew: {crane.SlewRateDeg:+0.0;-0.0;0.0} deg/s (head {crane.TangentialSpeed:F1} m/s)   Trolley: {crane.TrolleyVelocity:+0.00;-0.00;0.00} m/s   Radius: {crane.Radius:F1} m"
                : "";

            string status = run != null ? run.StatusText() + "\n\n" : "";

            text.text = status +
                $"Ropes: {state}   {reach}\n" +
                $"Length: {cable.PermittedLength:F2} m  (actual {cable.ActualSeparation:F2} m)\n" +
                $"Tension: {displayedTensionN / 1000f:F1} kN   peak {cable.PeakTensionN / 1000f:F1} kN   rating {cable.BreakForceN / 1000f:F0} kN\n" +
                $"Per rope: {ropeText} kN   anti-sway {cable.AntiSwayForceN / 1000f:F1} kN\n" +
                $"Hoist: {hoist:+0.00;-0.00;0.00} m/s{takeUp}\n" +
                $"{motion}\n" +
                "[A/D slew   W/S trolley   Q raise   E lower   Space hook/release   wheel zoom   RMB orbit]";
        }
    }
}
