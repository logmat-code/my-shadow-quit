using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Ethereal shadow vine lattice for World 2 ("The Inverted Forest").
    /// Permeable exclusively by the Shadow (which passes through unimpeded),
    /// while solidly barring the physical Protagonist and MovableBlocks.
    /// Can optionally be dissolved by completing linked puzzle mechanisms.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ShadowVinePassage : MonoBehaviour
    {
        [SerializeField] private List<PuzzleNode> unlockNodes = new List<PuzzleNode>();
        [SerializeField] private bool stayOpenOnceUnlocked = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _shadowIgnored;
        private bool _isOpen;

        public bool IsOpen => _isOpen;
        public bool IsBlockingProtagonist => _col != null && _col.enabled && !_isOpen;

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
                    _sr.sprite = AssetRegistry.GetSprite("World2/shadow_vine", 32f);
                }
            }

            if (_col != null)
            {
                _col.size = new Vector2(1.0f, 2.0f);
                _col.isTrigger = false;
            }

            EnsureShadowCollisionIgnored();
        }

        private Collider2D _lastShadowCollider;

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

            if (_col != null)
            {
                _col.enabled = !_isOpen;
            }

            if (_sr != null)
            {
                _sr.color = _isOpen ? new Color(1f, 1f, 1f, 0.2f) : Color.white;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_isOpen ? "shadow_shift" : "stone_door", 0.6f, 1.15f);
            }
        }
    }
}
