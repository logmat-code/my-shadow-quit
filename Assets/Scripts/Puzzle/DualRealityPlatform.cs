using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Dual reality platform in World 5 (The Abyss of the Last Flame).
    /// Alternates between Light Phase (solid to Player) and Shadow Phase (solid to Shadow),
    /// or switches based on Flame Anchor presence.
    /// Implements IMovingSurface for seamless passenger movement.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DualRealityPlatform : MonoBehaviour, IMovingSurface
    {
        public enum RealityState
        {
            LightActive,
            ShadowActive,
            AlternatingTimer
        }

        [Header("State Settings")]
        [SerializeField] private RealityState mode = RealityState.AlternatingTimer;
        [SerializeField] private float switchInterval = 3.5f;
        [SerializeField] private bool startsInLightState = true;

        [Header("Visuals")]
        [SerializeField] private Sprite lightStateSprite;
        [SerializeField] private Sprite shadowStateSprite;

        private BoxCollider2D col;
        private SpriteRenderer sr;
        private float timer;
        private bool isLightState;

        public Vector2 SurfaceVelocity => Vector2.zero; // Static surface or zero velocity
        public bool IsLightState => isLightState;

        private void Awake()
        {
            col = GetComponent<BoxCollider2D>();
            sr = GetComponent<SpriteRenderer>();

            if (lightStateSprite == null)
                lightStateSprite = Resources.Load<Sprite>("Sprites/World5/dual_plat_light");
            if (shadowStateSprite == null)
                shadowStateSprite = Resources.Load<Sprite>("Sprites/World5/dual_plat_shadow");

            isLightState = startsInLightState;
            UpdateRealityState();
        }

        public void Configure(RealityState platformMode, float interval = 3.5f, bool startLight = true)
        {
            mode = platformMode;
            switchInterval = interval;
            isLightState = startLight;
            UpdateRealityState();
        }

        private void Update()
        {
            if (mode == RealityState.AlternatingTimer)
            {
                timer += Time.deltaTime;
                if (timer >= switchInterval)
                {
                    timer = 0f;
                    SetLightState(!isLightState);
                }
            }
        }

        public void SetLightState(bool light)
        {
            isLightState = light;
            UpdateRealityState();
        }

        private void UpdateRealityState()
        {
            if (sr != null)
            {
                sr.sprite = isLightState ? lightStateSprite : shadowStateSprite;
                // Subtle alpha/tint hint
                sr.color = isLightState ? new Color(1f, 1f, 1f, 0.95f) : new Color(0.9f, 0.7f, 1f, 0.95f);
            }

            // In Light State, solid to Player; in Shadow State, solid to Shadow
            GameObject player = GameObject.FindWithTag("Player");
            GameObject shadow = GameObject.FindWithTag("Shadow");

            if (col != null)
            {
                if (player != null)
                {
                    Collider2D pCol = player.GetComponent<Collider2D>();
                    if (pCol != null) Physics2D.IgnoreCollision(col, pCol, !isLightState);
                }
                if (shadow != null)
                {
                    Collider2D sCol = shadow.GetComponent<Collider2D>();
                    if (sCol != null) Physics2D.IgnoreCollision(col, sCol, isLightState);
                }
            }
        }
    }
}
