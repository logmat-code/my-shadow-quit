using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Mechanical ratchet clockwork switch.
    /// Can be activated by either the Protagonist or Shadow.
    /// Supports both permanent latching and timed spring-winding activation (e.g. 5-7s)
    /// to open ancient doors, start gear mechanisms, or temporarily freeze pendulums.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ClockworkSwitch : PuzzleNode
    {
        [Header("Clockwork Settings")]
        [SerializeField] private bool isTimed = true;
        [SerializeField] private float timerDuration = 6.0f;
        [SerializeField] private AncientDoor targetDoor;
        [SerializeField] private List<PendulumObstacle> linkedPendulums = new List<PendulumObstacle>();
        [SerializeField] private List<RotatingGearPlatform> linkedGears = new List<RotatingGearPlatform>();

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private float _remainingTimer;
        private bool _isPressed;
        private Sprite _spriteOff;
        private Sprite _spriteOn;

        public float RemainingTimer => _remainingTimer;
        public float TimerDuration => timerDuration;

        public void Configure(
            bool timed,
            float duration = 6.0f,
            AncientDoor door = null,
            List<PendulumObstacle> pendulums = null,
            List<RotatingGearPlatform> gears = null)
        {
            isTimed = timed;
            timerDuration = duration;
            targetDoor = door;
            if (pendulums != null) linkedPendulums = pendulums;
            if (gears != null) linkedGears = gears;

            EnsureInitialized();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            _spriteOff = AssetRegistry.GetSprite("World3/clockwork_switch_off", 32f);
            _spriteOn = AssetRegistry.GetSprite("World3/clockwork_switch_on", 32f);

            if (_sr != null)
            {
                _sr.sortingOrder = 5;
                _sr.sprite = isActivated ? _spriteOn : _spriteOff;
            }

            if (_col != null)
            {
                _col.size = new Vector2(0.9f, 0.9f);
                _col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryActivate(other);
        private void OnTriggerStay2D(Collider2D other) => TryActivate(other);

        private void TryActivate(Collider2D other)
        {
            if (other == null) return;
            bool isPlayer = other.GetComponent<PlayerController>() != null || other.CompareTag("Player");
            bool isShadow = other.GetComponent<ShadowController>() != null || other.CompareTag("Shadow");

            if (!isPlayer && !isShadow) return;

            ActivateMechanism();
        }

        public void ActivateMechanism()
        {
            if (isActivated && (!isTimed || _remainingTimer > timerDuration * 0.85f))
            {
                return;
            }

            _remainingTimer = timerDuration;
            SetActivated(true);

            if (_sr != null && _spriteOn != null)
            {
                _sr.sprite = _spriteOn;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("clockwork_switch", 0.85f, 1.0f);
            }

            ApplyLinkedTargets(true);
        }

        private void Update()
        {
            if (!isActivated || !isTimed) return;

            _remainingTimer -= Time.deltaTime;
            if (_remainingTimer <= 0f)
            {
                _remainingTimer = 0f;
                SetActivated(false);

                if (_sr != null && _spriteOff != null)
                {
                    _sr.sprite = _spriteOff;
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("clockwork_switch", 0.65f, 0.82f);
                }

                ApplyLinkedTargets(false);
            }
        }

        private void ApplyLinkedTargets(bool active)
        {
            if (targetDoor != null)
            {
                if (active) targetDoor.Open();
                else targetDoor.Close();
            }

            foreach (var p in linkedPendulums)
            {
                if (p != null) p.SetFrozen(active);
            }

            foreach (var g in linkedGears)
            {
                if (g != null) g.SetPaused(!active);
            }
        }

        protected override void OnVisualStateUpdated(bool active)
        {
            if (_sr != null)
            {
                _sr.sprite = active ? _spriteOn : _spriteOff;
            }
        }
    }
}
