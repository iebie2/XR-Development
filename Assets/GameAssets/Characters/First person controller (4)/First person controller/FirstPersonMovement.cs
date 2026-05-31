using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;
    Vector2 velocity;
    Camera playerCamera;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void FixedUpdate()
    {
        if (XRRuntimeSupport.IsActive)
        {
            MoveWithXRController();
            return;
        }

        velocity.y = Input.GetAxis("Vertical") * speed * Time.fixedDeltaTime;
        velocity.x = Input.GetAxis("Horizontal") * speed * Time.fixedDeltaTime;
        transform.Translate(velocity.x, 0, velocity.y);
    }

    private void MoveWithXRController()
    {
        Vector2 axis = XRRuntimeSupport.GetMoveAxis();

        if (axis.sqrMagnitude < 0.01f)
        {
            return;
        }

        Transform directionSource = playerCamera != null ? playerCamera.transform : transform;
        Vector3 forward = Vector3.ProjectOnPlane(directionSource.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(directionSource.right, Vector3.up).normalized;
        Vector3 movement = (forward * axis.y + right * axis.x) * speed * Time.fixedDeltaTime;

        transform.position += movement;
    }
}
