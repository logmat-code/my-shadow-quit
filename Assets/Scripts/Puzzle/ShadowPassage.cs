using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Traversable shadow lattice barrier.
    /// Blocks the physical Protagonist (and MovableBlocks) while closed, allowing the Shadow to pass freely.
    /// Can be linked to PuzzleNodes (such as a ShadowSwitch on the far side) so that activating the mechanism
    /// dissolves/opens the violet barrier with a smooth visual & physical transition, allowing the Protagonist through.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ShadowPassage : MonoBehaviour
    {
        [SerializeField] private List<PuzzleNode> unlockNodes = new List<PuzzleNode>();
        [SerializeField] private bool requireAllNodes = true;
        [SerializeField] private bool stayOpenOnceUnlocked = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _shadowIgnored;
        private bool _isOpen;
        private Vector3 _initialScale = Vector3.one;

        public bool IsOpen => _isOpen;
        public bool IsBlockingProtagonist => _col != null && _col.enabled && !_isOpen;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            if (_sr != null)
            {
                _sr.sortingOrder = 6;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("Environment/shadow_passage");
                }
            }

            if (_col != null)
            {
                _col.size = new Vector2(0.88f, 1.98f);
                _col.isTrigger = false;
            }

            _initialScale = transform.localScale;
            EnsureShadowCollisionIgnored();
        }

        public void EnsureShadowCollisionIgnored()
        {
            if (!_shadowIgnored && _col != null)
            {
                ShadowController shadow = FindFirstObjectByType<ShadowController>();
                if (shadow != null && shadow.Collider != null)
                {
                    Physics2D.IgnoreCollision(_col, shadow.Collider, true);
                    _shadowIgnored = true;
                }
            }
        }

        public void ConfigureUnlockNodes(List<PuzzleNode> nodes, bool requireAll = true, bool latchOpen = true)
        {
            EnsureInitialized();
            foreach (PuzzleNode oldNode in unlockNodes)
            {
                if (oldNode != null) oldNode.OnStateChanged -= OnNodeStateChanged;
            }

            unlockNodes = nodes ?? new List<PuzzleNode>();
            requireAllNodes = requireAll;
            stayOpenOnceUnlocked = latchOpen;

            foreach (PuzzleNode node in unlockNodes)
            {
                if (node != null) node.OnStateChanged += OnNodeStateChanged;
            }

            EvaluateUnlockState();
        }

        private void OnEnable()
        {
            foreach (PuzzleNode node in unlockNodes)
            {
                if (node != null)
                {
                    node.OnStateChanged -= OnNodeStateChanged;
                    node.OnStateChanged += OnNodeStateChanged;
                }
            }
        }

        private void OnDisable()
        {
            foreach (PuzzleNode node in unlockNodes)
            {
                if (node != null) node.OnStateChanged -= OnNodeStateChanged;
            }
        }

        private void OnNodeStateChanged(PuzzleNode node, bool active)
        {
            EvaluateUnlockState();
        }

        public void EvaluateUnlockState()
        {
            if (stayOpenOnceUnlocked && _isOpen) return;
            if (unlockNodes == null || unlockNodes.Count == 0) return;

            bool shouldOpen = requireAllNodes;
            foreach (PuzzleNode node in unlockNodes)
            {
                if (node == null) continue;
                if (requireAllNodes && !node.IsActivated)
                {
                    shouldOpen = false;
                    break;
                }
                if (!requireAllNodes && node.IsActivated)
                {
                    shouldOpen = true;
                    break;
                }
            }

            SetOpen(shouldOpen);
        }

        public void SetOpen(bool open)
        {
            EnsureInitialized();
            if (_isOpen == open) return;
            _isOpen = open;

            if (_col != null)
            {
                _col.enabled = !_isOpen;
            }

            if (_sr != null)
            {
                // Immediate visual state update so EditMode tests and PlayMode frames reflect openness right away
                _sr.color = _isOpen
                    ? new Color(0.75f, 0.50f, 1f, 0.14f)
                    : new Color(1f, 1f, 1f, 0.88f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("shadow_shift", 0.75f, _isOpen ? 1.18f : 0.9f);
            }
        }

        private void Update()
        {
            if (!_shadowIgnored && _col != null)
            {
                ShadowController shadow = FindFirstObjectByType<ShadowController>();
                if (shadow != null && shadow.Collider != null)
                {
                    Physics2D.IgnoreCollision(_col, shadow.Collider, true);
                    _shadowIgnored = true;
                }
            }

            if (_sr != null)
            {
                if (_isOpen)
                {
                    // Dissolve violet barrier visually when unlocked so the Protagonist clearly sees the open path
                    Color c = _sr.color;
                    c.a = Mathf.MoveTowards(c.a, 0.12f, Time.deltaTime * 3.5f);
                    _sr.color = c;

                    Vector3 targetScale = new Vector3(_initialScale.x * 0.35f, _initialScale.y, _initialScale.z);
                    transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 6f);
                }
                else
                {
                    float alpha = 0.82f + 0.12f * Mathf.Sin(Time.time * 3.5f);
                    _sr.color = new Color(1f, 1f, 1f, alpha);
                    transform.localScale = Vector3.Lerp(transform.localScale, _initialScale, Time.deltaTime * 6f);
                }
            }
        }
    }
}
