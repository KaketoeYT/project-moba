using UnityEngine;

namespace _Project.Scripts.Utilities
{
    // The game plays out on one flat plane, so every height decision lives here, in one place.
    // Nothing else in the project should need to know how high anything is.
    public static class GamePlane
    {
        // World height of the ground. Aiming, placing walls and landing stones all use it.
        public const float GroundY = 0f;

        // How far above the ground projectiles fly. Roughly chest height, so they overlap every character.
        public const float ProjectileHeight = 1f;

        // Height of the plane the mouse aims at. 0 = aim at the ground (where characters' feet are).
        // Set it to ProjectileHeight to aim at chest height instead.
        public const float AimHeight = 0f;

        // The same position, moved onto the ground
        public static Vector3 OnGround(Vector3 position)
        {
            return new Vector3(position.x, GroundY, position.z);
        }

        // The same position, moved to projectile height
        public static Vector3 AtProjectileHeight(Vector3 position)
        {
            return new Vector3(position.x, GroundY + ProjectileHeight, position.z);
        }

        // Direction from one point to another on the flat plane (height ignored).
        // Zero if they are on top of each other.
        public static Vector3 Direction(Vector3 from, Vector3 to)
        {
            Vector3 flat = to - from;
            flat.y = 0f;
            return flat.sqrMagnitude < 0.0001f ? Vector3.zero : flat.normalized;
        }
    }
}