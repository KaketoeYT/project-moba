using UnityEngine;

// Lets outside forces (shockwaves, knockbacks) push a character that moves with PlayerMovement.
// PlayerMovement reads Velocity every frame; this script just makes the push fade out.
namespace _Project.Scripts.Player
{
    public class ForceReceiver : MonoBehaviour
    {
        [Tooltip("How quickly a push fades. A push travels roughly (push speed / drag) units.")]
        [SerializeField, Min(0.1f)] private float drag = 5f;

        public Vector3 Velocity { get; private set; }

        public void AddImpulse(Vector3 impulse)
        {
            Velocity += impulse;
        }

        private void Update()
        {
            if (Velocity == Vector3.zero)
            {
                return;
            }

            // Exponential decay, so it feels the same at any frame rate
            Velocity *= Mathf.Exp(-drag * Time.deltaTime);

            if (Velocity.sqrMagnitude < 0.01f)
            {
                Velocity = Vector3.zero;
            }
        }
    }
}