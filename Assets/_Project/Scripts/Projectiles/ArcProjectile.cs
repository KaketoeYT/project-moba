using System;
using UnityEngine;

// Flies from where it spawns to a target point along a parabola, like a mortar shell.
// It has no collider on purpose: it arcs over everything and only does something when it lands.
namespace _Project.Scripts.Projectiles
{
    public class ArcProjectile : MonoBehaviour
    {
        [Tooltip("Horizontal speed in units per second")]
        [SerializeField, Min(0.1f)] private float speed = 12f;

        [Tooltip("Peak height as a fraction of the flight distance (0.4 = 40%)")]
        [SerializeField, Min(0f)] private float arcHeightRatio = 0.4f;

        public GameObject Owner { get; private set; }

        // Raised once when the projectile touches down, with the landing point.
        // Effect components (ShockwaveOnLand, SpawnOnLand...) listen to this.
        public event Action<Vector3> Landed;

        private Vector3 _start;
        private Vector3 _target;
        private float _duration;
        private float _arcHeight;
        private float _elapsed;
        private bool _launched;

        public void Launch(GameObject owner, Vector3 target)
        {
            Owner = owner;
            _start = transform.position;
            _target = target;

            Vector3 flat = target - _start;
            flat.y = 0f;
            float distance = flat.magnitude;

            _duration = Mathf.Max(distance / speed, 0.1f);
            _arcHeight = distance * arcHeightRatio;
            _elapsed = 0f;
            _launched = true;
        }

        private void Update()
        {
            if (!_launched)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);

            // Straight line from start to target, plus a parabola on top:
            // 4 * t * (1 - t) is 0 at both ends and 1 at the halfway point
            Vector3 position = Vector3.Lerp(_start, _target, t);
            position.y += _arcHeight * 4f * t * (1f - t);
            transform.position = position;

            if (t >= 1f)
            {
                Land();
            }
        }

        private void Land()
        {
            _launched = false;
            Landed?.Invoke(_target);
            Destroy(gameObject);
        }
    }
}