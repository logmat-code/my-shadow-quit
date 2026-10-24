using System;
using UnityEngine;

namespace MyShadowQuit.Combat
{
    public interface IDamageable
    {
        bool TakeDamage(int amount, Vector2 hitDirection, float knockback = 0f);
        void KillInstantly();
        bool IsDead { get; }
    }
}
