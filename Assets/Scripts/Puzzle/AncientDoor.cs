using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Heavy carved stone door that opens when its required PuzzleNodes are activated.
    /// Supports both AND logic (all nodes active) and OR logic (any node active).
    /// Renders visual rune conduit lines connecting each required mechanism to the door
    /// so the player can clearly read puzzle dependencies and see which locks are active.
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

        private readonly List<LineRenderer> _conduitLines = new List<LineRenderer>();

        public bool IsOpen => _isOpen;
        public IReadOnlyList<PuzzleNode> RequiredNodes => requiredNodes;

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

            RebuildConduitLines();
            EvaluateDoorState();
        }

        private void RebuildConduitLines()
        {
            for (int i = 0; i < _conduitLines.Count; i++)
            {
                if (_conduitLines[i] != null)
                {
                    if (Application.isPlaying) Destroy(_conduitLines[i].gameObject);
                    else DestroyImmediate(_conduitLines[i].gameObject);
                }
            }
            _conduitLines.Clear();

            Shader lineShader = Shader.Find("Sprites/Default");
            for (int i = 0; i < requiredNodes.Count; i++)
            {
                PuzzleNode node = requiredNodes[i];
                if (node == null) continue;

                GameObject lineObj = new GameObject($"RuneConduit_{i}");
                lineObj.transform.SetParent(transform, false);
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 3;
                lr.startWidth = 0.045f;
                lr.endWidth = 0.045f;
                lr.sortingOrder = 1;
                if (lineShader != null)
                {
                    lr.material = new Material(lineShader);
                }

                _conduitLines.Add(lr);
            }

            UpdateConduitVisuals();
        }

        private void UpdateConduitVisuals()
        {
            for (int i = 0; i < _conduitLines.Count && i < requiredNodes.Count; i++)
            {
                LineRenderer lr = _conduitLines[i];
                PuzzleNode node = requiredNodes[i];
                if (lr == null || node == null) continue;

                Vector3 start = node.transform.position + new Vector3(0f, -0.2f, 0f);
                Vector3 end = transform.position + new Vector3(0f, (i - 0.5f) * 0.55f, 0f);
                // Orthogonal stepped rune path along masonry
                Vector3 mid = new Vector3(end.x - 1.2f, start.y, 0f);

                lr.SetPosition(0, start);
                lr.SetPosition(1, mid);
                lr.SetPosition(2, end);

                bool active = node.IsActivated;
                bool isShadowNode = node is ShadowSwitch;

                Color col = active
                    ? (isShadowNode ? new Color(0.78f, 0.52f, 1f, 0.85f) : new Color(1f, 0.82f, 0.32f, 0.85f))
                    : new Color(0.32f, 0.36f, 0.52f, 0.32f);

                lr.startColor = col;
                lr.endColor = col;
            }
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
            UpdateConduitVisuals();
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
