using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerHurtResponse : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private CinemachineBrain cinemachineBrain;

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
        if (!cinemachineBrain) cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();

        if (!rb) Debug.LogError("PlayerHurtResponse: Missing Rigidbody2D on Player.", this);
        if (!sprite) Debug.LogError("PlayerHurtResponse: Missing SpriteRenderer (child) on Player.", this);
        if (!playerHealth) Debug.LogError("PlayerHurtResponse: Missing Health component on Player.", this);
        if (!cinemachineBrain) Debug.LogError("PlayerHurtResponse: Missing CinemachineBrain.", this);
    }

    public void TryHurt(Vector2 knockbackVelocity, float invulnTime)
    {
        if (!rb || !playerHealth) return;
        if (invulnerable) return;
        if (!playerHealth.CanTakeDamage()) return;

        ShakeActiveCamera(4f, 0.25f);

        playerHealth.TakeDamage(1);

        invulnerable = true;
        LockHorizontal = true;
        rb.linearVelocity = knockbackVelocity;

        if (endHurtCo != null) StopCoroutine(endHurtCo);
        if (flashCo != null) StopCoroutine(flashCo);

        endHurtCo = StartCoroutine(EndHurt(invulnTime));
        flashCo = StartCoroutine(FlashRoutine());
    }

    private void ShakeActiveCamera(float strength, float time)
    {
        if (!cinemachineBrain) return;

        var activeCam = cinemachineBrain.ActiveVirtualCamera as Component;
        if (!activeCam) return;

        var shake = activeCam.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(strength, time);
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

        if (sprite) sprite.color = baseColor;
        flashCo = null;
    }
}