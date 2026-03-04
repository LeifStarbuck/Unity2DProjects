using UnityEngine;

public class SpiderBodyTrigger : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackX = 8f;
    [SerializeField] private float knockbackY = 6f;
    [SerializeField] private float invulnTime = 0.35f;

    [SerializeField] private SpiderPatrolSquish spider;

    private void Awake()
    {
        if (spider == null)
            spider = GetComponentInParent<SpiderPatrolSquish>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var playerRb = other.attachedRigidbody;
        if (playerRb == null) return;

        if (!playerRb.CompareTag("Player")) return;

        var player = playerRb.GetComponent<PlayerHurtResponse>();
        if (player == null) return;

        float dir = spider != null ? spider.Direction : 1f;

        Vector2 knockVel = new Vector2(dir * knockbackX, knockbackY);

        player.TryHurt(knockVel, invulnTime);
    }
}