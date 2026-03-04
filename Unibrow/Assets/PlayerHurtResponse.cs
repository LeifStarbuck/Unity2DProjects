using System.Collections;
using UnityEngine;

public class PlayerHurtResponse : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sprite;   // drag your player sprite renderer
    [SerializeField] private float flashDuration = 0.12f;

    private bool invulnerable;
    private Color baseColor;

    public bool LockHorizontal { get; private set; }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite) baseColor = sprite.color;
    }

public void TryHurt(Vector2 knockbackVelocity, float invulnTime)
{
    if (invulnerable) return;

    invulnerable = true;
    LockHorizontal = true;

    rb.linearVelocity = knockbackVelocity;

    StopAllCoroutines();
    StartCoroutine(EndHurt(invulnTime));
    StartCoroutine(FlashRoutine());
}

private IEnumerator EndHurt(float t)
{
    yield return new WaitForSeconds(t);
    LockHorizontal = false;
    invulnerable = false;
}
    private IEnumerator HurtRoutine(float t)
    {
        invulnerable = true;
        LockHorizontal = true;

        yield return new WaitForSeconds(t);

        LockHorizontal = false;
        invulnerable = false;
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