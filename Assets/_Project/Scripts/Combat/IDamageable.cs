namespace _Project.Scripts.Combat
{
    // Implement on anything that can take damage (players, walls, towers...)
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}