using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Heavy stone floor plate activated by the Protagonist, Shadow, or MovableBlock standing on it.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class PressurePlate : PuzzleNode
    {
        [SerializeField] private bool stayPressedOnceActivated;

        private SpriteRenderer _sr;
        private BoxCollider2D _trigger;
        private Sprite _upSprite;
        private Sprite _downSprite;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = 3;
            _upSprite = AssetRegistry.GetSprite("Environment/pressure_plate_up");
            _downSprite = AssetRegistry.GetSprite("Environment/pressure_plate_down");
            _sr.sprite = _upSprite;

            _trigger = GetComponent<BoxCollider2D>();
            _trigger.isTrigger = true;
            _trigger.size = new Vector2(0.75f, 0.35f);
            _trigger.offset = new Vector2(0f, -0.05f);
        }

        private void FixedUpdate()
        {
            EvaluateOccupancy();
        }

        public void EvaluateOccupancy()
        {
            if (stayPressedOnceActivated && isActivated) return;

            Bounds b = _trigger.bounds;
            Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f);
            bool occupied = false;
            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit == _trigger || hit.isTrigger) continue;
                if (hit.GetComponent<PlayerController>() != null ||
                    hit.GetComponent<ShadowController>() != null ||
                    hit.GetComponent<MovableBlock>() != null)
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
            }
        }
    }
}
