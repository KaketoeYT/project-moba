using UnityEngine;

namespace _Project.Scripts.Utilities
{
    // Put on any prefab that should remove itself after a while (rubble, effects...)
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 8f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}