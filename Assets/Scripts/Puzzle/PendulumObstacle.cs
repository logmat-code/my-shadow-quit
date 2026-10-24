using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Oscillating gothic pendulum obstacle.
    /// Swings rhythmically across chambers with predictable timing.
    /// Triggers chamber restart on contact with either Protagonist or Shadow.
    /// Can be frozen temporarily by ClockworkSwitches.
    /// </summary>
    public class PendulumObstacle : MonoBehaviour
    {
        private static float s_lastRestartTime = -10f;
        private const float DebounceDuration = 0.85f;

        [SerializeField] private float armLength = 3.2f;
        [SerializeField] private float maxAngle = 55f;
        [SerializeField] private float swingSpeed = 2.2f;
        [SerializeField] private float phaseOffset = 0f;
        [SerializeField] private bool isFrozen = false;

        private Transform _armRoot;
        private Transform _bladeTransform;
        private CircleCollider2D _bladeCollider;
        private float _timeAccumulator;

        public bool IsFrozen
        {
            get => isFrozen;
            set => isFrozen = value;
        }

        public void Configure(float length = 3.2f, float angle = 55f, float speed = 2.2f, float phase = 0f)
        {
            armLength = length;
            maxAngle = angle;
            swingSpeed = speed;
            phaseOffset = phase;
            _timeAccumulator = phase;

            BuildVisualsAndCollider();
        }

        private void Awake()
        {
            BuildVisualsAndCollider();
        }

        public void BuildVisualsAndCollider()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }

            // Pivot hub
            GameObject hubObj = new GameObject("PendulumPivot");
            hubObj.transform.SetParent(transform, false);
            hubObj.transform.localPosition = Vector3.zero;
            _armRoot = hubObj.transform;

            SpriteRenderer hubSr = hubObj.AddComponent<SpriteRenderer>();
            hubSr.sprite = AssetRegistry.GetSprite("World3/clockwork_switch_off", 32f);
            hubSr.sortingOrder = 7;
            hubObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            // Chain arm
            int links = Mathf.Max(2, Mathf.RoundToInt(armLength / 1.0f));
            float linkSpacing = armLength / links;
            Sprite armSprite = AssetRegistry.GetSprite("World3/pendulum_arm", 32f);

            for (int i = 0; i < links; i++)
            {
                GameObject link = new GameObject($"ChainLink_{i}");
                link.transform.SetParent(_armRoot, false);
                link.transform.localPosition = new Vector3(0f, -(i + 0.5f) * linkSpacing, 0f);
                SpriteRenderer lsr = link.AddComponent<SpriteRenderer>();
                lsr.sprite = armSprite;
                lsr.sortingOrder = 6;
                link.transform.localScale = new Vector3(1f, linkSpacing / 2.0f, 1f);
            }

            // Blade at bottom
            GameObject bladeObj = new GameObject("PendulumBlade");
            bladeObj.transform.SetParent(_armRoot, false);
            bladeObj.transform.localPosition = new Vector3(0f, -armLength, 0f);
            SpriteRenderer bsr = bladeObj.AddComponent<SpriteRenderer>();
            bsr.sprite = AssetRegistry.GetSprite("World3/pendulum_blade", 32f);
            bsr.sortingOrder = 8;
            _bladeTransform = bladeObj.transform;

            _bladeCollider = bladeObj.AddComponent<CircleCollider2D>();
            _bladeCollider.isTrigger = true;
            _bladeCollider.radius = 0.65f;
            _bladeCollider.offset = new Vector2(0f, -0.1f);

            var triggerReceiver = bladeObj.AddComponent<PendulumBladeContact>();
            triggerReceiver.Initialize(this);
        }

        private void Update()
        {
            if (isFrozen || _armRoot == null) return;

            _timeAccumulator += Time.deltaTime * swingSpeed;
            float currentAngle = maxAngle * Mathf.Sin(_timeAccumulator);
            _armRoot.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }

        public void EvaluateContact(Collider2D other)
        {
            if (other == null) return;

            bool isPlayer = other.GetComponent<PlayerController>() != null || other.CompareTag("Player");
            bool isShadow = other.GetComponent<ShadowController>() != null || other.CompareTag("Shadow");

            if (!isPlayer && !isShadow) return;

            if (Time.unscaledTime - s_lastRestartTime < DebounceDuration) return;
            s_lastRestartTime = Time.unscaledTime;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("stone_door", 0.85f, 1.4f);
            }

            if (Combat.HealthSystem.Instance != null)
            {
                Combat.HealthSystem.Instance.TakeDamage(1, Vector2.up * 5.5f, 0.85f);
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }

        public void SetFrozen(bool frozen)
        {
            isFrozen = frozen;
        }

        private class PendulumBladeContact : MonoBehaviour
        {
            private PendulumObstacle _parent;

            public void Initialize(PendulumObstacle parent)
            {
                _parent = parent;
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (_parent != null) _parent.EvaluateContact(other);
            }

            private void OnTriggerStay2D(Collider2D other)
            {
                if (_parent != null) _parent.EvaluateContact(other);
            }
        }
    }
}
