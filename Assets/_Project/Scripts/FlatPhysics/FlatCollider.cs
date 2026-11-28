using _Project.Scripts.Utilities;
using UnityEngine;

// A flat shape on the XZ game plane. Replaces Unity's colliders: height is ignored completely.
//   Circle - characters, bullets, towers, ranges
//   Box    - walls, rubble (turns with the object's Y rotation)
// Size and offset are scaled by the object's X/Z scale, like Unity colliders.
// FlatWorld does all the checks (overlaps, walking into walls).
namespace _Project.Scripts.FlatPhysics
{
    public enum FlatShape
    {
        Circle = 0,
        Box = 1
    }

    [DisallowMultipleComponent]
    public class FlatCollider : MonoBehaviour
    {
        [SerializeField] private FlatShape shape = FlatShape.Circle;

        [Tooltip("Circle only")]
        [SerializeField, Min(0f)] private float radius = 0.5f;

        [Tooltip("Box only: width (X) and depth (Z)")]
        [SerializeField] private Vector2 size = Vector2.one;

        [Tooltip("Shift from the object's position, on the plane (X, Z)")]
        [SerializeField] private Vector2 offset;

        [Tooltip("A zone (tower range, homing radius): it notices what is inside it, but nothing hits it or bumps into it")]
        [SerializeField] private bool isTrigger;

        [Tooltip("Characters can't walk through it (walls, towers). Leave off on characters so they can pass each other")]
        [SerializeField] private bool blocksMovement = true;

        public FlatShape Shape => shape;
        public bool IsTrigger => isTrigger;
        public bool BlocksMovement => blocksMovement && !isTrigger;

        // Centre of the shape on the plane
        public Vector2 Center => FlatWorld.ToFlat(transform.TransformPoint(new Vector3(offset.x, 0f, offset.y)));

        public float WorldRadius
        {
            get
            {
                Vector3 scale = transform.lossyScale;
                return radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            }
        }

        public Vector2 HalfSize
        {
            get
            {
                Vector3 scale = transform.lossyScale;
                return new Vector2(size.x * Mathf.Abs(scale.x), size.y * Mathf.Abs(scale.z)) * 0.5f;
            }
        }

        // The box's sides on the plane (its local X and Z)
        public Vector2 Right => FlatAxis(transform.right, Vector2.right);
        public Vector2 Forward => FlatAxis(transform.forward, Vector2.up);

        // Radius of a circle that fits the whole shape
        public float BoundingRadius => shape == FlatShape.Circle ? WorldRadius : HalfSize.magnitude;

        // Used by the converter tool
        public void Configure(FlatShape newShape, float newRadius, Vector2 newSize, Vector2 newOffset, bool trigger, bool blocks)
        {
            shape = newShape;
            radius = newRadius;
            size = newSize;
            offset = newOffset;
            isTrigger = trigger;
            blocksMovement = blocks;
        }

        private void OnEnable()
        {
            FlatWorld.Register(this);
        }

        private void OnDisable()
        {
            FlatWorld.Unregister(this);
        }

        // The point of this shape closest to `point` (the point itself if it is inside)
        public Vector2 ClosestPoint(Vector2 point)
        {
            Vector2 center = Center;
            Vector2 toPoint = point - center;

            if (shape == FlatShape.Circle)
            {
                float r = WorldRadius;
                return toPoint.sqrMagnitude <= r * r ? point : center + toPoint.normalized * r;
            }

            Vector2 right = Right;
            Vector2 forward = Forward;
            Vector2 half = HalfSize;

            float x = Mathf.Clamp(Vector2.Dot(toPoint, right), -half.x, half.x);
            float z = Mathf.Clamp(Vector2.Dot(toPoint, forward), -half.y, half.y);
            return center + right * x + forward * z;
        }

        private static Vector2 FlatAxis(Vector3 axis, Vector2 fallback)
        {
            var flat = new Vector2(axis.x, axis.z);
            return flat.sqrMagnitude < 0.0001f ? fallback : flat.normalized;
        }

        // Draws the shape just above the ground: red = blocks movement, yellow = zone, green = can be hit
        private void OnDrawGizmos()
        {
            Gizmos.color = isTrigger ? new Color(1f, 0.85f, 0.2f) : BlocksMovement ? new Color(1f, 0.35f, 0.3f) : new Color(0.3f, 1f, 0.4f);
            float y = GamePlane.GroundY + 0.05f;
            Vector2 center = Center;

            if (shape == FlatShape.Circle)
            {
                const int segments = 32;
                float r = WorldRadius;
                Vector3 previous = FlatWorld.ToWorld(center + new Vector2(r, 0f), y);

                for (int i = 1; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    Vector3 next = FlatWorld.ToWorld(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r, y);
                    Gizmos.DrawLine(previous, next);
                    previous = next;
                }

                return;
            }

            Vector2 right = Right * HalfSize.x;
            Vector2 forward = Forward * HalfSize.y;
            Vector3 a = FlatWorld.ToWorld(center - right - forward, y);
            Vector3 b = FlatWorld.ToWorld(center + right - forward, y);
            Vector3 c = FlatWorld.ToWorld(center + right + forward, y);
            Vector3 d = FlatWorld.ToWorld(center - right + forward, y);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}
