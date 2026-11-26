using _Project.Scripts.Combat;
using UnityEngine;

namespace _Project.Scripts.Structures
{
    // A placed wall: blocks movement and bullets, breaks after enough damage,
    // and collapses into rubble on the far side when a shockwave lands close enough.
    public class Wall : MonoBehaviour, IDamageable, IShockwaveReceiver
    {
        [SerializeField, Min(1f)] private float maxHealth = 5f;

        [SerializeField] private GameObject rubblePrefab;

        [Tooltip("How far from the wall's centre the rubble appears")]
        [SerializeField, Min(0f)] private float rubbleOffset = 1.2f;

        [Tooltip("How close a shockwave must land to knock the wall down: 0 = anywhere in the blast radius, 1 = only dead centre")]
        [SerializeField, Range(0f, 1f)] private float collapseThreshold = 0.5f;

        private float _health;
        private bool _isGone; // stops it from being handled twice in the same frame (damage AND shockwave)

        private void Awake()
        {
            _health = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (_isGone)
            {
                return;
            }

            _health -= amount;

            if (_health <= 0f)
            {
                _isGone = true;
                Destroy(gameObject);
            }
        }

        public void OnShockwave(Vector3 point, float strength)
        {
            if (_isGone || strength < collapseThreshold)
            {
                return;
            }

            _isGone = true;

            if (rubblePrefab != null)
            {
                // The wall's flat face points along transform.forward. The rubble falls on the face
                // away from the blast, which is the opposite side from where the shockwave started.
                Vector3 toWall = transform.position - point;
                float side = Vector3.Dot(toWall, transform.forward) >= 0f ? 1f : -1f;
                Vector3 rubblePosition = transform.position + transform.forward * (side * rubbleOffset);

                Instantiate(rubblePrefab, rubblePosition, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}