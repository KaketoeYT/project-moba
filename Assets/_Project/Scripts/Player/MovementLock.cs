using UnityEngine;

namespace _Project.Scripts.Player
{
    // Stops a character from moving for a short time. Used by abilities that make you stand still,
    // and ready for stuns and roots later: anything can call Lock().
    public class MovementLock : MonoBehaviour
    {
        private float _lockedUntil;

        public bool IsLocked => Time.time < _lockedUntil;

        public void Lock(float duration)
        {
            // A new lock never shortens a longer one that is already running
            _lockedUntil = Mathf.Max(_lockedUntil, Time.time + duration);
        }
    }
}