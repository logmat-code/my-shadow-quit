using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Manages the local water tide level within a World 4 cathedral room.
    /// Can raise or lower the tide when triggered by Tide switches.
    /// </summary>
    public class WaterLevelZone : MonoBehaviour
    {
        [Header("Tide Levels")]
        [SerializeField] private float lowWaterY = -2f;
        [SerializeField] private float highWaterY = 2f;
        [SerializeField] private float transitionSpeed = 2.5f;
        [SerializeField] private bool startsHigh = false;

        private float currentWaterY;
        private float targetWaterY;
        private Transform surfaceVisual;
        private Transform bodyVisual;
        private SpriteRenderer bodyRenderer;
        private BoxCollider2D waterCollider;
        private float visualWidth = 36f;

        public float CurrentWaterY => currentWaterY;
        public float TargetWaterY => targetWaterY;
        public bool IsHigh => Mathf.Approximately(targetWaterY, highWaterY);

        private static readonly List<WaterLevelZone> activeZones = new List<WaterLevelZone>();

        public static WaterLevelZone GetActiveZone()
        {
            return activeZones.Count > 0 ? activeZones[0] : null;
        }

        private void Awake()
        {
            currentWaterY = startsHigh ? highWaterY : lowWaterY;
            targetWaterY = currentWaterY;
            surfaceVisual = transform.Find("WaterSurfaceVisual");
            bodyVisual = transform.Find("WaterBodyVisual");
            if (bodyVisual != null) bodyRenderer = bodyVisual.GetComponent<SpriteRenderer>();
            waterCollider = GetComponent<BoxCollider2D>();
            if (waterCollider != null)
            {
                waterCollider.isTrigger = true;
            }
            UpdateVisualAndCollider();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckLethalContact(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            CheckLethalContact(collision);
        }

        private void CheckLethalContact(Collider2D collision)
        {
            if (collision == null || collision.isTrigger) return;

            // Only trigger lethal drowning for Player and Shadow characters
            Entities.PlayerController player = collision.GetComponent<Entities.PlayerController>();
            Entities.ShadowController shadow = collision.GetComponent<Entities.ShadowController>();

            if (player == null && shadow == null) return;

            Rigidbody2D rb = collision.attachedRigidbody;
            Bounds charBounds = collision.bounds;

            // 1. SAFE STATE: Standing on or jumping from a BuoyantPlatform2D
            // If the character is aboard or jumping away from a buoyant platform, they are riding / disembarking safely.
            Collider2D[] broadSafetyHits = Physics2D.OverlapBoxAll(new Vector2(charBounds.center.x, charBounds.min.y + 0.15f), new Vector2(charBounds.size.x * 1.3f, 0.6f), 0f);
            foreach (Collider2D bHit in broadSafetyHits)
            {
                if (bHit == null || bHit == collision || bHit.isTrigger) continue;
                if (bHit.GetComponent<BuoyantPlatform2D>() != null || bHit.GetComponentInParent<BuoyantPlatform2D>() != null)
                {
                    // Character's feet are above or near the top surface of the boat/buoy
                    if (charBounds.min.y >= bHit.bounds.min.y - 0.05f)
                    {
                        return; // 100% safe on/above buoyant platform
                    }
                }
            }

            // 2. SAFE STATE: Valid platform support underneath feet
            Vector2 footCheckCenter = new Vector2(charBounds.center.x, charBounds.min.y - 0.04f);
            Vector2 footCheckSize = new Vector2(charBounds.size.x * 0.9f, 0.18f);
            Collider2D[] footHits = Physics2D.OverlapBoxAll(footCheckCenter, footCheckSize, 0f);
            bool isSafelySupported = false;
            Collider2D supportingCol = null;
            foreach (Collider2D hit in footHits)
            {
                if (hit == null || hit == collision || hit.isTrigger) continue;

                if (hit.GetComponent<BuoyantPlatform2D>() != null ||
                    hit.GetComponentInParent<BuoyantPlatform2D>() != null ||
                    hit.GetComponent<IMovingSurface>() != null ||
                    hit.GetComponent<MovableBlock>() != null ||
                    hit.name.StartsWith("PlatCol") ||
                    hit.name.StartsWith("FloorCollider") ||
                    !hit.isTrigger)
                {
                    if (hit.bounds.max.y >= charBounds.min.y - 0.20f)
                    {
                        isSafelySupported = true;
                        supportingCol = hit;
                        break;
                    }
                }
            }

            if (player != null && player.IsGrounded && isSafelySupported) return;
            if (shadow != null && shadow.IsGrounded && isSafelySupported) return;
            if (isSafelySupported) return;

            // 3. SAFE STATE: Jumping / ascending upward away from a surface
            // When jumping, vertical velocity is strongly positive (Player jumpForce=10.6, Shadow jumpForce=7.8).
            // A character moving upward (rb.linearVelocity.y > 0.05f) is ascending into the air, not plunging into water.
            if (rb != null && rb.linearVelocity.y > 0.05f)
            {
                return; // Jumping / rising through the air is completely safe
            }

            // 4. SAFE STATE: Character is entirely above the water surface
            // The character only drowns if they have physically penetrated below the water surface line.
            // When descending (linearVelocity.y <= 0.05f), feet must fall into the water (charBounds.min.y < currentWaterY - 0.06f).
            if (charBounds.min.y >= currentWaterY - 0.04f)
            {
                return; // Still above water level
            }

            // 5. LETHAL STATE: Falling downward into exposed water without support
            int lvl = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelIndex + 1 : -1;
            Vector2 vel = rb != null ? rb.linearVelocity : Vector2.zero;
            bool grounded = (player != null && player.IsGrounded) || (shadow != null && shadow.IsGrounded);
            string colName = supportingCol != null ? supportingCol.name : "None";

            Debug.Log($"[DEATH_DIAGNOSTIC] Level={lvl}, Frame={Time.frameCount}, Character={collision.name}, Method=WaterLevelZone.CheckLethalContact, " +
                      $"Pos={collision.transform.position}, Vel={vel}, BoundsMinY={charBounds.min.y:F3}, WaterY={currentWaterY:F3}, " +
                      $"Grounded={grounded}, SupportingCol={colName}, Reason=SubmergedInExposedWater");

            if (Combat.HealthSystem.Instance != null && !Combat.HealthSystem.Instance.IsDead)
            {
                if (Core.AudioManager.Instance != null)
                {
                    Core.AudioManager.Instance.PlaySFX("puzzle_complete", 0.7f, 0.4f);
                }
                Combat.HealthSystem.Instance.KillInstantly();
            }
        }

        private void OnEnable()
        {
            if (!activeZones.Contains(this))
                activeZones.Add(this);
        }

        private void OnDisable()
        {
            activeZones.Remove(this);
        }

        private void Update()
        {
            // Visual water surface wave shimmer
            if (surfaceVisual != null)
            {
                float wave = Mathf.Sin(Time.time * 2.2f + transform.position.x) * 0.04f;
                surfaceVisual.position = new Vector3(transform.position.x, currentWaterY + wave, transform.position.z);
            }
        }

        private void FixedUpdate()
        {
            if (!Mathf.Approximately(currentWaterY, targetWaterY))
            {
                currentWaterY = Mathf.MoveTowards(currentWaterY, targetWaterY, transitionSpeed * Time.fixedDeltaTime);
                UpdateVisualAndCollider();
            }
        }

        public void SetTideState(bool high)
        {
            targetWaterY = high ? highWaterY : lowWaterY;
        }

        public void ToggleTide()
        {
            SetTideState(!IsHigh);
        }

        public void Configure(float lowY, float highY, bool highAtStart, float speed = 2.5f, float width = 36f)
        {
            lowWaterY = lowY;
            highWaterY = highY;
            startsHigh = highAtStart;
            transitionSpeed = speed;
            visualWidth = width;
            currentWaterY = startsHigh ? highWaterY : lowWaterY;
            targetWaterY = currentWaterY;
            UpdateVisualAndCollider();
        }

        private void UpdateVisualAndCollider()
        {
            float floorBottomY = -5.0f;
            float bodyDepth = Mathf.Max(0.2f, currentWaterY - floorBottomY);

            if (surfaceVisual != null)
            {
                surfaceVisual.position = new Vector3(transform.position.x, currentWaterY, transform.position.z);
            }

            if (bodyVisual != null)
            {
                bodyVisual.position = new Vector3(transform.position.x, currentWaterY - (bodyDepth * 0.5f), transform.position.z);
                bodyVisual.localScale = new Vector3(visualWidth, bodyDepth, 1f);
            }

            if (waterCollider != null)
            {
                waterCollider.isTrigger = true;
                // Water lethal trigger strictly covers the body of water from surface down to floor
                float lethalHeight = Mathf.Max(0.1f, currentWaterY - floorBottomY);
                waterCollider.size = new Vector2(visualWidth, lethalHeight);
                waterCollider.offset = new Vector2(0f, currentWaterY - transform.position.y - (lethalHeight * 0.5f));
            }
        }
    }
}
