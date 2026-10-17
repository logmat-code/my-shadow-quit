using System;
using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Base class for all puzzle triggers (PressurePlate, LightSwitch, ShadowSwitch, TorchPedestal).
    /// Provides clear event-driven activation state for AncientDoors and other mechanisms.
    /// </summary>
    public abstract class PuzzleNode : MonoBehaviour
    {
        public event Action<PuzzleNode, bool> OnStateChanged;

        [SerializeField] protected bool isActivated;

        public bool IsActivated => isActivated;

        protected void SetActivated(bool active)
        {
            if (isActivated == active) return;
            isActivated = active;
            OnVisualStateUpdated(isActivated);
            OnStateChanged?.Invoke(this, isActivated);
        }

        protected abstract void OnVisualStateUpdated(bool active);
    }
}
