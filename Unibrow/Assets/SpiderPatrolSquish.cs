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

    [Header("Visuals")]
    [SerializeField] private Transform visualRoot; // SpiderVisual (abdomen/thorax/eyes)
    [SerializeField] private Transform eyesRoot;   // optional: an "Eyes" parent you can hide to blink

    [Header("Squish")]
    [SerializeField] private float squishX = 1.4f;
    [SerializeField] private float squishY = 0.3f;
    [SerializeField] private float squishTime = 0.15f;
    [SerializeField] private float squishDig = -.4f;

    [Header("Pause And Reflect")]
    [SerializeField] private float turnPause = 1f;

    [Header("Look Over Edge")]
    [SerializeField] private float lookLeanX = 0.08f;   // how far to lean forward during pause
    [SerializeField] private float lookBobY = 0.04f;    // tiny bob
    [SerializeField] private float lookSpeed = 10f;     // animation speed

    [Header("Blink")]
    [SerializeField] private float blinkMinInterval = 1.2f;
    [SerializeField] private float blinkMaxInterval = 3.5f;
    [SerializeField] private float blinkDuration = 0.08f;

    [Header("Debug")]
    [SerializeField] private bool debug = true;

    [SerializeField] private Collider2D spiderCollider; // assign CircleCollider2D

    private Rigidbody2D rb;
    private int dir = 1;
    private bool squished = false;
    private Vector3 baseScale;

    private float pauseTimer = 0f;
    private bool pendingFlip = false;

    private bool prevGrounded;
    private bool prevGroundAhead;
    private bool prevWallAhead;

    private Vector3 visualBaseLocalPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;

        if (spiderCollider == null)
            spiderCollider = GetComponent<Collider2D>();

        if (visualRoot != null)
            visualBaseLocalPos = visualRoot.localPosition;
    }

    void OnEnable()
    {
        // Start blinking loop
        if (eyesRoot != null)
            StartCoroutine(BlinkLoop());
    }

    void FixedUpdate()
    {
        if (squished) return;

        bool isGrounded = spiderCollider != null && spiderCollider.IsTouchingLayers(groundLayer);

        // PAUSE BLOCK (pause first, then flip once, then resume)
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;

            // stop horizontal motion while pausing
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // look animation while pausing
            DoLookAnimation();

            if (pauseTimer <= 0f && pendingFlip)
            {
                pendingFlip = false;
                Flip(); // Flip does NOT re-start pause
                ResetLook();
            }

            return;
        }

        // Not paused: keep visuals normal
        ResetLook();

        if (!isGrounded) return;

        // Move
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        bool groundAhead = Physics2D.Raycast(groundProbe.position, Vector2.down, 0.25f, groundLayer);
        bool wallAhead = Physics2D.Raycast(wallProbe.position, new Vector2(dir, 0f), 0.15f, groundLayer);

        // Schedule pause-then-flip (only schedule once)
        if ((!groundAhead || wallAhead) && !pendingFlip)
        {
            pendingFlip = true;
            pauseTimer = turnPause;
        }

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

        // Flip visuals so thorax leads
        if (visualRoot != null)
        {
            Vector3 s = visualRoot.localScale;
            visualRoot.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }

        // Immediately push it in the new direction (no pause here)
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
    }

    private void DoLookAnimation()
    {
        if (visualRoot == null) return;

        // Lean slightly forward in the current dir + bob a tiny bit
        float t = Time.time * lookSpeed;
        float lean = Mathf.Lerp(0f, lookLeanX, 0.5f + 0.5f * Mathf.Sin(t));
        float bob = lookBobY * Mathf.Sin(t * 0.8f);

        visualRoot.localPosition = visualBaseLocalPos + new Vector3(dir * lean, bob, 0f);
    }

    private void ResetLook()
    {
        if (visualRoot == null) return;
        visualRoot.localPosition = visualBaseLocalPos;
    }

    private IEnumerator BlinkLoop()
    {
        // Random blink forever while enabled
        while (true)
        {
            float wait = Random.Range(blinkMinInterval, blinkMaxInterval);
            yield return new WaitForSeconds(wait);

            if (eyesRoot != null)
            {
                eyesRoot.gameObject.SetActive(false);
                yield return new WaitForSeconds(blinkDuration);
                if (eyesRoot != null) eyesRoot.gameObject.SetActive(true);
            }
        }
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

        // squish
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        transform.localScale = new Vector3(baseScale.x * 0.9f, baseScale.y * 1.2f, baseScale.z);
        transform.localScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);
        transform.localPosition += new Vector3(0f, squishDig, 0f);

        // blood spray!!!
        var col2d = GetComponent<Collider2D>();
        float halfWidth = col2d ? col2d.bounds.extents.x : 0.2f; // fallback width

        if (BloodFx.Instance != null)
        {
            BloodFx.Instance.SprayBothSides(transform.position, halfWidth, CgaPalette.Pair.LightRed_Red);
        }
        else
        {
            Debug.LogWarning("BloodFx.Instance is null (no BloodFx in scene).");
        }       

        yield return new WaitForSeconds(squishTime);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (groundProbe != null) Gizmos.DrawWireSphere(groundProbe.position, probeRadius);
        if (wallProbe != null) Gizmos.DrawWireSphere(wallProbe.position, probeRadius);
    }
}