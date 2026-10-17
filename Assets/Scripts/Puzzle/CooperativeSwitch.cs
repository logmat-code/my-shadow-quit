using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Cooperative dual-condition switch for World 2 ("The Inverted Forest").
    /// Requires synchronized cooperation between Protagonist and Shadow.
    /// Can be configured as a dual-occupancy plate or paired with a partner switch.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class CooperativeSwitch : PuzzleNode
    {
        [SerializeField] private CooperativeSwitch partnerSwitch;
        [SerializeField] private bool requireSimultaneousPress = true;
        [SerializeField] private float syncWindow = 1.2f;
        [SerializeField] private bool stayActivatedOnceCompleted = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private Sprite _idleSprite;
        private Sprite _activeSprite;

        private bool _isLocallyOccupied;
        private float _pressTimer;

        public bool IsLocallyOccupied => _isLocallyOccupied;

        public void Configure(CooperativeSwitch partner = null, bool simultaneous = true, bool stayActivated = true)
        {
            partnerSwitch = partner;
            requireSimultaneousPress = simultaneous;
            stayActivatedOnceCompleted = stayActivated;
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

            if (_idleSprite == null) _idleSprite = AssetRegistry.GetSprite("Environment/pressure_plate_up", 32f);
            if (_activeSprite == null) _activeSprite = AssetRegistry.GetSprite("Environment/pressure_plate_down", 32f);

            if (_sr != null)
            {
                _sr.sortingOrder = 3;
                _sr.sprite = isActivated ? _activeSprite : _idleSprite;
            }

            if (_trigger != null)
            {
                _trigger.isTrigger = true;
                _trigger.size = new Vector2(0.85f, 0.40f);
                _trigger.offset = new Vector2(0f, -0.05f);
            }
        }

        private void FixedUpdate()
        {
            EvaluateOccupancy();
        }

        public void EvaluateOccupancy()
        {
            if (isActivated && stayActivatedOnceCompleted) return;

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                (Vector2)transform.position + (_trigger != null ? _trigger.offset : Vector2.zero),
                _trigger != null ? _trigger.size : new Vector2(0.8f, 0.4f),
                0f
            );

            bool occupied = false;
            foreach (Collider2D hit in hits)
            {
                if (hit != null && !hit.isTrigger &&
                    (hit.GetComponent<PlayerController>() != null ||
                     hit.GetComponent<ShadowController>() != null ||
                     hit.GetComponent<MovableBlock>() != null))
                {
                    occupied = true;
                    break;
                }
            }

            _isLocallyOccupied = occupied;
            if (_isLocallyOccupied)
            {
                _pressTimer = syncWindow;
            }
            else if (_pressTimer > 0f)
            {
                _pressTimer -= Time.fixedDeltaTime;
            }

            // Check cooperative condition with partner switch
            bool partnerActive = false;
            if (partnerSwitch != null)
            {
                partnerActive = requireSimultaneousPress ? partnerSwitch.IsLocallyOccupied : partnerSwitch.IsRecentlyPressed;
            }
            else
            {
                // Self-contained dual check (e.g. both Player AND Shadow on this switch)
                int characterCount = 0;
                foreach (Collider2D hit in hits)
                {
                    if (hit != null && !hit.isTrigger)
                    {
                        if (hit.GetComponent<PlayerController>() != null) characterCount++;
                        if (hit.GetComponent<ShadowController>() != null) characterCount++;
                    }
                }
                partnerActive = (characterCount >= 2);
            }

            bool cooperativeSuccess = _isLocallyOccupied && partnerActive;

            if (cooperativeSuccess)
            {
                SetActivated(true);
                if (partnerSwitch != null && !partnerSwitch.IsActivated)
                {
                    partnerSwitch.SetActivated(true);
                }
            }
            else if (!stayActivatedOnceCompleted && isActivated)
            {
                SetActivated(false);
            }
        }

        public bool IsRecentlyPressed => _pressTimer > 0f || _isLocallyOccupied;

        protected override void OnVisualStateUpdated(bool active)
        {
            if (_sr != null)
            {
                _sr.sprite = active ? _activeSprite : _idleSprite;
                _sr.color = active ? new Color(0.3f, 1f, 0.7f) : Color.white;
            }

            if (active && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("plate_press", 0.75f, 1.15f);
            }
        }
    }
}
