using UnityEngine;

public class WorldMousePos : MonoBehaviour
{
    // World point under the mouse cursor (last valid hit)
    public Vector3 Hit;

    [SerializeField] private Camera cam;

    // Optional debug marker that follows the mouse hit point
    [SerializeField] private GameObject Sphere;

    // Which layers the mouse ray can hit (ground, walls, etc.)
    [SerializeField] private LayerMask hitMask = ~0;

    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Ignore trigger colliders (tower ranges, bullets, etc.) so they don't block the aim ray
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hitMask, QueryTriggerInteraction.Ignore))
        {
            return; // mouse isn't over anything, keep the last valid Hit
        }

        Hit = hit.point;

        if (Sphere != null)
        {
            Sphere.transform.position = hit.point;
        }
    }
}