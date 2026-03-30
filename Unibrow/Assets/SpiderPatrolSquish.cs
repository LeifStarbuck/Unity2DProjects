using System.Collections;
using UnityEngine;

public class SpiderPatrolSquish : PatrolCritterBase
{
    [Header("Squish")]
    [SerializeField] private float squishX = 1.4f;
    [SerializeField] private float squishY = 0.3f;
    [SerializeField] private float squishDig = -0.4f;
    [SerializeField] private float squishAnimTime = 0.05f;
    [SerializeField] private float corpseLifetime = 10f;
    [SerializeField] private float squishBounceAmount = 0.8f;
    [SerializeField] private Collider2D headTriggerCollider;

    [Header("Squish By Physics Hits")]
    [SerializeField] private float minKillSpeed = 15f;
    [SerializeField] private float minKillSpeedDown = 4f;
    [SerializeField] private LayerMask squishersLayerMask;

    [Header("Player Stomp (Trigger-based)")]
    [SerializeField] private bool allowPlayerStomp = true;
    [SerializeField] private float playerBounceY = 10f;
    [SerializeField] private float playerBounceBoostY = 15f;
    [SerializeField] private bool requireJumpHoldForBoost = true;
    [SerializeField] private float stompMinDownSpeed = 0.1f;

    [SerializeField] private float deadColliderRadius = 0.1f;
    [SerializeField] private Vector2 deadColliderOffset = new Vector2(0f, -0.05f);
    private Vector3 baseScale;

    protected override void Awake()
    {
        base.Awake();
        baseScale = transform.localScale;
    }

    public void TryStompFromPlayer(Collider2D playerCol)
    {
        if (isInactive) return;
        if (!allowPlayerStomp) return;
        if (!playerCol.CompareTag("Player")) return;

        Rigidbody2D playerRb = playerCol.attachedRigidbody;
        if (playerRb == null) return;

        if (playerRb.linearVelocity.y > -stompMinDownSpeed) return;

        Vector2 incomingDir = playerRb.linearVelocity;

        bool jumpHeld = Input.GetKey(KeyCode.Space);
        float bounceY = (requireJumpHoldForBoost && jumpHeld) ? playerBounceBoostY : playerBounceY;

        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceY);

        if (incomingDir.sqrMagnitude < 0.01f)
            incomingDir = new Vector2(dir, 0f);

        StartCoroutine(SquishAndDie(incomingDir));
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInactive) return;

        // First let base handle critter-vs-critter turnarounds.
        //PatrolCritterBase otherCritter = collision.collider.GetComponent<PatrolCritterBase>();
        PatrolCritterBase otherCritter = collision.collider.GetComponentInParent<PatrolCritterBase>();
        if (otherCritter != null && otherCritter != this && !otherCritter.IsInactive)
        {
            base.OnCollisionEnter2D(collision);
            return;
        }

        // Then spider-specific squish-by-physics behavior.
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb == null) return;

        if (squishersLayerMask.value != 0)
        {
            int otherLayerBit = 1 << collision.collider.gameObject.layer;
            if ((squishersLayerMask.value & otherLayerBit) == 0)
                return;
        }

        Vector2 relVel = collision.relativeVelocity;
        float impactSpeed = relVel.magnitude;

        bool mostlyDown = relVel.y < -Mathf.Abs(relVel.x);
        float threshold = mostlyDown ? minKillSpeedDown : minKillSpeed;

        if (impactSpeed >= threshold)
        {
            Vector2 sprayDir = otherRb.linearVelocity;
            if (sprayDir.sqrMagnitude < 0.01f)
                sprayDir = -relVel;

            StartCoroutine(SquishAndDie(sprayDir));
        }
    }
    private IEnumerator SquishAndDie(Vector2 incomingDir)
    {
        SetInactive(); // stop patrol/AI logic so the spider becomes a corpse

        ApplyDeadColliderShape(); // lots of issues with the modified dead body shape, so modify the collider accordingly

        // Reset the visual child to its normal baseline before corpse posing.
        if (visualRoot != null)
            visualRoot.localPosition = visualBaseLocalPos;

        // Disable the stomp trigger so the dead spider no longer responds like a live enemy.
        if (headTriggerCollider != null)
            headTriggerCollider.enabled = false;

        // Move the root object to a harmless layer so the corpse stops interacting like an enemy.
        int deadLayer = LayerMask.NameToLayer("DeadEnemy");
        if (deadLayer != -1)
            gameObject.layer = deadLayer;

        // Turn physics off during the squash animation so Rigidbody2D does not fight our manual movement.
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Vector3 startScale = transform.localScale;
        Vector3 finalScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);

        // Overshoot the squash slightly, then settle back for a cartoony bounce.
        Vector3 overshootScale = new Vector3(
            finalScale.x * (1f + squishBounceAmount),
            finalScale.y * (1f - squishBounceAmount),
            finalScale.z
        );

        Vector3 startPos = transform.localPosition;
        Vector3 finalPos = startPos + new Vector3(0f, squishDig, 0f);
        Vector3 overshootPos = finalPos + new Vector3(0f, squishDig * 0.2f, 0f);

        Collider2D col2d = GetComponent<Collider2D>();
        float halfWidth = col2d ? col2d.bounds.extents.x : 0.2f;

        if (BloodFx.Instance != null)
        {
            BloodFx.Instance.SprayDirectional(
                transform.position,
                halfWidth,
                incomingDir,
                CgaPalette.Pair.LightRed_Red
            );
        }

        float squashInTime = squishAnimTime * 0.65f;
        float settleTime = squishAnimTime * 0.35f;

        float t = 0f;
        while (t < squashInTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / squashInTime);

            // First phase: hit the oversquash pose.
            transform.localScale = Vector3.Lerp(startScale, overshootScale, k);
            transform.localPosition = Vector3.Lerp(startPos, overshootPos, k);

            yield return null;
        }

        t = 0f;
        while (t < settleTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / settleTime);

            // Second phase: settle from oversquash into the final corpse pose.
            transform.localScale = Vector3.Lerp(overshootScale, finalScale, k);
            transform.localPosition = Vector3.Lerp(overshootPos, finalPos, k);

            yield return null;
        }

        // Lock in the final corpse shape/position on the root.
        transform.localScale = finalScale;
        transform.localPosition = finalPos;

        if (visualRoot != null)
            visualRoot.localPosition = visualBaseLocalPos;

        // Re-enable physics AFTER the squish animation so gravity can take over cleanly.
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 3f;

        rb.constraints = RigidbodyConstraints2D.None;

        Debug.Log($"constraints={rb.constraints}, angVel={rb.angularVelocity}, bodyType={rb.bodyType}");
        yield return new WaitForSeconds(corpseLifetime);
        Destroy(gameObject);
        
    }

private void ApplyDeadColliderShape()
{
    if (bodyCollider is CircleCollider2D circle)
    {
        circle.radius = deadColliderRadius;
        circle.offset = deadColliderOffset;
    }
}
}