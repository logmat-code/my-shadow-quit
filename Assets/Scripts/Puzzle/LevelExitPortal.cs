using System;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Sanctuary exit archway. Requires the Protagonist (and optionally the Shadow on the final level)
    /// to enter once the level's exit door is unlocked.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class LevelExitPortal : MonoBehaviour
    {
        public static event Action OnLevelExitReached;

        public const float ReconnectRadius = 3.5f;

        [SerializeField] private AncientDoor guardingDoor;
        [SerializeField] private bool requireReconnectedShadow;

        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private bool _triggered;

        public bool IsTriggered => _triggered;
        public AncientDoor GuardingDoor => guardingDoor;
        public bool RequireReconnectedShadow => requireReconnectedShadow;

        public void Configure(AncientDoor door, bool requireShadowNear = false)
        {
            EnsureInitialized();
            guardingDoor = door;
            requireReconnectedShadow = requireShadowNear;
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
                _sr.sortingOrder = 2;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("Environment/exit_sanctuary");
                }
            }

            if (_trigger != null)
            {
                _trigger.isTrigger = true;
                _trigger.size = new Vector2(1.8f, 2.2f);
            }
        }

        private void Update()
        {
            if (!_triggered && (guardingDoor == null || guardingDoor.IsOpen))
            {
                EvaluatePortalProximity();
            }
        }

        private void FixedUpdate()
        {
            if (!_triggered && (guardingDoor == null || guardingDoor.IsOpen))
            {
                EvaluatePortalProximity();
            }
        }

        public void EvaluatePortalProximity()
        {
            if (_triggered) return;
            if (guardingDoor != null && !guardingDoor.IsOpen) return;

            PlayerController player = (GameManager.Instance != null && GameManager.Instance.Levels != null && GameManager.Instance.Levels.Player != null)
                ? GameManager.Instance.Levels.Player
                : FindFirstObjectByType<PlayerController>();

            if (player == null) return;

            bool playerAtArch = IsCharacterAtArch(player.transform.position);

            if (!requireReconnectedShadow)
            {
                if (playerAtArch)
                {
                    TryCompleteLevel(player);
                }
                return;
            }

            ShadowController shadow = (GameManager.Instance != null && GameManager.Instance.Levels != null && GameManager.Instance.Levels.Shadow != null)
                ? GameManager.Instance.Levels.Shadow
                : FindFirstObjectByType<ShadowController>();

            bool shadowAtArch = shadow != null && IsCharacterAtArch(shadow.transform.position);

            // Trigger reunion when either character is inside the Sanctuary Arch and the other is within ReconnectRadius
            if (playerAtArch || shadowAtArch)
            {
                TryCompleteLevel(player);
            }
        }

        public bool IsCharacterAtArch(Vector2 charPos)
        {
            Vector2 portalPos = transform.position;
            return Mathf.Abs(charPos.x - portalPos.x) <= 1.45f && Mathf.Abs(charPos.y - portalPos.y) <= 1.65f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCompleteLevelWithCollider(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCompleteLevelWithCollider(other);
        }

        public bool TryCompleteLevel(PlayerController player)
        {
            if (_triggered || player == null) return false;
            if (guardingDoor != null && !guardingDoor.IsOpen) return false;

            if (requireReconnectedShadow)
            {
                ShadowController shadow = (GameManager.Instance != null && GameManager.Instance.Levels != null && GameManager.Instance.Levels.Shadow != null)
                    ? GameManager.Instance.Levels.Shadow
                    : FindFirstObjectByType<ShadowController>();

                if (shadow != null && Vector2.Distance(player.transform.position, shadow.transform.position) > ReconnectRadius)
                {
                    return false;
                }
            }

            _triggered = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("puzzle_complete", 0.95f, 1.0f);
            }
            OnLevelExitReached?.Invoke();
            return true;
        }

        private void TryCompleteLevelWithCollider(Collider2D other)
        {
            if (_triggered || other == null) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                TryCompleteLevel(player);
                return;
            }

            // If the Shadow enters the Sanctuary Arch while the Protagonist is already waiting nearby
            // (whose Rigidbody2D may be sleeping), evaluate level completion using the Protagonist!
            ShadowController shadow = other.GetComponent<ShadowController>();
            if (shadow != null)
            {
                PlayerController existingPlayer = (GameManager.Instance != null && GameManager.Instance.Levels != null && GameManager.Instance.Levels.Player != null)
                    ? GameManager.Instance.Levels.Player
                    : FindFirstObjectByType<PlayerController>();

                if (existingPlayer != null && (IsCharacterAtArch(existingPlayer.transform.position) || IsCharacterAtArch(shadow.transform.position)))
                {
                    TryCompleteLevel(existingPlayer);
                }
            }
        }
    }
}
