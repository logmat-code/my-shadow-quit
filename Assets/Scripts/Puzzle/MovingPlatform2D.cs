using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Kinematic moving platform for World 2 ("The Inverted Forest").
    /// Moves smoothly between waypoints in FixedUpdate using Rigidbody2D.MovePosition.
    /// Safely conveys standing characters (Protagonist, Shadow, MovableBlock) without parenting
    /// their transforms, preventing physics glitches and jitter in 2D platforming.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class MovingPlatform2D : MonoBehaviour, IMovingSurface
    {
        [SerializeField] private Vector2[] waypoints;
        [SerializeField] private float speed = 2.2f;
        [SerializeField] private float waitDuration = 0.6f;
        [SerializeField] private bool pingPong = true;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private Rigidbody2D _rb;

        private int _currentWaypointIndex;
        private int _direction = 1;
        private float _waitTimer;
        private Vector2 _previousPosition;

        public Vector2[] Waypoints => waypoints;
        public float Speed => speed;
        public bool IsMoving => waypoints != null && waypoints.Length > 1 && _waitTimer <= 0f;

        /// <summary>Real kinematic velocity applied this physics step (zero while waiting).</summary>
        public Vector2 SurfaceVelocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

        public void Configure(Vector2[] points, float moveSpeed = 2.2f, float waitTime = 0.6f, bool isPingPong = true)
        {
            waypoints = points;
            speed = moveSpeed;
            waitDuration = waitTime;
            pingPong = isPingPong;
            EnsureInitialized();
            if (waypoints != null && waypoints.Length > 0)
            {
                transform.position = waypoints[0];
                if (_rb != null) _rb.position = waypoints[0];
                _previousPosition = waypoints[0];
            }
        }

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
                _sr.sortingOrder = 4;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("World2/moving_platform", 32f);
                }
            }

            if (_col != null)
            {
                _col.size = new Vector2(2.0f, 0.72f);
                _col.offset = Vector2.zero;
            }

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (waypoints == null || waypoints.Length == 0)
            {
                waypoints = new[] { (Vector2)transform.position, (Vector2)transform.position + Vector2.right * 3.5f };
            }

            _previousPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        }

        private void FixedUpdate()
        {
            if (_rb == null || waypoints == null || waypoints.Length <= 1) return;

            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            Vector2 currentPos = _rb.position;

            if (_waitTimer > 0f)
            {
                _waitTimer -= dt;
                _rb.linearVelocity = Vector2.zero;
                _previousPosition = currentPos;
                return;
            }

            Vector2 targetWp = waypoints[_currentWaypointIndex];
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetWp, speed * dt);
            bool arrived = (targetWp - newPos).sqrMagnitude < 0.0004f;
            if (arrived) newPos = targetWp;

            // Drive the kinematic body with a real velocity so the physics solver sees the
            // platform motion and passengers (Player / Shadow) can match it exactly.
            _rb.linearVelocity = (newPos - currentPos) / dt;

            if (arrived)
            {
                _waitTimer = waitDuration;
                AdvanceWaypoint();
            }

            _previousPosition = newPos;
        }

        private void AdvanceWaypoint()
        {
            if (pingPong)
            {
                _currentWaypointIndex += _direction;
                if (_currentWaypointIndex >= waypoints.Length)
                {
                    _currentWaypointIndex = waypoints.Length - 2;
                    _direction = -1;
                }
                else if (_currentWaypointIndex < 0)
                {
                    _currentWaypointIndex = 1;
                    _direction = 1;
                }
            }
            else
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        public void ResetPlatform()
        {
            _currentWaypointIndex = 0;
            _direction = 1;
            _waitTimer = 0f;
            if (waypoints != null && waypoints.Length > 0)
            {
                transform.position = waypoints[0];
                if (_rb != null)
                {
                    _rb.position = waypoints[0];
                    _rb.linearVelocity = Vector2.zero;
                }
                _previousPosition = waypoints[0];
            }
        }

        private void OnDisable()
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }
    }
}
