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
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<BoxCollider2D>();
            _rb = GetComponent<Rigidbody2D>();

            _sr.sortingOrder = 7;
            _sr.sprite = AssetRegistry.GetSprite("Environment/movable_block");

            _col.size = new Vector2(0.92f, 0.92f);
            _rb.mass = 2.2f;
            _rb.linearDamping = 4f;
            _rb.gravityScale = 2.5f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }
}
