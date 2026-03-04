using UnityEngine;

public class SpiderBodyTrigger : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackX = 8f;
    [SerializeField] private float knockbackY = 6f;
    [SerializeField] private float invulnTime = 0.35f;

    [Header("Auto-found at runtime")]
    [SerializeField] private SpiderPatrolSquish spider;
    [SerializeField] private Rigidbody2D spiderRb;

    private void Awake()
    {
        // Robust: find on parent/root even if Reset never ran
        if (!spider) spider = GetComponentInParent<SpiderPatrolSquish>();
        if (!spider) spider = transform.root.GetComponent<SpiderPatrolSquish>();

        if (!spiderRb) spiderRb = GetComponentInParent<Rigidbody2D>();
        if (!spiderRb) spiderRb = transform.root.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player may have child colliders; attachedRigidbody points to the RB owner
        var playerRb = other.attachedRigidbody;
        if (playerRb == null) return;

        // Tag is usually on the player root (RB owner), not the child collider
        if (!playerRb.CompareTag("Player")) return;

        var player = playerRb.GetComponent<PlayerHurtResponse>();
        if (player == null) return;

        // Prefer spider.Direction if we have it; otherwise infer from spider's current velocity.
        float dir = 0f;

        if (spider != null)
        {
            dir = spider.Direction; // requires: public int Direction => dir; in SpiderPatrolSquish
        }

        if (dir == 0f && spiderRb != null)
        {
            dir = Mathf.Sign(spiderRb.linearVelocity.x);
        }

        if (dir == 0f) dir = 1f; // last-resort default

        Vector2 knockVel = new Vector2(dir * knockbackX, knockbackY);
        player.TryHurt(knockVel, invulnTime);
    }
}