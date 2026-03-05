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
    [SerializeField] private float squishDig = -0.4f;

    [Header("Pause And Reflect")]
    [SerializeField] private float turnPause = 1f;

    [Header("Walk Bob")]
    [SerializeField] private float walkBobY = 0.04f;
    [SerializeField] private float walkBobSpeed = 10f;

    [Header("Blink (only while paused)")]
    [SerializeField] private float blinkMinInterval = 1.2f;
    [SerializeField] private float blinkMaxInterval = 3.5f;
    [SerializeField] private float blinkDuration = 0.08f;

    [Header("Squish By Physics Hits")]
    [SerializeField] private float minKillSpeed = 15f;
    [SerializeField] private float minKillSpeedDown = 4f;
    [SerializeField] private LayerMask squishersLayerMask;

    [Header("Player Stomp (Trigger-based)")]
    [SerializeField] private bool allowPlayerStomp = true;
    [SerializeField] private float playerBounceY = 10f;
    [SerializeField] private float stompMinDownSpeed = 0.1f;

    [Header("Pause Lean")]
    [SerializeField] private float pauseLeanX = 0.18f;
    [SerializeField] private float pauseLeanSpeed = 6f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    [SerializeField] private Collider2D spiderCollider; // assign CircleCollider2D

    private Rigidbody2D rb;
    private int dir = 1;
    public int Direction => dir;

    private bool squished = false;
    private Vector3 baseScale;

    private float pauseTimer = 0f;
    private bool pendingFlip = false;

    private bool prevGrounded;
    private bool prevGroundAhead;
    private bool prevWallAhead;

    private Vector3 visualBaseLocalPos;

    // Blink control (pause-only)
    private Coroutine pauseBlinkCo;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;

        if (spiderCollider == null)
            spiderCollider = GetComponent<Collider2D>();

        if (visualRoot != null)
            visualBaseLocalPos = visualRoot.localPosition;
    }

    void OnDisable()
    {
        StopPauseBlink();
    }

    void FixedUpdate()
    {
        if (squished) return;

        bool isGrounded = spiderCollider != null && spiderCollider.IsTouchingLayers(groundLayer);

        bool isPaused = pauseTimer > 0f;

        // ---- PAUSE BLOCK ----
        if (isPaused)
        {
            pauseTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // no bob while paused
            DoPauseLean();

            // blink only while paused
            EnsurePauseBlinkRunning(true);

            if (pauseTimer <= 0f && pendingFlip)
            {
                pendingFlip = false;
                Flip();
            }
            return;
        }

        // not paused anymore
        EnsurePauseBlinkRunning(false);

        if (!isGrounded)
        {
            ResetVisual();
            return;
        }

        // ---- MOVE ----
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        // bob only while walking (grounded + not paused + moving)
        DoWalkBob();

        bool groundAhead = Physics2D.Raycast(groundProbe.position, Vector2.down, 0.25f, groundLayer);
        bool wallAhead = Physics2D.Raycast(wallProbe.position, new Vector2(dir, 0f), 0.15f, groundLayer);

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
private void DoPauseLean()
{
    if (visualRoot == null) return;

    visualRoot.localPosition =
        Vector3.Lerp(
            visualRoot.localPosition,
            visualBaseLocalPos + new Vector3(dir * pauseLeanX, 0f, 0f),
            Time.deltaTime * pauseLeanSpeed
        );
}
    private void Flip()
    {
        dir *= -1;

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

        if (visualRoot != null)
        {
            Vector3 s = visualRoot.localScale;
            visualRoot.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
    }

    // -------- Visuals --------

    private void DoWalkBob()
    {
        if (visualRoot == null) return;

        float t = Time.time * walkBobSpeed;
        float bob = walkBobY * Mathf.Sin(t);
        visualRoot.localPosition = visualBaseLocalPos + new Vector3(0f, bob, 0f);
    }

    private void ResetVisual()
    {
        if (visualRoot == null) return;
        visualRoot.localPosition = visualBaseLocalPos;
    }

    // -------- Blink (pause-only) --------

    private void EnsurePauseBlinkRunning(bool shouldRun)
    {
        if (eyesRoot == null) return;

        if (shouldRun)
        {
            if (pauseBlinkCo == null)
                pauseBlinkCo = StartCoroutine(PauseBlinkLoop());
        }
        else
        {
            StopPauseBlink();
        }
    }

    private void StopPauseBlink()
    {
        if (pauseBlinkCo != null)
        {
            StopCoroutine(pauseBlinkCo);
            pauseBlinkCo = null;
        }

        if (eyesRoot != null)
            eyesRoot.gameObject.SetActive(true);
    }

    private IEnumerator PauseBlinkLoop()
    {
        // Only runs while paused (we stop it when leaving pause).
        while (true)
        {
            float wait = Random.Range(blinkMinInterval, blinkMaxInterval);
            yield return new WaitForSeconds(wait);

            if (pauseTimer <= 0f) yield break; // safety

            if (eyesRoot != null)
            {
                eyesRoot.gameObject.SetActive(false);
                yield return new WaitForSeconds(blinkDuration);
                if (eyesRoot != null) eyesRoot.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Call this from a HeadTrigger (IsTrigger collider) when the Player overlaps the spider head.
    /// Works even when Player↔Enemy collisions are disabled in the Physics2D matrix.
    /// </summary>
    public void TryStompFromPlayer(Collider2D playerCol)
    {
        if (squished) return;
        if (!allowPlayerStomp) return;
        if (!playerCol.CompareTag("Player")) return;

        var playerRb = playerCol.attachedRigidbody;
        if (playerRb == null) return;

        if (playerRb.linearVelocity.y > -stompMinDownSpeed) return;

        Vector2 incomingDir = playerRb.linearVelocity;
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, playerBounceY);

        if (incomingDir.sqrMagnitude < 0.01f) incomingDir = new Vector2(dir, 0f);
        StartCoroutine(SquishAndDie(incomingDir));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (squished) return;

        var otherRb = collision.rigidbody;
        if (otherRb == null) return;

        if (squishersLayerMask.value != 0)
        {
            int otherLayerBit = 1 << collision.collider.gameObject.layer;
            if ((squishersLayerMask.value & otherLayerBit) == 0)
                return;
        }

        Vector2 relVel = collision.relativeVelocity;
        float speed = relVel.magnitude;

        bool mostlyDown = relVel.y < -Mathf.Abs(relVel.x);
        float threshold = mostlyDown ? minKillSpeedDown : minKillSpeed;

        if (speed >= threshold)
        {
            Vector2 sprayDir = otherRb.linearVelocity;
            if (sprayDir.sqrMagnitude < 0.01f)
                sprayDir = -relVel;

            StartCoroutine(SquishAndDie(sprayDir));
        }
    }

    IEnumerator SquishAndDie(Vector2 incomingDir)
    {
        squished = true;

        StopPauseBlink(); // ensure eyes restore & coroutine stops

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        transform.localScale = new Vector3(baseScale.x * 0.9f, baseScale.y * 1.2f, baseScale.z);
        transform.localScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);
        transform.localPosition += new Vector3(0f, squishDig, 0f);

        var col2d = GetComponent<Collider2D>();
        float halfWidth = col2d ? col2d.bounds.extents.x : 0.2f;

        if (BloodFx.Instance != null)
        {
            BloodFx.Instance.SprayDirectional(transform.position, halfWidth, incomingDir, CgaPalette.Pair.LightRed_Red);
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