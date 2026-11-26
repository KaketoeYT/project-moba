using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [FormerlySerializedAs("TargetPlayer")]
    [SerializeField] private Transform targetPlayer;

    [FormerlySerializedAs("TargetFree")]
    [SerializeField] private Transform targetFree;

    [SerializeField] private KeyCode toggleKey = KeyCode.Space;

    private FreeCameraMovement _freeCameraMovement;
    private bool _isFreeCamera;

    private void Awake()
    {
        if (targetPlayer == null || targetFree == null)
        {
            Debug.LogError($"{nameof(CameraController)} on {name}: assign both camera targets.", this);
            enabled = false;
            return;
        }

        // Cached once; may stay null if the free target has no movement script
        targetFree.TryGetComponent(out _freeCameraMovement);
    }

    private void Start()
    {
        // Follow the player from the very beginning
        SetFreeCamera(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetFreeCamera(!_isFreeCamera);
        }
    }

    private void SetFreeCamera(bool free)
    {
        _isFreeCamera = free;

        if (free)
        {
            // Start free-roaming from wherever the player camera currently is
            targetFree.position = targetPlayer.position;
        }

        AttachTo(free ? targetFree : targetPlayer);

        // Edge scrolling only makes sense while the free camera is active
        if (_freeCameraMovement != null)
        {
            _freeCameraMovement.enabled = free;
        }
    }

    private void AttachTo(Transform target)
    {
        // worldPositionStays = false, then zero the local pose so the camera sits exactly on the target.
        // Parenting means no per-frame follow code is needed at all.
        transform.SetParent(target, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}