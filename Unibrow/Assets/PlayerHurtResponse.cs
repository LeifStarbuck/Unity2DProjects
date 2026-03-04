using System.Collections;
using UnityEngine;

public class PlayerHurtResponse : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sprite;   // drag your player sprite renderer
    [SerializeField] private float flashDuration = 0.12f;

    private bool invulnerable;
    private Color baseColor;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite) baseColor = sprite.color;
    }

    public void TryHurt(Vector2 knockbackImpulse, float invulnTime)
    {
        if (invulnerable) return;

        // Knockback: reset current velocity then apply impulse
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(knockbackImpulse, ForceMode2D.Impulse);

        StartCoroutine(InvulnRoutine(invulnTime));
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator InvulnRoutine(float t)
    {
        invulnerable = true;
        yield return new WaitForSeconds(t);
        invulnerable = false;
    }

    private IEnumerator FlashRoutine()
    {
        if (!sprite) yield break;

        sprite.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        if (sprite) sprite.color = baseColor;
    }
}