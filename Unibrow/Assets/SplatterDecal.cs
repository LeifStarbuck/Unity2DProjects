using UnityEngine;

public class SplatterDecal : MonoBehaviour
{
    [SerializeField] private float fallTime = 0.12f;
    [SerializeField] private float maxLength = 0.35f;      // world units
    [SerializeField] private float fadeStart = 10f;        // seconds
    [SerializeField] private float fadeEnd = 30f;          // hard cap
    [SerializeField] private float gravity = 10f;          // visual gravity

    private MaterialPropertyBlock mpb;
    private MeshRenderer mr;
    private float spawnTime;
    private float baseY;
    private float velocity;
    private float targetLength;
    private float width;

    private static readonly int LightColorId = Shader.PropertyToID("_LightColor");
    private static readonly int DarkColorId  = Shader.PropertyToID("_DarkColor");
    private static readonly int AlphaId      = Shader.PropertyToID("_Alpha");

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void Init(Vector3 contactPoint, float decalWidth, float desiredLength, Color32 light, Color32 dark)
    {
        spawnTime = Time.time;
        baseY = contactPoint.y;
        velocity = 0f;

        width = decalWidth;
        targetLength = Mathf.Clamp(desiredLength, 0.06f, maxLength);

        // Position: we anchor at top near contact and grow downward.
        transform.position = new Vector3(contactPoint.x, contactPoint.y, transform.position.z);
        transform.localScale = new Vector3(width, 0.02f, 1f);

        mr.GetPropertyBlock(mpb);
        mpb.SetColor(LightColorId, light);
        mpb.SetColor(DarkColorId, dark);
        mpb.SetFloat(AlphaId, 1f);
        mr.SetPropertyBlock(mpb);

        gameObject.SetActive(true);
    }

    private void Update()
    {
        float age = Time.time - spawnTime;

        // 1) quick "fall/lengthen" phase
        if (age <= fallTime)
        {
            velocity += gravity * Time.deltaTime;
            float dy = velocity * Time.deltaTime;

            // push bottom down by increasing height, keep top pinned
            float newH = Mathf.Lerp(0.02f, targetLength, age / fallTime);
            transform.localScale = new Vector3(width, newH, 1f);

            // keep top edge approximately at contact point by shifting down half height
            transform.position = new Vector3(transform.position.x, baseY - (newH * 0.5f), transform.position.z);
        }

        // 2) very slow fade, hard cap at 30s
        float alpha = 1f;
        if (age > fadeStart)
        {
            float t = Mathf.InverseLerp(fadeStart, fadeEnd, age);
            // "almost imperceptible" fade curve
            alpha = Mathf.Clamp01(1f - t);
        }

        mr.GetPropertyBlock(mpb);
        mpb.SetFloat(AlphaId, alpha);
        mr.SetPropertyBlock(mpb);

        if (age >= fadeEnd)
        {
            gameObject.SetActive(false);
        }
    }
}