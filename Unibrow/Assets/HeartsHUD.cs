using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image heartPrefab; // a simple UI Image prefab

    [Header("Sprites")]
    [SerializeField] private Sprite fullHeart;

    [SerializeField] private Color fullColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(0.3f, 0f, 0f, 1f); // dark red

    [Header("Pulse")]
    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private float pulseUpTime = 0.06f;
    [SerializeField] private float pulseDownTime = 0.10f;

    private int lastHp = -1;

    private readonly List<Image> hearts = new();

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += Refresh;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= Refresh;
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("HeartsHUD: playerHealth is not assigned.");
            return;
        }

        BuildHearts(playerHealth.MaxHp);
        Refresh(playerHealth.Hp, playerHealth.MaxHp);
    }

    private void BuildHearts(int maxHp)
    {
        // Clear old
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        hearts.Clear();

        for (int i = 0; i < maxHp; i++)
        {
            var img = Instantiate(heartPrefab, transform);
            img.sprite = fullHeart;
            hearts.Add(img);
        }
    }

    private void Refresh(int hp, int maxHp)
    {
        if (hearts.Count != maxHp)
            BuildHearts(maxHp);

        // Detect damage (hp went down)
        bool tookDamage = (lastHp != -1 && hp < lastHp);

        // Update visuals
        for (int i = 0; i < hearts.Count; i++)
            hearts[i].color = (i < hp) ? fullColor : emptyColor;

        // Pulse the hearts that were just lost
        if (tookDamage)
        {
            // Example: lastHp=5, hp=3 => pulse indices 3 and 4
            for (int i = hp; i < lastHp; i++)
            {
                if (i >= 0 && i < hearts.Count)
                    StartCoroutine(Pulse(hearts[i].rectTransform));
            }
        }

        lastHp = hp;
    }

private IEnumerator Pulse(RectTransform rt)
{
    if (!rt) yield break;

    rt.localScale = Vector3.one;

    Vector3 baseScale = Vector3.one;
    Vector3 bigScale = Vector3.one * pulseScale;
    Vector3 overshoot = Vector3.one * (pulseScale * 0.92f);

    float t = 0f;

    // POP OUT (fast ease-out)
    while (t < pulseUpTime)
    {
        t += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(t / pulseUpTime);

        // Ease-out curve
        k = 1f - Mathf.Pow(1f - k, 3f);

        rt.localScale = Vector3.Lerp(baseScale, bigScale, k);
        yield return null;
    }

    t = 0f;

    // BOUNCE BACK
    while (t < pulseDownTime)
    {
        t += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(t / pulseDownTime);

        // ease-in
        k = k * k;

        rt.localScale = Vector3.Lerp(bigScale, overshoot, k);
        yield return null;
    }

    rt.localScale = baseScale;
}
}