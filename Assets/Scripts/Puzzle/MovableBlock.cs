using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Pushable carved stone block used to hold down PressurePlates or act as a stepping stone.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class MovableBlock : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private Rigidbody2D _rb;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();

            if (_sr != null)
            {
                _sr.sortingOrder = 7;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("Environment/movable_block");
                }
            }

            if (_col != null)
            {
                _col.size = new Vector2(0.92f, 0.92f);
            }

            if (_rb != null)
            {
                _rb.mass = 2.2f;
                _rb.linearDamping = 4f;
                _rb.gravityScale = 2.5f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
    }
}
