using System.Collections;
using UnityEngine;

public class SpiderPatrolSquish : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundProbe;
    [SerializeField] private Transform wallProbe;
    [SerializeField] private float probeRadius = 0.1f;

    [Header("Squish")]
    [SerializeField] private float squishX = 1.4f;   // wider
    [SerializeField] private float squishY = 0.5f;   // flatter
    [SerializeField] private float squishTime = 0.15f;

    [SerializeField] private Collider2D spiderCollider; // assign CircleCollider2D
private bool isGrounded;


    private Rigidbody2D rb;
    private int dir = 1;
    private bool squished = false;
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if (squished) return;

        // grounded check: spider touching Ground layer anywhere
        isGrounded = spiderCollider.IsTouchingLayers(groundLayer);

        // If not grounded (falling / in air), don't do edge/wall logic (prevents flip spam)
        if (!isGrounded)
            return;

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        bool groundAhead = Physics2D.Raycast(groundProbe.position, Vector2.down, 0.25f, groundLayer);
        bool wallAhead   = Physics2D.Raycast(wallProbe.position, new Vector2(dir, 0f), 0.15f, groundLayer);

        if (!groundAhead || wallAhead)
            dir *= -1;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (squished) return;

        // Only respond to player stomps from above
        if (!collision.collider.CompareTag("Player")) return;

        // If the contact normal points UP (meaning player hit spider from above)
        for (int i = 0; i < collision.contactCount; i++)
        {
            var c = collision.GetContact(i);

            // Player hit spider from above
            if (c.normal.y < -0.5f)
            {
                // Bounce the player upward
                var playerRb = collision.collider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 10f);
                }

                StartCoroutine(SquishAndDie());
                break;
            }
        }

    }

    IEnumerator SquishAndDie()
    {
        squished = true;

        // Stop moving
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // freeze spider physics

        // Squish (circle -> oval)
        transform.localScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);

        yield return new WaitForSeconds(squishTime);

        // Remove spider (or disable)
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (groundProbe != null) Gizmos.DrawWireSphere(groundProbe.position, probeRadius);
        if (wallProbe != null) Gizmos.DrawWireSphere(wallProbe.position, probeRadius);
    }
}
