using UnityEngine;

namespace _Project.Scripts.Utilities
{
    // Editor-only visual helpers built on Debug.DrawLine.
    // [Conditional] means that in a real build the calls, and the work inside them, are removed entirely.
    public static class DebugDraw
    {
        private const int CircleSegments = 48;

        // A circle lying flat on the ground under 'center' (the ground is found with a raycast straight down)
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void CircleOnGround(Vector3 center, float radius, Color color)
        {
            if (Physics.Raycast(center, Vector3.down, out RaycastHit hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                // Slightly above the ground so the line isn't hidden inside it
                center.y = hit.point.y + 0.05f;
            }

            Circle(center, radius, color);
        }

        // A flat circle on the XZ plane, drawn as short straight lines
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Circle(Vector3 center, float radius, Color color)
        {
            float step = Mathf.PI * 2f / CircleSegments;
            Vector3 previous = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= CircleSegments; i++)
            {
                float angle = step * i;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Debug.DrawLine(previous, next, color);
                previous = next;
            }
        }
    }
}