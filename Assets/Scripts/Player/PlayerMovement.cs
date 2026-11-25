using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [FormerlySerializedAs("CharacterController")]
    [SerializeField] private CharacterController characterController;

    private float _verticalVelocity;

    // Called by Unity when you add the component in the editor: auto-fills the references
    private void Reset()
    {
        playerStats = GetComponent<PlayerStats>();
        characterController = GetComponent<CharacterController>();
    }

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        // Raw input = instant start/stop, which feels right for MOBA movement.
        // ClampMagnitude stops diagonals from being ~41% faster than straight lines.
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 velocity = Vector3.ClampMagnitude(input, 1f) * playerStats.MoveSpeed;

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