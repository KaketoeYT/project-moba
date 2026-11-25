using UnityEngine;
using UnityEngine.Events;

public class PlayerInputs : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    // Wired up in the Inspector (currently BulletSpawn.RangedAttack)
    [SerializeField] private UnityEvent basicAttack;

    private float _nextBasicAttackTime;

    private void Update()
    {
        if (Time.time < _nextBasicAttackTime || !Input.GetButtonDown("Fire1"))
        {
            return;
        }

        basicAttack.Invoke();
        _nextBasicAttackTime = Time.time + playerStats.AttackCooldown;
    }
}