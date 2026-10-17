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

        [SerializeField] private AncientDoor guardingDoor;
        [SerializeField] private bool requireReconnectedShadow;

        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private bool _triggered;

        public void Configure(AncientDoor door, bool requireShadowNear = false)
        {
            guardingDoor = door;
            requireReconnectedShadow = requireShadowNear;
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            _sr.sortingOrder = 2;
            _sr.sprite = AssetRegistry.GetSprite("Environment/exit_sanctuary");

            _trigger.isTrigger = true;
            _trigger.size = new Vector2(1.2f, 2.0f);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_triggered) return;
            if (guardingDoor != null && !guardingDoor.IsOpen) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (requireReconnectedShadow)
                {
                    ShadowController shadow = FindFirstObjectByType<ShadowController>();
                    if (shadow != null && Vector2.Distance(player.transform.position, shadow.transform.position) > 2.5f)
                    {
                        return;
                    }
                }

                _triggered = true;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("puzzle_complete", 0.95f, 1.0f);
                }
                OnLevelExitReached?.Invoke();
            }
        }
    }
}
