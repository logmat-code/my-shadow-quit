using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Solidified darkness barrier that blocks movement in the dark,
    /// OR a Light-Repelling Shadow Wall that solidifies when exposed to torchlight.
    /// Configurable via dissolveInLight:
    /// - dissolveInLight = true: Darkness Web (solid in dark, dissolves when illuminated by Torch).
    /// - dissolveInLight = false: Shadow Bridge / Solid Shadow (solid ONLY when in darkness, vanishes in light).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class DarknessBarrier : MonoBehaviour
    {
        [SerializeField] private bool dissolveInLight = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private TorchController _torch;

        public void Configure(bool dissolvesWhenLit, Vector2 size)
        {
            dissolveInLight = dissolvesWhenLit;
            if (_col != null) _col.size = size;
            if (_sr != null)
            {
                _sr.drawMode = SpriteDrawMode.Tiled;
                _sr.size = size;
            }
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<BoxCollider2D>();
            _sr.sortingOrder = 6;
            _sr.sprite = AssetRegistry.GetSprite("Environment/darkness_barrier");

            _col.size = new Vector2(0.9f, 1.95f);
            _col.isTrigger = false;
        }

        private void Update()
        {
            if (_torch == null)
            {
                _torch = FindFirstObjectByType<TorchController>();
            }

            bool isLit = _torch != null && _torch.IsPointIlluminated(transform.position);
            bool isSolid = dissolveInLight ? !isLit : isLit;

            _col.enabled = isSolid;
            float targetAlpha = isSolid ? 0.95f : 0.18f;
            Color c = _sr.color;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * 4f);
            _sr.color = c;
        }
    }
}
