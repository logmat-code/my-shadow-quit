using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    public enum CooperativeMode
    {
        BothPresent,       // Mode A: Both characters must occupy partner switches simultaneously
        LatchedActivation, // Mode B: Stepping on switch permanently latches it
        TimedActivation    // Mode C: Generous timer (e.g. 4.5s) with visual pulse/flicker feedback
    }

    /// <summary>
    /// Cooperative dual-condition switch for World 2 ("The Inverted Forest").
    /// Requires synchronized cooperation between Protagonist and Shadow.
    /// Can be configured as BothPresent, LatchedActivation, or TimedActivation.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class CooperativeSwitch : PuzzleNode
    {
        [SerializeField] private CooperativeSwitch partnerSwitch;
        [SerializeField] private CooperativeMode activationMode = CooperativeMode.BothPresent;
        [SerializeField] private float timedDuration = 4.5f;
        [SerializeField] private bool stayActivatedOnceCompleted = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private Sprite _idleSprite;
        private Sprite _activeSprite;

        private bool _isLocallyOccupied;
        private float _pressTimer;
        private bool _isLatched;

        public bool IsLocallyOccupied => _isLocallyOccupied;
        public bool IsLatched => _isLatched;
        public CooperativeMode Mode => activationMode;
        public float RemainingTimer => _pressTimer;

        public void Configure(CooperativeSwitch partner = null, CooperativeMode mode = CooperativeMode.BothPresent, float timer = 4.5f, bool stayActivated = true)
        {
            partnerSwitch = partner;
            activationMode = mode;
            timedDuration = timer;
            stayActivatedOnceCompleted = stayActivated;
            EnsureInitialized();
        }

        public void Configure(CooperativeSwitch partner, bool simultaneous, bool stayActivated = true)
        {
            Configure(partner, simultaneous ? CooperativeMode.BothPresent : CooperativeMode.TimedActivation, 4.5f, stayActivated);
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

        private void Update()
        {
            // Visual timer feedback for Timed Activation mode
            if (!isActivated && activationMode == CooperativeMode.TimedActivation && _pressTimer > 0f && _sr != null)
            {
                if (_pressTimer > 1.5f)
                {
                    // Gentle rhythmic jade pulse
                    float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 8f);
                    _sr.color = Color.Lerp(Color.white, new Color(0.35f, 1f, 0.75f), pulse);
                }
                else
                {
                    // Rapid warning flicker when timer is running out
                    float flicker = (Mathf.Sin(Time.time * 22f) > 0f) ? 1f : 0.4f;
                    _sr.color = Color.Lerp(new Color(1f, 0.85f, 0.3f), Color.white, flicker);
                }
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
            int characterCount = 0;
            foreach (Collider2D hit in hits)
            {
                if (hit != null && !hit.isTrigger)
                {
                    bool isPlayer = hit.GetComponent<PlayerController>() != null;
                    bool isShadow = hit.GetComponent<ShadowController>() != null;
                    bool isBlock = hit.GetComponent<MovableBlock>() != null;

                    if (isPlayer || isShadow || isBlock)
                    {
                        occupied = true;
                    }
                    if (isPlayer) characterCount++;
                    if (isShadow) characterCount++;
                }
            }

            _isLocallyOccupied = occupied;

            if (_isLocallyOccupied)
            {
                _pressTimer = timedDuration;
                _isLatched = true;
            }
            else if (_pressTimer > 0f)
            {
                _pressTimer -= Time.fixedDeltaTime;
            }

            bool cooperativeSuccess = false;

            switch (activationMode)
            {
                case CooperativeMode.BothPresent:
                    if (partnerSwitch != null)
                    {
                        cooperativeSuccess = _isLocallyOccupied && partnerSwitch.IsLocallyOccupied;
                    }
                    else
                    {
                        cooperativeSuccess = (characterCount >= 2);
                    }
                    break;

                case CooperativeMode.LatchedActivation:
                    if (partnerSwitch != null)
                    {
                        cooperativeSuccess = _isLatched && partnerSwitch.IsLatched;
                    }
                    else
                    {
                        cooperativeSuccess = _isLatched;
                    }
                    break;

                case CooperativeMode.TimedActivation:
                    bool partnerTimedActive = (partnerSwitch != null)
                        ? (partnerSwitch.IsLocallyOccupied || partnerSwitch.IsRecentlyPressed)
                        : true;
                    cooperativeSuccess = (_isLocallyOccupied || _pressTimer > 0f) && partnerTimedActive;
                    break;
            }

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

        public void ResetSwitch()
        {
            _isLocallyOccupied = false;
            _pressTimer = 0f;
            _isLatched = false;
            SetActivated(false);
            if (_sr != null)
            {
                _sr.sprite = _idleSprite;
                _sr.color = Color.white;
            }
        }

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
