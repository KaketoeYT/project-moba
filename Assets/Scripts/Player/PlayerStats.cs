using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStats : MonoBehaviour
{
    [FormerlySerializedAs("playerMoveSpeed")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Tooltip("Seconds between basic attacks (higher = slower)")]
    [FormerlySerializedAs("playerAttackSpeed")]
    [SerializeField, Min(0f)] private float attackCooldown = 1f;

    [FormerlySerializedAs("playerMaxHealth")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    [FormerlySerializedAs("playerHealth")]
    [SerializeField] private float health;

    // Read-only access for other scripts (movement, input, UI health bars...)
    public float MoveSpeed => moveSpeed;
    public float AttackCooldown => attackCooldown;
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