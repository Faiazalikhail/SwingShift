using UnityEngine;

namespace SwingShift
{
    /// Gives a container its mass and colour when the run starts.
    /// Mass and colour are drawn independently, so colour tells the player nothing about weight:
    /// the only way to learn a container's mass is to lift it and read the tension.
    [RequireComponent(typeof(Rigidbody))]
    public class Container : MonoBehaviour
    {
        [Tooltip("Possible masses in kilograms. One is picked at random. Unity recomputes the inertia tensor from the box collider for the chosen mass.")]
        [SerializeField] private float[] massOptionsKg = { 3000f, 5500f, 7500f };

        [Tooltip("Possible paint colours. One is picked at random, independent of mass.")]
        [SerializeField] private Color[] colours =
        {
            new Color(0.80f, 0.22f, 0.17f),
            new Color(0.16f, 0.42f, 0.70f),
            new Color(0.95f, 0.65f, 0.12f),
            new Color(0.18f, 0.55f, 0.34f),
            new Color(0.55f, 0.57f, 0.60f),
        };

        [Tooltip("Renderer that shows the paint colour.")]
        [SerializeField] private Renderer bodyRenderer;

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        /// Mass in kilograms chosen for this run.
        public float MassKg { get; private set; }

        private void Awake()
        {
            var body = GetComponent<Rigidbody>();
            if (massOptionsKg != null && massOptionsKg.Length > 0)
            {
                body.mass = massOptionsKg[Random.Range(0, massOptionsKg.Length)];
            }
            MassKg = body.mass;

            if (bodyRenderer != null && colours != null && colours.Length > 0)
            {
                // A property block tints this renderer only; the shared material asset is untouched.
                var block = new MaterialPropertyBlock();
                bodyRenderer.GetPropertyBlock(block);
                block.SetColor(ColorId, colours[Random.Range(0, colours.Length)]);
                bodyRenderer.SetPropertyBlock(block);
            }
        }
    }
}
