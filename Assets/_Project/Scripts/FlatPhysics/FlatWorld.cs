using System.Collections.Generic;
using UnityEngine;

// Our own tiny "physics": every FlatCollider on the XZ plane, and the few questions the game asks.
//   Overlap / OverlapCircle - what is touching this zone or circle (bullets, tower ranges, shockwaves)
//   MoveAndSlide            - walk a character, sliding along walls instead of through them
// Plain loops over every collider: simple and plenty fast for a MOBA's few hundred objects.
// Triggers (zones) are never returned by overlaps and never block anything.
namespace _Project.Scripts.FlatPhysics
{
    public static class FlatWorld
    {
        private static readonly List<FlatCollider> Colliders = new List<FlatCollider>();

        public static IReadOnlyList<FlatCollider> All => Colliders;

        internal static void Register(FlatCollider collider)
        {
            Colliders.Add(collider);
        }

        internal static void Unregister(FlatCollider collider)
        {
            Colliders.Remove(collider);
        }

        // World position -> plane position (x, z)
        public static Vector2 ToFlat(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        // Plane position -> world position at the given height
        public static Vector3 ToWorld(Vector2 position, float y)
        {
            return new Vector3(position.x, y, position.y);
        }

        // Everything solid (not zones) touching a circle. Clears `results` first.
        public static void OverlapCircle(Vector2 center, float radius, List<FlatCollider> results, int layerMask = ~0)
        {
            results.Clear();

            foreach (FlatCollider other in Colliders)
            {
                if (other.IsTrigger || (layerMask & (1 << other.gameObject.layer)) == 0)
                {
                    continue;
                }

                if (CircleTouches(other, center, radius))
                {
                    results.Add(other);
                }
            }
        }

        // Everything solid (not zones) touching this collider's shape. Clears `results` first.
        public static void Overlap(FlatCollider shape, List<FlatCollider> results)
        {
            results.Clear();

            foreach (FlatCollider other in Colliders)
            {
                if (other != shape && !other.IsTrigger && Overlaps(shape, other))
                {
                    results.Add(other);
                }
            }
        }

        public static bool Overlaps(FlatCollider a, FlatCollider b)
        {
            if (a.Shape == FlatShape.Circle)
            {
                return CircleTouches(b, a.Center, a.WorldRadius);
            }

            if (b.Shape == FlatShape.Circle)
            {
                return CircleTouches(a, b.Center, b.WorldRadius);
            }

            return BoxesOverlap(a, b);
        }

        // Moves a character's collider by `delta` (height ignored), sliding along anything that blocks movement.
        // Also pushes it out of something that appeared on top of it (a wall placed on it).
        public static void MoveAndSlide(FlatCollider mover, Vector3 delta)
        {
            Transform moverTransform = mover.transform;
            float radius = mover.BoundingRadius;
            Vector2 move = ToFlat(delta);

            // Small steps (half the radius) so a big push can't jump over a thin wall
            float stepLength = Mathf.Max(radius * 0.5f, 0.01f);
            int steps = Mathf.Clamp(Mathf.CeilToInt(move.magnitude / stepLength), 1, 32);
            Vector2 step = move / steps;

            Vector2 center = mover.Center;
            Vector2 pivotOffset = center - ToFlat(moverTransform.position);

            for (int i = 0; i < steps; i++)
            {
                center = PushOut(mover, center + step, radius);
            }

            Vector2 pivot = center - pivotOffset;
            moverTransform.position = new Vector3(pivot.x, moverTransform.position.y, pivot.y);
        }

        // Moves a circle out of everything that blocks movement. A few passes for corners.
        private static Vector2 PushOut(FlatCollider mover, Vector2 center, float radius)
        {
            for (int pass = 0; pass < 4; pass++)
            {
                bool pushed = false;

                foreach (FlatCollider other in Colliders)
                {
                    if (!other.BlocksMovement || other == mover || other.transform.IsChildOf(mover.transform))
                    {
                        continue;
                    }

                    if (TryGetPush(other, center, radius, out Vector2 push))
                    {
                        center += push;
                        pushed = true;
                    }
                }

                if (!pushed)
                {
                    break;
                }
            }

            return center;
        }

        // How far a circle has to move to stop overlapping `other`. False if they don't overlap.
        private static bool TryGetPush(FlatCollider other, Vector2 center, float radius, out Vector2 push)
        {
            push = Vector2.zero;
            Vector2 toCenter = center - other.Center;

            if (other.Shape == FlatShape.Circle)
            {
                float distance = toCenter.magnitude;
                float minDistance = radius + other.WorldRadius;

                if (distance >= minDistance)
                {
                    return false;
                }

                // Exactly on top of each other: pick any direction
                Vector2 direction = distance > 0.0001f ? toCenter / distance : Vector2.up;
                push = direction * (minDistance - distance);
                return true;
            }

            // Box: work in the box's own axes, where it is a plain rectangle
            Vector2 right = other.Right;
            Vector2 forward = other.Forward;
            Vector2 half = other.HalfSize;
            var local = new Vector2(Vector2.Dot(toCenter, right), Vector2.Dot(toCenter, forward));
            Vector2 localPush;

            bool centerInside = Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y;

            if (!centerInside)
            {
                var closest = new Vector2(Mathf.Clamp(local.x, -half.x, half.x), Mathf.Clamp(local.y, -half.y, half.y));
                Vector2 fromClosest = local - closest;
                float distance = fromClosest.magnitude;

                if (distance >= radius)
                {
                    return false;
                }

                localPush = fromClosest / distance * (radius - distance);
            }
            else
            {
                // Centre inside the box: leave through the nearest side
                float depthX = half.x - Mathf.Abs(local.x);
                float depthZ = half.y - Mathf.Abs(local.y);

                localPush = depthX < depthZ
                    ? new Vector2(Mathf.Sign(local.x) * (depthX + radius), 0f)
                    : new Vector2(0f, Mathf.Sign(local.y) * (depthZ + radius));
            }

            push = right * localPush.x + forward * localPush.y;
            return true;
        }

        private static bool CircleTouches(FlatCollider other, Vector2 center, float radius)
        {
            return (other.ClosestPoint(center) - center).sqrMagnitude <= radius * radius;
        }

        // Two turned rectangles overlap unless some side of one of them separates them
        private static bool BoxesOverlap(FlatCollider a, FlatCollider b)
        {
            Vector2 aRight = a.Right, aForward = a.Forward, aHalf = a.HalfSize;
            Vector2 bRight = b.Right, bForward = b.Forward, bHalf = b.HalfSize;
            Vector2 between = b.Center - a.Center;

            return !Separates(aRight) && !Separates(aForward) && !Separates(bRight) && !Separates(bForward);

            bool Separates(Vector2 axis)
            {
                float reachA = aHalf.x * Mathf.Abs(Vector2.Dot(aRight, axis)) + aHalf.y * Mathf.Abs(Vector2.Dot(aForward, axis));
                float reachB = bHalf.x * Mathf.Abs(Vector2.Dot(bRight, axis)) + bHalf.y * Mathf.Abs(Vector2.Dot(bForward, axis));
                return Mathf.Abs(Vector2.Dot(between, axis)) > reachA + reachB;
            }
        }
    }
}
