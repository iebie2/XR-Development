using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField]
    GroundCheck groundCheck;
    Rigidbody rigidbody;
    public float jumpStrength = 2;
    public event System.Action Jumped;
    bool wasXRJumpPressed;


    void Reset()
    {
        groundCheck = GetComponentInChildren<GroundCheck>();
        if (!groundCheck)
            groundCheck = GroundCheck.Create(transform);
    }

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");

        if (XRRuntimeSupport.IsActive)
        {
            bool xrJumpPressed = XRRuntimeSupport.GetJumpPressed();
            jumpPressed = xrJumpPressed && !wasXRJumpPressed && !XRUIButtonPointer.IsPointingAtButton;
            wasXRJumpPressed = xrJumpPressed;
        }

        if (jumpPressed && groundCheck.isGrounded)
        {
            rigidbody.AddForce(Vector3.up * 100 * jumpStrength);
            Jumped?.Invoke();
        }
    }
}
