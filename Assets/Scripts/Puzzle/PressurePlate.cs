using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    public enum PressurePlateAllowedActivator
    {
        Any,               // Default: Protagonist, Shadow, or MovableBlock
        ProtagonistOnly,   // Plate A: Only Protagonist (amber tint/glow)
        ShadowOnly         // Plate B: Only Shadow (violet tint/glow)
    }

    /// <summary>
    /// Heavy stone floor plate activated by the Protagonist, Shadow, or MovableBlock standing on it.
    /// Supports character-specific filtering and visual feedback (amber for Protagonist, violet for Shadow).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class PressurePlate : PuzzleNode
    {
        [SerializeField] private bool stayPressedOnceActivated;
        [SerializeField] private PressurePlateAllowedActivator allowedActivator = PressurePlateAllowedActivator.Any;

        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private Sprite _upSprite;
        private Sprite _downSprite;

        public bool StayPressedOnceActivated => stayPressedOnceActivated;
        public PressurePlateAllowedActivator AllowedActivator => allowedActivator;

        public void Configure(bool shouldLatch, PressurePlateAllowedActivator activator = PressurePlateAllowedActivator.Any)
        {
            stayPressedOnceActivated = shouldLatch;
            allowedActivator = activator;
            EnsureInitialized();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_trigger == null) _trigger = GetComponent<BoxCollider2D>();

            if (_sr != null)
            {
                _sr.sortingOrder = 3;
                if (_upSprite == null) _upSprite = AssetRegistry.GetSprite("Environment/pressure_plate_up");
                if (_downSprite == null) _downSprite = AssetRegistry.GetSprite("Environment/pressure_plate_down");
                if (_sr.sprite == null) _sr.sprite = isActivated ? _downSprite : _upSprite;
                ApplyColorTheme();
            }

            if (_trigger != null)
            {
                _trigger.isTrigger = true;
                // Height 0.65m with offset +0.15m ensures trigger bounds extend from plate base up to +0.475m
                // comfortably overlapping the character colliders standing on top of the plate/platform
                _trigger.size = new Vector2(0.9f, 0.65f);
                _trigger.offset = new Vector2(0f, 0.15f);
            }
        }

        private void FixedUpdate()
        {
            EvaluateOccupancy();
        }

        public void EvaluateOccupancy()
        {
            EnsureInitialized();
            if (stayPressedOnceActivated && isActivated) return;

            Bounds b = _trigger.bounds;
            Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f);
            bool occupied = false;
            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit == _trigger || hit.isTrigger) continue;

                bool isPlayer = hit.GetComponent<PlayerController>() != null || hit.GetComponentInParent<PlayerController>() != null;
                bool isShadow = hit.GetComponent<ShadowController>() != null || hit.GetComponentInParent<ShadowController>() != null;
                bool isBlock = hit.GetComponent<MovableBlock>() != null || hit.GetComponentInParent<MovableBlock>() != null;

                bool matchesActivator = false;
                switch (allowedActivator)
                {
                    case PressurePlateAllowedActivator.ProtagonistOnly:
                        matchesActivator = isPlayer;
                        break;
                    case PressurePlateAllowedActivator.ShadowOnly:
                        matchesActivator = isShadow;
                        break;
                    case PressurePlateAllowedActivator.Any:
                    default:
                        matchesActivator = isPlayer || isShadow || isBlock;
                        break;
                }

                if (matchesActivator)
                {
                    occupied = true;
                    break;
                }
            }

            if (occupied != isActivated)
            {
                SetActivated(occupied);
                if (occupied && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("switch_activate", 0.75f, 0.9f);
                }
            }
        }

        protected override void OnVisualStateUpdated(bool active)
        {
            if (_sr != null)
            {
                _sr.sprite = active ? _downSprite : _upSprite;
                ApplyColorTheme();
            }
        }

        private void ApplyColorTheme()
        {
            if (_sr == null) return;
            switch (allowedActivator)
            {
                case PressurePlateAllowedActivator.ProtagonistOnly:
                    // Amber illumination for Protagonist
                    _sr.color = isActivated ? new Color(1f, 0.85f, 0.35f, 1f) : new Color(1f, 0.70f, 0.25f, 0.90f);
                    break;
                case PressurePlateAllowedActivator.ShadowOnly:
                    // Violet illumination for Shadow
                    _sr.color = isActivated ? new Color(0.85f, 0.45f, 1f, 1f) : new Color(0.70f, 0.30f, 0.95f, 0.90f);
                    break;
                case PressurePlateAllowedActivator.Any:
                default:
                    _sr.color = Color.white;
                    break;
            }
        }
    }
}
