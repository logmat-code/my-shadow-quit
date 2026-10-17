using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Shimmering celestial light bridge for World 2 ("The Inverted Forest").
    /// Starts unmanifested or dormant, and solidifies into a traversable energy walkway
    /// when powered by associated puzzle triggers (TorchPedestal, LightSwitch, PressurePlate).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class LightBridgeMechanism : MonoBehaviour
    {
        [SerializeField] private List<PuzzleNode> activatorNodes = new List<PuzzleNode>();
        [SerializeField] private bool requireAllActivators = true;
        [SerializeField] private bool stayActiveOnceTriggered = false;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _isActive;

        public bool IsActive => _isActive;

        public void Configure(List<PuzzleNode> nodes, bool requireAll = true, bool stayActive = false)
        {
            activatorNodes = nodes ?? new List<PuzzleNode>();
            requireAllActivators = requireAll;
            stayActiveOnceTriggered = stayActive;
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
                _sr.sortingOrder = 3;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("World2/light_bridge", 32f);
                }
            }

            if (_col != null)
            {
                _col.size = new Vector2(2.0f, 0.5f);
                _col.offset = Vector2.zero;
                _col.enabled = _isActive;
            }

            UpdateVisuals();
        }

        private void Update()
        {
            EvaluateActivators();
        }

        public void EvaluateActivators()
        {
            if (_isActive && stayActiveOnceTriggered) return;
            if (activatorNodes == null || activatorNodes.Count == 0) return;

            bool satisfied = requireAllActivators;
            if (requireAllActivators)
            {
                foreach (PuzzleNode node in activatorNodes)
                {
                    if (node == null || !node.IsActivated)
                    {
                        satisfied = false;
                        break;
                    }
                }
            }
            else
            {
                satisfied = false;
                foreach (PuzzleNode node in activatorNodes)
                {
                    if (node != null && node.IsActivated)
                    {
                        satisfied = true;
                        break;
                    }
                }
            }

            if (satisfied != _isActive)
            {
                SetActiveState(satisfied);
            }
        }

        public void SetActiveState(bool active)
        {
            _isActive = active;

            if (_col != null)
            {
                _col.enabled = _isActive;
            }

            UpdateVisuals();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_isActive ? "crystal_resonate" : "stone_door", 0.6f, _isActive ? 1.25f : 0.85f);
            }
        }

        private void UpdateVisuals()
        {
            if (_sr != null)
            {
                _sr.color = _isActive ? Color.white : new Color(0.4f, 0.8f, 1f, 0.20f);
            }
        }
    }
}
