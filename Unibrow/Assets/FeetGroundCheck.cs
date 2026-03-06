using UnityEngine;

public class FeetGroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; }

    [SerializeField] private LayerMask groundLayers;

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((groundLayers.value & (1 << other.gameObject.layer)) != 0)
            IsGrounded = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if ((groundLayers.value & (1 << other.gameObject.layer)) != 0)
            IsGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if ((groundLayers.value & (1 << other.gameObject.layer)) != 0)
            IsGrounded = false;
    }
}