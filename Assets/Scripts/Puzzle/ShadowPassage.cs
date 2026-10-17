using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Traversable shadow lattice barrier.
    /// Blocks the physical Protagonist (and MovableBlocks), while allowing the Shadow to pass freely.
    /// Visually communicates its shadow-only nature via violet lattice runes.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ShadowPassage : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _shadowIgnored;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<BoxCollider2D>();
            _sr.sortingOrder = 6;
            _sr.sprite = AssetRegistry.GetSprite("Environment/shadow_passage");

            _col.size = new Vector2(0.88f, 1.98f);
            _col.isTrigger = false;
        }

        private void Update()
        {
            if (!_shadowIgnored)
            {
                ShadowController shadow = FindFirstObjectByType<ShadowController>();
                if (shadow != null && shadow.Collider != null)
                {
                    Physics2D.IgnoreCollision(_col, shadow.Collider, true);
                    _shadowIgnored = true;
                }
            }

            // Subtle violet shimmer
            if (_sr != null)
            {
                float alpha = 0.82f + 0.12f * Mathf.Sin(Time.time * 3.5f);
                _sr.color = new Color(1f, 1f, 1f, alpha);
            }
        }
    }
}
