using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Ancient stone & iron pedestal where the protagonist can place or retrieve the torch (E).
    /// Can also act as a PuzzleNode when a puzzle requires a torch placed in a specific holder.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TorchPedestal : PuzzleNode
    {
        private SpriteRenderer _sr;
        private Sprite _unlitSprite;
        private Sprite _litSprite;
        private TorchController _mountedTorch;

        public bool HasTorch => _mountedTorch != null;
        public Vector2 TorchMountPoint => (Vector2)transform.position + new Vector2(0f, 0.42f);

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = 4;
            _unlitSprite = AssetRegistry.GetSprite("Environment/torch_pedestal_unlit");
            _litSprite = AssetRegistry.GetSprite("Environment/torch_pedestal_lit");
            _sr.sprite = _unlitSprite;
        }

        public void AcceptTorch(TorchController torch)
        {
            _mountedTorch = torch;
            SetActivated(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.0f);
            }
        }

        public void RemoveTorch()
        {
            _mountedTorch = null;
            SetActivated(false);
        }

        protected override void OnVisualStateUpdated(bool active)
        {
            if (_sr != null)
            {
                _sr.sprite = active ? _litSprite : _unlitSprite;
            }
        }
    }
}
