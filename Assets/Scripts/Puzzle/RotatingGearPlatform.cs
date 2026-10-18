using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Puzzle
{
    public enum GearMotionType
    {
        Orbital,
        HorizontalOscillation,
        VerticalOscillation,
        StationaryRotating
    }

    /// <summary>
    /// Stable kinematic platform associated with giant rotating clocktower gears.
    /// Keeps the top passenger surface strictly horizontal to guarantee stable physics
    /// and prevent character jitter or catapulting, while rotating the visual gear mechanism.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class RotatingGearPlatform : MonoBehaviour
    {
        [SerializeField] private GearMotionType motionType = GearMotionType.Orbital;
        [SerializeField] private Vector2 orbitCenter = Vector2.zero;
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float startAngle = 0f;
        [SerializeField] private float gearVisualRotationSpeed = 45f;
        [SerializeField] private bool isPaused = false;

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private Transform _gearVisual;
        private float _currentAngle;

        public bool IsPaused
        {
            get => isPaused;
            set => isPaused = value;
        }

        public void Configure(
            GearMotionType type,
            Vector2 center,
            float orbitRadius = 2.5f,
            float motionSpeed = 1.2f,
            float initialAngle = 0f,
            Vector2? platformSize = null)
        {
            motionType = type;
            orbitCenter = center;
            radius = orbitRadius;
            speed = motionSpeed;
            startAngle = initialAngle;
            _currentAngle = initialAngle * Mathf.Deg2Rad;

            EnsureInitialized(platformSize);
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized(Vector2? platformSize = null)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            Vector2 size = platformSize ?? new Vector2(2.6f, 0.55f);
            if (_col != null)
            {
                _col.size = size;
                _col.offset = new Vector2(0f, 0.15f);
                _col.isTrigger = false;
            }

            BuildVisuals(size);
        }

        private void BuildVisuals(Vector2 size)
        {
            // Clean up old visuals
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }

            // 1. Rotating background gear
            GameObject gearObj = new GameObject("GearVisual");
            gearObj.transform.SetParent(transform, false);
            gearObj.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            SpriteRenderer gearSr = gearObj.AddComponent<SpriteRenderer>();
            gearSr.sprite = AssetRegistry.GetSprite("World3/gear_platform", 32f);
            gearSr.sortingOrder = 3;
            _gearVisual = gearObj.transform;

            // 2. Top horizontal stone/iron walkway
            GameObject walkwayObj = new GameObject("WalkwayVisual");
            walkwayObj.transform.SetParent(transform, false);
            walkwayObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            SpriteRenderer walkSr = walkwayObj.AddComponent<SpriteRenderer>();
            walkSr.sprite = AssetRegistry.GetSprite("World3/clocktower_platform", 32f);
            walkSr.sortingOrder = 4;
            walkwayObj.transform.localScale = new Vector3(size.x / 2.0f, 1f, 1f);
        }

        private void FixedUpdate()
        {
            if (isPaused) return;

            float dt = Time.fixedDeltaTime;

            // Rotate the visual gear constantly
            if (_gearVisual != null)
            {
                _gearVisual.Rotate(0f, 0f, -gearVisualRotationSpeed * dt);
            }

            // Move the platform horizontally, vertically, or in orbit
            Vector2 targetPos = _rb.position;
            switch (motionType)
            {
                case GearMotionType.Orbital:
                    _currentAngle += speed * dt;
                    targetPos = orbitCenter + new Vector2(Mathf.Cos(_currentAngle), Mathf.Sin(_currentAngle)) * radius;
                    break;

                case GearMotionType.HorizontalOscillation:
                    _currentAngle += speed * dt;
                    targetPos = orbitCenter + new Vector2(Mathf.Sin(_currentAngle) * radius, 0f);
                    break;

                case GearMotionType.VerticalOscillation:
                    _currentAngle += speed * dt;
                    targetPos = orbitCenter + new Vector2(0f, Mathf.Sin(_currentAngle) * radius);
                    break;

                case GearMotionType.StationaryRotating:
                    targetPos = orbitCenter;
                    break;
            }

            _rb.MovePosition(targetPos);
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
        }
    }
}
