using _Project.Scripts.Utilities;
using UnityEngine;

namespace _Project.Scripts.Controls
{
    public class WorldMousePos : MonoBehaviour
    {
        // World point under the mouse cursor on the ground plane (last valid point)
        public Vector3 Hit;

        [SerializeField] private Camera cam;

        // Optional debug marker that follows the mouse point
        [SerializeField] private GameObject Sphere;

        // An invisible flat plane that the mouse ray is intersected with
        private static readonly Plane AimPlane = new Plane(Vector3.up, new Vector3(0f, GamePlane.GroundY + GamePlane.AimHeight, 0f));

        private void Awake()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void Update()
        {
            Ray ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);

            // No colliders involved: walls, characters and terrain can never change where you aim.
            // Fails only if the ray never reaches the plane, in which case we keep the last valid point.
            if (!AimPlane.Raycast(ray, out float distance))
            {
                return;
            }

            Hit = ray.GetPoint(distance);

            if (Sphere != null)
            {
                Sphere.transform.position = Hit;
            }
        }
    }
}