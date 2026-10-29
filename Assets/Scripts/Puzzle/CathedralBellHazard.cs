using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Massive sunken cathedral bell that swings or hangs in flooded vaults.
    /// Deals damage / kills player if struck by the swinging bronze rim.
    /// </summary>
    public class CathedralBellHazard : MonoBehaviour
    {
        [Header("Swing Motion")]
        [SerializeField] private float swingAngle = 45f;
        [SerializeField] private float swingSpeed = 1.8f;
        [SerializeField] private float angleOffset = 0f;

        private float timer;
        private Transform pivotTransform;

        private void Awake()
        {
            pivotTransform = transform;
            timer = angleOffset;
        }

        public void Configure(float maxAngle, float speed, float offset = 0f)
        {
            swingAngle = maxAngle;
            swingSpeed = speed;
            angleOffset = offset;
            timer = offset;
        }

        private void Update()
        {
            timer += Time.deltaTime * swingSpeed;
            float currentZ = Mathf.Sin(timer) * swingAngle;
            pivotTransform.rotation = Quaternion.Euler(0f, 0f, currentZ);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Shadow") ||
                collision.GetComponent<Entities.PlayerController>() != null || collision.GetComponent<Entities.ShadowController>() != null)
            {
                if (Combat.HealthSystem.Instance != null)
                {
                    Combat.HealthSystem.Instance.TakeDamage(1, Vector2.up * 5.5f, 0.85f);
                }
                else if (Core.GameManager.Instance != null)
                {
                    Core.GameManager.Instance.RestartCurrentLevel();
                }
            }
        }
    }
}
