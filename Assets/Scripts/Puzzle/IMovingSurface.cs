using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Implemented by kinematic platforms that carry passengers. Exposes the real velocity the
    /// platform is driven with in FixedUpdate so characters can move relative to the surface
    /// instead of being teleported or relying on friction.
    /// </summary>
    public interface IMovingSurface
    {
        Vector2 SurfaceVelocity { get; }
    }

    /// <summary>
    /// Shared helper used by PlayerController and ShadowController to resolve the moving surface
    /// (if any) underneath their feet and to compute carried velocities consistently.
    /// </summary>
    public static class MovingSurfaceUtil
    {
        /// <summary>
        /// Returns the velocity of the first moving surface found in the supplied foot-contact colliders.
        /// </summary>
        public static bool TryGetSurfaceVelocity(Collider2D hit, out Vector2 velocity)
        {
            velocity = Vector2.zero;
            if (hit == null) return false;
            if (hit.TryGetComponent(out IMovingSurface surface))
            {
                velocity = surface.SurfaceVelocity;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Horizontal passenger update. Movement input is applied relative to the reference
        /// velocity (surface velocity while grounded, decaying inherited velocity in the air),
        /// so standing still keeps the passenger glued to the surface with no cumulative drift.
        /// </summary>
        public static float StepHorizontal(float currentVelX, float referenceVelX, float nextReferenceVelX, float targetSpeed, float accelRate, float dt)
        {
            float relative = currentVelX - referenceVelX;
            relative = Mathf.MoveTowards(relative, targetSpeed, accelRate * dt);
            return relative + nextReferenceVelX;
        }
    }
}
