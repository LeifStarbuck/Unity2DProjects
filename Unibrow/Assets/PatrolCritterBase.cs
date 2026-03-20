using System.Collections;
using UnityEngine;

public abstract class PatrolCritterBase : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected Transform groundProbe;
    [SerializeField] protected Transform wallProbe;
    [SerializeField] protected float probeRadius = 0.1f;

    [Header("Body")]
    [SerializeField] protected Collider2D bodyCollider;

    [Header("Visuals")]
    [SerializeField] protected Transform visualRoot;
    [SerializeField] protected Transform eyesRoot;

    [Header("Pause And Reflect")]
    [SerializeField] protected float turnPause = 1f;

    [Header("Walk Bob")]
    [SerializeField] protected float walkBobY = 0.04f;
    [SerializeField] protected float walkBobSpeed = 10f;

    [Header("Blink (only while paused)")]
    [SerializeField] protected float blinkMinInterval = 1.2f;
    [SerializeField] protected float blinkMaxInterval = 3.5f;
    [SerializeField] protected float blinkDuration = 0.08f;

    [Header("Pause Lean")]
    [SerializeField] protected float pauseLeanX = 0.18f;
    [SerializeField] protected float pauseLeanSpeed = 6f;

    [Header("Critter vs Critter")]
    [SerializeField] protected bool turnAroundOnCritterContact = true;
    [SerializeField] protected float ignoreCritterCollisionTime = 0.15f;

    [Header("Debug")]
    [SerializeField] protected bool debug = false;

    protected Rigidbody2D rb;
    protected int dir = 1;
    public int Direction => dir;

    protected bool isInactive = false;

    protected float pauseTimer = 0f;
    protected bool pendingFlip = false;

    protected Vector3 visualBaseLocalPos;
    protected Coroutine pauseBlinkCo;

    private bool prevGrounded;
    private bool prevGroundAhead;
    private bool prevWallAhead;

    public Collider2D BodyCollider => bodyCollider;
    public bool IsInactive => isInactive;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (visualRoot != null)
            visualBaseLocalPos = visualRoot.localPosition;
    }

    protected virtual void OnDisable()
    {
        StopPauseBlink();
    }

    protected virtual void FixedUpdate()
    {
        if (isInactive) return;

        bool isGrounded = bodyCollider != null && bodyCollider.IsTouchingLayers(groundLayer);
        bool isPaused = pauseTimer > 0f;

        if (isPaused)
        {
            pauseTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            DoPauseLean();
            EnsurePauseBlinkRunning(true);

            if (pauseTimer <= 0f && pendingFlip)
            {
                pendingFlip = false;
                Flip();
            }

            return;
        }

        EnsurePauseBlinkRunning(false);

        if (!isGrounded)
        {
            ResetVisual();
            return;
        }

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
        DoWalkBob();

        bool groundAhead = groundProbe != null &&
                           Physics2D.Raycast(groundProbe.position, Vector2.down, 0.25f, groundLayer);

        bool wallAhead = wallProbe != null &&
                         Physics2D.Raycast(wallProbe.position, new Vector2(dir, 0f), 0.15f, groundLayer);

        if ((!groundAhead || wallAhead) && !pendingFlip)
        {
            BeginTurnPause();
        }

        if (debug)
        {
            if (isGrounded != prevGrounded || groundAhead != prevGroundAhead || wallAhead != prevWallAhead)
            {
                Debug.Log($"[{name}] grounded={isGrounded} groundAhead={groundAhead} wallAhead={wallAhead} dir={dir} vel={rb.linearVelocity}");
            }

            prevGrounded = isGrounded;
            prevGroundAhead = groundAhead;
            prevWallAhead = wallAhead;
        }
    }

    public virtual void RequestTurn()
    {
        if (isInactive) return;
        if (pauseTimer > 0f || pendingFlip) return;
        BeginTurnPause();
    }
    
    protected virtual void BeginTurnPause()
    {
        pendingFlip = true;
        pauseTimer = turnPause;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    protected virtual void Flip()
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

    protected virtual void DoPauseLean()
    {
        if (visualRoot == null) return;

        visualRoot.localPosition = Vector3.Lerp(
            visualRoot.localPosition,
            visualBaseLocalPos + new Vector3(dir * pauseLeanX, 0f, 0f),
            Time.deltaTime * pauseLeanSpeed
        );
    }

    protected virtual void DoWalkBob()
    {
        if (visualRoot == null) return;

        float t = Time.time * walkBobSpeed;
        float bob = walkBobY * Mathf.Sin(t);
        visualRoot.localPosition = visualBaseLocalPos + new Vector3(0f, bob, 0f);
    }

    protected virtual void ResetVisual()
    {
        if (visualRoot == null) return;
        visualRoot.localPosition = visualBaseLocalPos;
    }

    protected virtual void EnsurePauseBlinkRunning(bool shouldRun)
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

    protected virtual void StopPauseBlink()
    {
        if (pauseBlinkCo != null)
        {
            StopCoroutine(pauseBlinkCo);
            pauseBlinkCo = null;
        }

        if (eyesRoot != null)
            eyesRoot.gameObject.SetActive(true);
    }

    protected virtual IEnumerator PauseBlinkLoop()
    {
        while (true)
        {
            float wait = Random.Range(blinkMinInterval, blinkMaxInterval);
            yield return new WaitForSeconds(wait);

            if (pauseTimer <= 0f) yield break;

            if (eyesRoot != null)
            {
                eyesRoot.gameObject.SetActive(false);
                yield return new WaitForSeconds(blinkDuration);

                if (eyesRoot != null)
                    eyesRoot.gameObject.SetActive(true);
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInactive) return;
        if (!turnAroundOnCritterContact) return;

        //PatrolCritterBase otherCritter = collision.collider.GetComponent<PatrolCritterBase>();
        PatrolCritterBase otherCritter = collision.collider.GetComponentInParent<PatrolCritterBase>();
        if (otherCritter == null || otherCritter == this || otherCritter.IsInactive) return;

        RequestTurn();
        otherCritter.RequestTurn();

        if (BodyCollider != null && otherCritter.BodyCollider != null)
        {
            StartCoroutine(TemporarilyIgnoreCollision(BodyCollider, otherCritter.BodyCollider, ignoreCritterCollisionTime));
        }
    }

    protected IEnumerator TemporarilyIgnoreCollision(Collider2D a, Collider2D b, float duration)
    {
        if (a == null || b == null) yield break;

        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(duration);

        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }

    protected void SetInactive()
    {
        isInactive = true;
        StopPauseBlink();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (groundProbe != null) Gizmos.DrawWireSphere(groundProbe.position, probeRadius);
        if (wallProbe != null) Gizmos.DrawWireSphere(wallProbe.position, probeRadius);
    }
}