using System.Collections;
using UnityEngine;

public class HiddenRoomRevealTrigger : MonoBehaviour
{
    [Header("What To Fade")]
    [SerializeField] private SpriteRenderer[] renderersToFade;

    [Header("What To Disable")]
    [SerializeField] private Collider2D[] collidersToDisable;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(FadeAndDisable());
    }

    private IEnumerator FadeAndDisable()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, t);

            for (int i = 0; i < renderersToFade.Length; i++)
            {
                SpriteRenderer sr = renderersToFade[i];
                if (sr == null) continue;

                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }

        for (int i = 0; i < collidersToDisable.Length; i++)
        {
            Collider2D col = collidersToDisable[i];
            if (col != null)
                col.enabled = false;
        }
    }
}