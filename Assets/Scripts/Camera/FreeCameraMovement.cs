using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    [Tooltip("Units per second")]
    [SerializeField, Min(0f)] private float cameraSpeed = 5f;

    [Tooltip("Size of the scroll zone at each screen edge, as a fraction of the screen (0.05 = 5%)")]
    [SerializeField, Range(0.01f, 0.25f)] private float cameraEdgeDistance = 0.05f;

    private void Update()
    {
        // Don't scroll while the game window isn't focused
        if (!Application.isFocused)
        {
            return;
        }

        Vector2 mouse = Input.mousePosition;
        float marginX = Screen.width * cameraEdgeDistance;
        float marginY = Screen.height * cameraEdgeDistance;

        Vector3 direction = Vector3.zero;

        if (mouse.y > Screen.height - marginY) direction.z += 1f; // up
        if (mouse.y < marginY)                 direction.z -= 1f; // down
        if (mouse.x < marginX)                 direction.x -= 1f; // left
        if (mouse.x > Screen.width - marginX)  direction.x += 1f; // right

        if (direction == Vector3.zero)
        {
            return;
        }

        // World axes are right here because the camera target is only tilted, never turned sideways
        transform.position += direction.normalized * (cameraSpeed * Time.deltaTime);
    }
}