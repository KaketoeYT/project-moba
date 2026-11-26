using System;
using UnityEngine;
using UnityEngine.Serialization;
using _Project.Scripts.Combat;

namespace _Project.Scripts.Player
{
        public class PlayerStats : MonoBehaviour, IDamageable
    {
        [FormerlySerializedAs("playerMoveSpeed")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [FormerlySerializedAs("playerMaxHealth")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        [FormerlySerializedAs("playerHealth")]
        [SerializeField] private float health;

        // Read-only access for other scripts (movement, UI health bars...)
        public float MoveSpeed => moveSpeed;
        public float Health => health;
        public float MaxHealth => maxHealth;
        public bool IsDead => health <= 0f;

        // Other scripts can subscribe to this (death screen, score, respawn...)
        public event Action Died;

        private void Awake()
        {
            // [Min] only clamps values you edit in the Inspector, not ones saved earlier as 0
            if (maxHealth < 1f)
            {
                Debug.LogWarning($"{nameof(PlayerStats)} on {name}: Max Health was {maxHealth}, using 1. Set it in the Inspector.", this);
                maxHealth = 1f;
            }

            health = maxHealth;
        }

        // Used by CharacterLoadout to turn this object into a given character. Resets to full health.
        public void SetBaseStats(float newMoveSpeed, float newMaxHealth)
        {
            moveSpeed = Mathf.Max(0f, newMoveSpeed);
            maxHealth = Mathf.Max(1f, newMaxHealth);
            health = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead)
            {
                return;
            }

            health = Mathf.Max(health - amount, 0f);

            if (IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            Died?.Invoke();

            // Placeholder: in a MOBA you'll probably want a respawn timer instead of destroying
            Destroy(gameObject);
        }
    }
}