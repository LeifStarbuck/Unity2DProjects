using UnityEngine;

public class SplatterDecal : MonoBehaviour
{
    [Header("Drip Growth")]
    [Tooltip("How long the drip grows downward.")]
    [SerializeField] private float dripDuration = 1.6f;

    [Tooltip("World-units per second of extra creep after main drip finishes.")]
    [SerializeField] private float creepSpeed = 0.00f; // try 0.02f for subtle creep

    [Tooltip("Clamp final length.")]
    [SerializeField] private float maxLength = 0.8f;

    [Tooltip("Easing curve for growth. X=time(0-1), Y=length(0-1).")]
    [SerializeField] private AnimationCurve growthCurve =
        new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Header("Fade + Cleanup")]
    [SerializeField] private float fadeStart = 12f;   // start fading later for longer presence
    [SerializeField] private float fadeEnd = 30f;     // must be <= 30s per your requirement

    private MaterialPropertyBlock mpb;
    private MeshRenderer mr;

    private float spawnTime;
    private float baseY;
    private float targetLength;
    private float width;

    private static readonly int LightColorId = Shader.PropertyToID("_LightColor");
    private static readonly int DarkColorId  = Shader.PropertyToID("_DarkColor");
    private static readonly int AlphaId      = Shader.PropertyToID("_Alpha");

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mpb = new MaterialPropertyBlock();

        // Make sure the curve has sane tangents for smooth drip
        if (growthCurve == null || growthCurve.length == 0)
            growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public void Init(Vector3 contactPoint, float decalWidth, float desiredLength, Color32 light, Color32 dark)
    {
        spawnTime = Time.time;
        baseY = contactPoint.y;

        width = Mathf.Max(0.02f, decalWidth);
        targetLength = Mathf.Clamp(desiredLength, 0.08f, maxLength);

        // Anchor at the contact point (top of drip)
        transform.position = new Vector3(contactPoint.x, contactPoint.y, transform.position.z);

        // Start tiny, grow down
        SetLength(0.02f);

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

        // Growth phase: slow drip down
        float length;
        if (age <= dripDuration)
        {
            float t = Mathf.Clamp01(age / dripDuration);
            float eased = Mathf.Clamp01(growthCurve.Evaluate(t));
            length = Mathf.Lerp(0.02f, targetLength, eased);
        }
        else
        {
            // Optional creep after it "lands"
            float extra = creepSpeed * (age - dripDuration);
            length = Mathf.Min(maxLength, targetLength + extra);
        }

        SetLength(length);

        // Slow fade, hard cap
        float alpha = 1f;
        if (age > fadeStart)
        {
            float t = Mathf.InverseLerp(fadeStart, fadeEnd, age);
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

    private void SetLength(float length)
    {
        // Scale Y controls drip length; keep top pinned by shifting down half length
        transform.localScale = new Vector3(width, length, 1f);
        transform.position = new Vector3(transform.position.x, baseY - (length * 0.5f), transform.position.z);
    }
}