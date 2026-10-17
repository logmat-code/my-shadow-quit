using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Checkpoint brazier that ignites when the Protagonist passes it, storing respawn coordinates.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class Checkpoint : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private Sprite _inactiveSprite;
        private Sprite _activeSprite;
        private bool _isActivated;

        public bool IsActivated => _isActivated;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            _sr.sortingOrder = 3;

            _inactiveSprite = AssetRegistry.GetSprite("Environment/checkpoint_inactive");
            _activeSprite = AssetRegistry.GetSprite("Environment/checkpoint_active");
            _sr.sprite = _inactiveSprite;

            _trigger.isTrigger = true;
            _trigger.size = new Vector2(1.0f, 1.4f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isActivated) return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                _isActivated = true;
                _sr.sprite = _activeSprite;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("torch_ignite", 0.8f, 1.15f);
                }
            }
        }
    }
}
