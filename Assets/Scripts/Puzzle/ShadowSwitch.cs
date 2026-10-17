using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Violet obelisk mechanism that only the Shadow can activate (via E or touching when controlled).
    /// Optionally requires a nearby torch or darkness condition.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ShadowSwitch : PuzzleNode
    {
        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private Sprite _inactiveSprite;
        private Sprite _activeSprite;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = 4;
            _inactiveSprite = AssetRegistry.GetSprite("Environment/shadow_switch_inactive");
            _activeSprite = AssetRegistry.GetSprite("Environment/shadow_switch_active");
            _sr.sprite = _inactiveSprite;

            _trigger = GetComponent<BoxCollider2D>();
            _trigger.isTrigger = true;
            _trigger.size = new Vector2(0.85f, 1.35f);
        }

        public void ActivateByShadow(ShadowController shadow)
        {
            if (isActivated) return;
            SetActivated(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.9f, 1.08f);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (isActivated) return;
            ShadowController shadow = other.GetComponent<ShadowController>();
            if (shadow != null && shadow.IsControlActive && GameInput.GetInteractDown())
            {
                ActivateByShadow(shadow);
            }
        }

        protected override void OnVisualStateUpdated(bool active)
        {
            if (_sr != null)
            {
                _sr.sprite = active ? _activeSprite : _inactiveSprite;
            }
        }
    }
}
