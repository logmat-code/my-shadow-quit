using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Floats on top of the water level in World 4 rooms.
    /// Implements IMovingSurface to smoothly carry Player and Shadow as the water rises and falls.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BuoyantPlatform2D : MonoBehaviour, IMovingSurface
    {
        [Header("Buoyancy Settings")]
        [SerializeField] private WaterLevelZone waterZone;
        [SerializeField] private float waterOffset = 0.25f;
        [SerializeField] private float floatSmoothing = 15.0f;
        [SerializeField] private float bobbingAmplitude = 0.05f;
        [SerializeField] private float bobbingFrequency = 1.4f;

        private Rigidbody2D rb;
        private Vector2 previousPos;
        private Vector2 surfaceVelocity;
        private float bobbingTimer;

        public Vector2 SurfaceVelocity => surfaceVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            previousPos = rb.position;
            bobbingTimer = Random.Range(0f, 6.28f);
        }

        private void Start()
        {
            if (waterZone == null)
            {
                waterZone = WaterLevelZone.GetActiveZone();
            }
        }

        public void Configure(WaterLevelZone zone, float offset = 0.2f)
        {
            waterZone = zone;
            waterOffset = offset;
        }

        private void FixedUpdate()
        {
            bobbingTimer += Time.fixedDeltaTime * bobbingFrequency;
            float targetY = rb.position.y;

            if (waterZone != null)
            {
                float baseWaterY = waterZone.CurrentWaterY + waterOffset;
                float bob = Mathf.Sin(bobbingTimer) * bobbingAmplitude;
                float desiredY = baseWaterY + bob;
                targetY = Mathf.MoveTowards(rb.position.y, desiredY, floatSmoothing * Time.fixedDeltaTime);
            }

            Vector2 nextPos = new Vector2(rb.position.x, targetY);
            surfaceVelocity = (nextPos - previousPos) / Time.fixedDeltaTime;
            previousPos = nextPos;
            rb.MovePosition(nextPos);
        }
    }
}
