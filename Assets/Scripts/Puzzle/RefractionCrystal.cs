using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Light refraction crystal in the Drowned Cathedral.
    /// When the player brings a lit torch within range, the crystal focuses the light beam,
    /// powering linked doors, water mechanisms, or portals.
    /// </summary>
    public class RefractionCrystal : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRadius = 2.4f;
        [SerializeField] private LayerMask torchLayer;

        [Header("Targets to Open/Activate")]
        [SerializeField] private AncientDoor linkedDoor;

        [Header("Visuals")]
        [SerializeField] private Sprite crystalOffSprite;
        [SerializeField] private Sprite crystalLitSprite;

        private SpriteRenderer sr;
        private bool isLit = false;

        public bool IsLit => isLit;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (crystalOffSprite == null)
                crystalOffSprite = Resources.Load<Sprite>("Sprites/World4/crystal_off");
            if (crystalLitSprite == null)
                crystalLitSprite = Resources.Load<Sprite>("Sprites/World4/crystal_lit");
            UpdateVisual();
        }

        public void Configure(AncientDoor door, float radius = 2.4f)
        {
            linkedDoor = door;
            detectionRadius = radius;
        }

        private void Update()
        {
            CheckTorchProximity();
        }

        private void CheckTorchProximity()
        {
            Entities.TorchController[] torches = FindObjectsByType<Entities.TorchController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            bool foundLitTorch = false;

            for (int i = 0; i < torches.Length; i++)
            {
                if (torches[i] != null)
                {
                    float dist = Vector2.Distance(transform.position, torches[i].transform.position);
                    if (dist <= detectionRadius)
                    {
                        foundLitTorch = true;
                        break;
                    }
                }
            }

            if (foundLitTorch != isLit)
            {
                isLit = foundLitTorch;
                UpdateVisual();
                if (linkedDoor != null)
                {
                    if (isLit) linkedDoor.Open();
                    else linkedDoor.Close();
                }
            }
        }

        private void UpdateVisual()
        {
            if (sr != null)
            {
                sr.sprite = isLit ? crystalLitSprite : crystalOffSprite;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isLit ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
