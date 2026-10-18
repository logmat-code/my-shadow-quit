using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Spectral gothic iron grate for World 3 ("The Hollow Clocktower").
    /// Solid to the physical Protagonist, but permeable exclusively to the Shadow.
    /// Can optionally be unlocked and opened by linked puzzle mechanisms.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ShadowClockworkPassage : MonoBehaviour
    {
        [SerializeField] private List<PuzzleNode> unlockNodes = new List<PuzzleNode>();
        [SerializeField] private bool stayOpenOnceUnlocked = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _shadowIgnored;
        private bool _isOpen;
        private Collider2D _lastShadowCollider;

        public bool IsOpen => _isOpen;
        public bool IsBlockingProtagonist => _col != null && _col.enabled && !_isOpen;
        public bool IsShadowIgnored => _shadowIgnored;

        public void Configure(List<PuzzleNode> nodes = null, bool stayOpen = true)
        {
            if (nodes != null) unlockNodes = nodes;
            stayOpenOnceUnlocked = stayOpen;
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

            if (_sr != null)
            {
                _sr.sortingOrder = 6;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("World3/shadow_grate", 32f);
                }
            }

            if (_col != null)
            {
                _col.size = new Vector2(1.0f, 2.0f);
                _col.isTrigger = false;
            }

            EnsureShadowCollisionIgnored();
        }

        public void EnsureShadowCollisionIgnored()
        {
            if (_col == null) return;

            ShadowController shadow = (GameManager.Instance != null && GameManager.Instance.Levels != null && GameManager.Instance.Levels.Shadow != null)
                ? GameManager.Instance.Levels.Shadow
                : FindFirstObjectByType<ShadowController>();

            if (shadow != null)
            {
                Collider2D shadowCol = shadow.GetComponent<Collider2D>();
                if (shadowCol != null && shadowCol != _lastShadowCollider)
                {
                    Physics2D.IgnoreCollision(_col, shadowCol, true);
                    _lastShadowCollider = shadowCol;
                    _shadowIgnored = true;
                }
            }
        }

        private void Update()
        {
            EnsureShadowCollisionIgnored();
            EvaluateUnlockConditions();
        }

        public void ResetPassage()
        {
            _lastShadowCollider = null;
            _shadowIgnored = false;
            SetOpen(false);
            EnsureShadowCollisionIgnored();
        }

        private void EvaluateUnlockConditions()
        {
            if (_isOpen && stayOpenOnceUnlocked) return;
            if (unlockNodes == null || unlockNodes.Count == 0) return;

            bool allActive = true;
            foreach (PuzzleNode node in unlockNodes)
            {
                if (node == null || !node.IsActivated)
                {
                    allActive = false;
                    break;
                }
            }

            if (allActive && !_isOpen)
            {
                SetOpen(true);
            }
            else if (!allActive && _isOpen && !stayOpenOnceUnlocked)
            {
                SetOpen(false);
            }
        }

        public void SetOpen(bool open)
        {
            _isOpen = open;
            if (_col != null) _col.enabled = !open;
            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = open ? 0.25f : 1.0f;
                _sr.color = c;
            }

            if (open && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("stone_door", 0.75f, 1.25f);
            }
        }
    }
}
