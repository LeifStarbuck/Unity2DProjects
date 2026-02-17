using UnityEngine;

public class FeetGroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Feet] ENTER: {other.name} layer={LayerMask.LayerToName(other.gameObject.layer)} isTrigger={other.isTrigger}");
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            IsGrounded = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Helps if Enter is missed due to setup issues
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            IsGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[Feet] EXIT: {other.name} layer={LayerMask.LayerToName(other.gameObject.layer)}");
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            IsGrounded = false;
    }
}
