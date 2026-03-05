using System.Collections;
using UnityEngine;

public class PlayerHurtResponse : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sprite;   // drag your player sprite renderer
    [SerializeField] private float flashDuration = 0.12f;

    private bool invulnerable;
    private Color baseColor;

    private Health playerHealth;

    public bool LockHorizontal { get; private set; }

    private Coroutine endHurtCo;
    private Coroutine flashCo;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite) baseColor = sprite.color;

        if (!playerHealth) playerHealth = GetComponent<Health>();

        // Loud, actionable errors instead of silent null refs later
        if (!rb) Debug.LogError("PlayerHurtResponse: Missing Rigidbody2D on Player.", this);
        if (!sprite) Debug.LogError("PlayerHurtResponse: Missing SpriteRenderer (child) on Player.", this);
        if (!playerHealth) Debug.LogError("PlayerHurtResponse: Missing Health component on Player.", this);
    }

    public void TryHurt(Vector2 knockbackVelocity, float invulnTime)
    {
        // Prevent NullReferenceException
        if (!rb || !playerHealth) return;

        // Your local invuln gate
        if (invulnerable) return;

        // Health i-frames gate (if Health owns i-frames)
        if (!playerHealth.CanTakeDamage()) return;

        // Apply damage first (only once per valid hit)
        playerHealth.TakeDamage(1);

        invulnerable = true;
        LockHorizontal = true;

        // Apply knockback once
        rb.linearVelocity = knockbackVelocity;

        // Stop only what we own (avoid cancelling unrelated coroutines)
        if (endHurtCo != null) StopCoroutine(endHurtCo);
        if (flashCo != null) StopCoroutine(flashCo);

        endHurtCo = StartCoroutine(EndHurt(invulnTime));
        flashCo = StartCoroutine(FlashRoutine());
    }

    private IEnumerator EndHurt(float t)
    {
        yield return new WaitForSeconds(t);
        LockHorizontal = false;
        invulnerable = false;
        endHurtCo = null;
    }

    private IEnumerator FlashRoutine()
    {
        if (!sprite) yield break;

        sprite.color = Color.red;
        yield return new WaitForSeconds(flashDuration);

        // Restore color (even if something changed it mid-flash)
        if (sprite) sprite.color = baseColor;
        flashCo = null;
    }
}