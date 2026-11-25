using UnityEngine;

public class BulletSpawn : MonoBehaviour
{
    // Typed as BulletVar so Instantiate returns it directly (no GetComponent needed)
    [SerializeField] private BulletVar bulletPrefab;

    // Drag the camera's WorldMousePos here. Falls back to the main camera if left empty.
    [SerializeField] private WorldMousePos mousePos;

    private void Awake()
    {
        if (mousePos == null && !Camera.main.TryGetComponent(out mousePos))
        {
            Debug.LogError($"{nameof(BulletSpawn)} on {name}: no WorldMousePos found on the main camera.", this);
            enabled = false;
        }
    }

    // Hooked up to PlayerInputs.basicAttack in the Inspector
    public void RangedAttack()
    {
        // Flat (XZ) direction from this object towards the mouse hit point
        Vector3 direction = mousePos.Hit - transform.position;
        direction.y = 0f;

        // Mouse is (almost) exactly on top of the player, no meaningful direction
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction.Normalize();

        BulletVar bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(direction));
        bullet.bulletOwner = gameObject;
        bullet.bulletDirection = direction;
    }
}