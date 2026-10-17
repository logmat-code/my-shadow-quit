using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Heavy carved stone door that opens when its required PuzzleNodes are activated.
    /// Supports both AND logic (all nodes active) and OR logic (any node active).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class AncientDoor : MonoBehaviour
    {
        [SerializeField] private List<PuzzleNode> requiredNodes = new List<PuzzleNode>();
        [SerializeField] private bool requireAllNodes = true;
        [SerializeField] private bool stayOpenOnceUnlocked = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private Sprite _closedSprite;
        private Sprite _openSprite;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();
            if (_closedSprite == null) _closedSprite = AssetRegistry.GetSprite("Environment/ancient_door_closed");
            if (_openSprite == null) _openSprite = AssetRegistry.GetSprite("Environment/ancient_door_open");

            if (_sr != null)
            {
                _sr.sortingOrder = 5;
                if (_sr.sprite == null) _sr.sprite = _closedSprite;
            }
            if (_col != null)
            {
                _col.size = new Vector2(0.85f, 2.95f);
                _col.isTrigger = false;
            }
        }

        public void Configure(List<PuzzleNode> nodes, bool requireAll = true, bool latchOpen = true)
        {
            EnsureInitialized();
            foreach (PuzzleNode oldNode in requiredNodes)
            {
                if (oldNode != null) oldNode.OnStateChanged -= OnNodeChanged;
            }

            requiredNodes = nodes ?? new List<PuzzleNode>();
            requireAllNodes = requireAll;
            stayOpenOnceUnlocked = latchOpen;

            foreach (PuzzleNode node in requiredNodes)
            {
                if (node != null) node.OnStateChanged += OnNodeChanged;
            }

            EvaluateDoorState();
        }

        private void OnEnable()
        {
            foreach (PuzzleNode node in requiredNodes)
            {
                if (node != null)
                {
                    node.OnStateChanged -= OnNodeChanged;
                    node.OnStateChanged += OnNodeChanged;
                }
            }
        }

        private void OnDisable()
        {
            foreach (PuzzleNode node in requiredNodes)
            {
                if (node != null) node.OnStateChanged -= OnNodeChanged;
            }
        }

        private void OnNodeChanged(PuzzleNode node, bool active)
        {
            EvaluateDoorState();
        }

        public void EvaluateDoorState()
        {
            if (stayOpenOnceUnlocked && _isOpen) return;
            if (requiredNodes == null || requiredNodes.Count == 0) return;

            bool shouldOpen = requireAllNodes;
            foreach (PuzzleNode node in requiredNodes)
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
            if (_col != null) _col.enabled = !_isOpen;
            if (_sr != null) _sr.sprite = _isOpen ? _openSprite : _closedSprite;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("stone_door", 0.9f, _isOpen ? 1.0f : 0.85f);
            }
        }
    }
}
