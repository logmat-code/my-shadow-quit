using UnityEngine;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// The sacred altar of the Last Flame at Level 50.
    /// Lighting this altar triggers the grand finale of the five-world prototype.
    /// </summary>
    public class FinalFlameAltar : MonoBehaviour
    {
        [Header("Altar Settings")]
        [SerializeField] private float igniteDistance = 2.0f;

        [Header("Visuals")]
        [SerializeField] private Sprite altarSprite;
        [SerializeField] private GameObject flameGlowObject;

        private SpriteRenderer sr;
        private bool isRekindled = false;

        public bool IsRekindled => isRekindled;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (altarSprite == null)
                altarSprite = Resources.Load<Sprite>("Sprites/World5/final_flame_altar");
            if (flameGlowObject != null)
                flameGlowObject.SetActive(false);
        }

        private void Update()
        {
            if (isRekindled) return;

            Entities.TorchController[] torches = FindObjectsByType<Entities.TorchController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < torches.Length; i++)
            {
                if (torches[i] != null)
                {
                    if (Vector2.Distance(transform.position, torches[i].transform.position) <= igniteDistance)
                    {
                        Rekindle();
                        break;
                    }
                }
            }
        }

        public void Rekindle()
        {
            if (isRekindled) return;
            isRekindled = true;

            if (flameGlowObject != null)
                flameGlowObject.SetActive(true);

            LevelExitPortal portal = FindAnyObjectByType<LevelExitPortal>();
            if (portal != null)
            {
                PlayerController player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    portal.TryCompleteLevel(player);
                }
            }
        }
    }
}
