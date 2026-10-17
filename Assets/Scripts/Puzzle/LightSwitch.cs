using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Ancient solar crystal mechanism that activates while illuminated by the Torch,
    /// or latches permanently once illuminated if configured.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LightSwitch : PuzzleNode
    {
        [SerializeField] private bool latchWhenLit;

        private SpriteRenderer _sr;
        private Sprite _inactiveSprite;
        private Sprite _activeSprite;
        private TorchController _torch;

        public void Configure(bool shouldLatch)
        {
            latchWhenLit = shouldLatch;
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = 4;
            _inactiveSprite = AssetRegistry.GetSprite("Environment/light_switch_inactive");
            _activeSprite = AssetRegistry.GetSprite("Environment/light_switch_active");
            _sr.sprite = _inactiveSprite;
        }

        private void Update()
        {
            EvaluateIllumination();
        }

        public void EvaluateIllumination()
        {
            if (latchWhenLit && isActivated) return;

            if (_torch == null)
            {
                _torch = FindFirstObjectByType<TorchController>();
            }

            if (_torch != null)
            {
                bool lit = _torch.IsPointIlluminated(transform.position);
                if (lit != isActivated)
                {
                    SetActivated(lit);
                    if (lit && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("switch_activate", 0.85f, 1.15f);
                    }
                }
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
