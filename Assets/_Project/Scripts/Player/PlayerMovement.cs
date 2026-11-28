using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;

        [FormerlySerializedAs("CharacterController")]
        [SerializeField] private CharacterController characterController;

        // Optional: lets shockwaves and knockbacks push this character. Found automatically if on the same object.
        [SerializeField] private ForceReceiver forceReceiver;

        // Optional: lets abilities (or stuns later) stop this character from moving. Found automatically too.
        [SerializeField] private MovementLock movementLock;

        private float _verticalVelocity;

        // Called by Unity when you add the component in the editor: auto-fills the references
        private void Reset()
        {
            playerStats = GetComponent<PlayerStats>();
            characterController = GetComponent<CharacterController>();
            forceReceiver = GetComponent<ForceReceiver>();
            movementLock = GetComponent<MovementLock>();
        }

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (forceReceiver == null)
            {
                TryGetComponent(out forceReceiver);
            }

            if (movementLock == null)
            {
                TryGetComponent(out movementLock);
            }
        }

        private void Update()
        {
            // While locked (e.g. just used a basic attack) your movement input is ignored.
            // Raw input = instant start/stop, which feels right for MOBA movement.
            bool isLocked = movementLock != null && movementLock.IsLocked;
            Vector3 input = isLocked
                ? Vector3.zero
                : new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            // ClampMagnitude stops diagonals from being ~41% faster than straight lines
            Vector3 velocity = Vector3.ClampMagnitude(input, 1f) * playerStats.MoveSpeed;

            // Outside pushes add on top of your own movement. They still work while standing still.
            if (forceReceiver != null)
            {
                velocity += forceReceiver.Velocity;
            }

            // CharacterController has no built-in gravity. A small downward push while grounded
            // keeps isGrounded reliable and makes the player follow terrain slopes.
            if (characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            velocity.y = _verticalVelocity;

            // One Move call per frame, with everything combined
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}