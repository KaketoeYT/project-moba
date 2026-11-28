using _Project.Scripts.FlatPhysics;
using UnityEngine;

// Walks the character on the flat game plane. Height never changes; walls are handled by FlatWorld.
namespace _Project.Scripts.Player
{
    [RequireComponent(typeof(FlatCollider))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;

        [Tooltip("This character's body. Found automatically if on the same object.")]
        [SerializeField] private FlatCollider body;

        // Optional: lets shockwaves and knockbacks push this character. Found automatically if on the same object.
        [SerializeField] private ForceReceiver forceReceiver;

        // Optional: lets abilities (or stuns later) stop this character from moving. Found automatically too.
        [SerializeField] private MovementLock movementLock;

        // Called by Unity when you add the component in the editor: auto-fills the references
        private void Reset()
        {
            playerStats = GetComponent<PlayerStats>();
            body = GetComponent<FlatCollider>();
            forceReceiver = GetComponent<ForceReceiver>();
            movementLock = GetComponent<MovementLock>();
        }

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<FlatCollider>();
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

            // One move per frame, with everything combined. Also runs when standing still,
            // so a wall placed on top of you pushes you out.
            FlatWorld.MoveAndSlide(body, velocity * Time.deltaTime);
        }
    }
}