using System.Collections;
using UnityEngine;

public class SpiderPatrolSquish : PatrolCritterBase
{
    [Header("Squish")]
    [SerializeField] private float squishX = 1.4f;
    [SerializeField] private float squishY = 0.3f;
    [SerializeField] private float squishTime = 0.15f;
    [SerializeField] private float squishDig = -0.4f;

    [Header("Squish By Physics Hits")]
    [SerializeField] private float minKillSpeed = 15f;
    [SerializeField] private float minKillSpeedDown = 4f;
    [SerializeField] private LayerMask squishersLayerMask;

    [Header("Player Stomp (Trigger-based)")]
    [SerializeField] private bool allowPlayerStomp = true;
    [SerializeField] private float playerBounceY = 10f;
    [SerializeField] private float stompMinDownSpeed = 0.1f;

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
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, playerBounceY);

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
        SetInactive();

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        transform.localScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);
        transform.localPosition += new Vector3(0f, squishDig, 0f);

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

        yield return new WaitForSeconds(squishTime);
        Destroy(gameObject);
    }
}