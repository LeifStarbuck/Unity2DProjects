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

    [Header("Optional Visual Flip")]
    [SerializeField] private Transform visualRoot; // e.g., SpiderVisual (abdomen/thorax). Leave null to skip.

    [Header("Squish")]
    [SerializeField] private float squishX = 1.4f;   // wider
    [SerializeField] private float squishY = 0.3f;   // flatter
    [SerializeField] private float squishTime = 0.15f;

    [Header("Pause And Reflect")]
    [SerializeField] private float turnPause = 1f;
    private float pauseTimer = 0f;

    [Header("Debug")]
    [SerializeField] private bool debug = true;

    private bool prevGrounded;
    private bool prevGroundAhead;
    private bool prevWallAhead;

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

        // Optional safety: auto-wire spiderCollider if not assigned
        if (spiderCollider == null)
            spiderCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (squished) return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.angularVelocity = Mathf.Sin(Time.time * 10f) * 10f;
            return;
        }

        isGrounded = spiderCollider != null && spiderCollider.IsTouchingLayers(groundLayer);
        if (!isGrounded) return;

        // Move
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        bool groundAhead = Physics2D.Raycast(groundProbe.position, Vector2.down, 0.25f, groundLayer);
        bool wallAhead = Physics2D.Raycast(wallProbe.position, new Vector2(dir, 0f), 0.15f, groundLayer);

        // Flip when cliff or wall
        if (!groundAhead || wallAhead)
            Flip();

        if (debug)
        {
            if (isGrounded != prevGrounded || groundAhead != prevGroundAhead || wallAhead != prevWallAhead)
            {
                Debug.Log($"[Spider] grounded={isGrounded} groundAhead={groundAhead} wallAhead={wallAhead} dir={dir} vel={rb.linearVelocity}");
                if (groundProbe != null) Debug.Log($"[Spider] groundProbe={groundProbe.position}");
                if (wallProbe != null) Debug.Log($"[Spider] wallProbe={wallProbe.position}");
            }

            prevGrounded = isGrounded;
            prevGroundAhead = groundAhead;
            prevWallAhead = wallAhead;
        }
    }

    private void Flip()
    {
        dir *= -1;

        pauseTimer = turnPause;

        // Mirror probes to the new "front" side
        if (groundProbe != null)
        {
            Vector3 lp = groundProbe.localPosition;
            groundProbe.localPosition = new Vector3(-lp.x, lp.y, lp.z);
        }

        if (wallProbe != null)
        {
            Vector3 lp = wallProbe.localPosition;
            wallProbe.localPosition = new Vector3(-lp.x, lp.y, lp.z);
        }

        // Optional: flip visuals so thorax leads
        if (visualRoot != null)
        {
            Vector3 s = visualRoot.localScale;
            visualRoot.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }

        // Optional: immediately apply the new direction velocity this frame
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (squished) return;
        if (!collision.collider.CompareTag("Player")) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            var c = collision.GetContact(i);
            if (c.normal.y < -0.5f)
            {
                var playerRb = collision.collider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 10f);

                StartCoroutine(SquishAndDie());
                break;
            }
        }
    }

    IEnumerator SquishAndDie()
    {
        squished = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        transform.localScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);

        yield return new WaitForSeconds(squishTime);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (groundProbe != null) Gizmos.DrawWireSphere(groundProbe.position, probeRadius);
        if (wallProbe != null) Gizmos.DrawWireSphere(wallProbe.position, probeRadius);
    }
}