using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Organic Light Root mechanism for World 2 ("The Inverted Forest").
    /// Reacts dynamically to the Torch's golden warmth and light radius.
    /// When illuminated, dormant vines unfurl and bloom into a solid traversable platform.
    /// When darkness returns (unless latched), roots curl back into impassable briars.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class LightRootMechanism : MonoBehaviour
    {
        [SerializeField] private float lightDetectionRadius = 3.8f;
        [SerializeField] private bool stayBloomedOnceIlluminated = false;
        [SerializeField] private List<PuzzleNode> auxiliaryNodes = new List<PuzzleNode>();

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private Sprite _inactiveSprite;
        private Sprite _activeSprite;
        private bool _isBloomed;

        public bool IsBloomed => _isBloomed;

        public void Configure(float detectionRadius = 3.8f, bool stayBloomed = false)
        {
            lightDetectionRadius = detectionRadius;
            stayBloomedOnceIlluminated = stayBloomed;
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

            if (_inactiveSprite == null) _inactiveSprite = AssetRegistry.GetSprite("World2/light_root_inactive", 32f);
            if (_activeSprite == null) _activeSprite = AssetRegistry.GetSprite("World2/light_root_active", 32f);

            if (_sr != null)
            {
                _sr.sortingOrder = 4;
                _sr.sprite = _isBloomed ? _activeSprite : _inactiveSprite;
            }

            if (_col != null)
            {
                _col.size = new Vector2(2.0f, 0.75f);
                _col.offset = Vector2.zero;
                _col.enabled = _isBloomed;
            }
        }

        private void Update()
        {
            EvaluateIlluminationState();
        }

        public void EvaluateIlluminationState()
        {
            if (_isBloomed && stayBloomedOnceIlluminated) return;

            bool isIlluminated = CheckTorchLightProximity();

            if (!isIlluminated && auxiliaryNodes != null && auxiliaryNodes.Count > 0)
            {
                foreach (PuzzleNode node in auxiliaryNodes)
                {
                    if (node != null && node.IsActivated)
                    {
                        isIlluminated = true;
                        break;
                    }
                }
            }

            if (isIlluminated != _isBloomed)
            {
                SetBloomedState(isIlluminated);
            }
        }

        private bool CheckTorchLightProximity()
        {
            TorchController torch = TorchController.Instance != null
                ? TorchController.Instance
                : (GameManager.Instance != null && GameManager.Instance.Levels != null
                    ? GameManager.Instance.Levels.Torch
                    : FindAnyObjectByType<TorchController>());

            if (torch == null) return false;

            // Distance check to Torch
            float dist = Vector2.Distance(transform.position, torch.transform.position);
            return dist <= lightDetectionRadius || torch.IsPointIlluminated(transform.position);
        }

        public void SetBloomedState(bool bloomed)
        {
            _isBloomed = bloomed;

            if (_sr != null)
            {
                _sr.sprite = _isBloomed ? _activeSprite : _inactiveSprite;
                _sr.color = _isBloomed ? Color.white : new Color(0.75f, 0.75f, 0.75f, 0.85f);
            }

            if (_col != null)
            {
                _col.enabled = _isBloomed;
            }

            if (AudioManager.Instance != null)
            {
                if (_isBloomed)
                {
                    AudioManager.Instance.PlaySFX("crystal_resonate", 0.55f, 1.2f);
                }
                else
                {
                    AudioManager.Instance.PlaySFX("web_burn", 0.4f, 0.9f);
                }
            }
        }
    }
}
