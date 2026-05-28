using UnityEngine;

public class LeverTorque : MonoBehaviour
{
    public float torqueForce = 50f;
    public Rigidbody rb;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            rb.AddTorque(Vector3.forward * torqueForce);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rb.AddTorque(-Vector3.forward * torqueForce);
        }
    }
}