using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// World 5 spectral bridge that materializes over the infinite void
    /// when activated by Echo Switches or Flame Anchors.
    /// Implements IMovingSurface.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class AbyssalBridge : MonoBehaviour, IMovingSurface
    {
        [Header("Bridge Settings")]
        [SerializeField] private bool startsActive = false;
        [SerializeField] private float fadeSpeed = 3.0f;

        private BoxCollider2D col;
        private SpriteRenderer sr;
        private bool isActive;
        private float currentAlpha;

        public Vector2 SurfaceVelocity => Vector2.zero;
        public bool IsActive => isActive;

        private void Awake()
        {
            col = GetComponent<BoxCollider2D>();
            sr = GetComponent<SpriteRenderer>();
            isActive = startsActive;
            currentAlpha = isActive ? 1.0f : 0.0f;
            ApplyState();
        }

        public void Configure(bool activeAtStart)
        {
            isActive = activeAtStart;
            currentAlpha = isActive ? 1.0f : 0.0f;
            ApplyState();
        }

        private void Update()
        {
            float targetAlpha = isActive ? 1.0f : 0.0f;
            if (!Mathf.Approximately(currentAlpha, targetAlpha))
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = currentAlpha;
                    sr.color = c;
                }
            }
        }

        public void SetBridgeActive(bool active)
        {
            isActive = active;
            ApplyState();
        }

        private void ApplyState()
        {
            if (col != null)
            {
                col.enabled = isActive;
            }
            if (sr != null)
            {
                Color c = sr.color;
                c.a = isActive ? 1.0f : 0.0f;
                sr.color = c;
            }
        }
    }
}
