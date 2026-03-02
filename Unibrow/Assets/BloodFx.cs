using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BloodFx : MonoBehaviour
{
    public static BloodFx Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private ParticleSystem sprayPrefab;     // BloodSprayPS
    [SerializeField] private SplatterDecal splatterPrefab;   // SplatterQuad

    [Header("Spray")]
    [SerializeField] private int particlesPerSide = 4;
    [SerializeField] private float spraySpeed = 4.0f;
    [SerializeField] private float sprayUpward = 0.6f;       // arc
    [SerializeField] private float spraySpread = 0.35f;      // random angle spread
    [SerializeField] private float zDepth = 0f;              // set if you need sorting

    [Header("Splatter")]
    [SerializeField] private float splatWidth = 0.10f;
    [SerializeField] private float splatLengthMin = 0.25f;
    [SerializeField] private float splatLengthMax = 0.9f;
    [SerializeField] private int maxSplats = 64;

    private ObjectPool<SplatterDecal> splatPool;
    private MaterialPropertyBlock mpb;
    private ParticleSystem sprayInstance;

    private readonly List<ParticleCollisionEvent> collisionEvents = new();

    private static readonly int LightColorId = Shader.PropertyToID("_LightColor");
    private static readonly int DarkColorId  = Shader.PropertyToID("_DarkColor");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        mpb = new MaterialPropertyBlock();

        splatPool = new ObjectPool<SplatterDecal>(
            createFunc: () => Instantiate(splatterPrefab, transform),
            actionOnGet: s => s.gameObject.SetActive(true),
            actionOnRelease: s => s.gameObject.SetActive(false),
            actionOnDestroy: s => Destroy(s.gameObject),
            collectionCheck: false,
            defaultCapacity: 24,
            maxSize: maxSplats
        );

        // One spray instance we reuse (no GC churn)
        sprayInstance = Instantiate(sprayPrefab, transform);
        sprayInstance.gameObject.SetActive(false);

        // Ensure this object receives OnParticleCollision callbacks
        var col = sprayInstance.collision;
        col.sendCollisionMessages = true;
    }

    /// Call this from anywhere.
    public void SprayBothSides(Vector3 center, float halfWidthWorld, CgaPalette.Pair palette)
    {
        var (light, dark) = CgaPalette.GetPair(palette);

        // left side
        Spray(center + new Vector3(-halfWidthWorld, 0f, 0f), Vector2.left, light, dark);

        // right side
        Spray(center + new Vector3(+halfWidthWorld, 0f, 0f), Vector2.right, light, dark);
    }

    public void SprayDirectional(Vector3 center, float halfWidthWorld, Vector2 impactDir, CgaPalette.Pair palette)
    {
        var (light, dark) = CgaPalette.GetPair(palette);

        Vector2 d = impactDir.sqrMagnitude > 0.0001f ? impactDir.normalized : Vector2.right;

        // Start the system once
        sprayInstance.transform.position = new Vector3(center.x, center.y, zDepth);
        sprayInstance.gameObject.SetActive(true);
        sprayInstance.Clear(true);
        sprayInstance.Simulate(0f, true, true);
        sprayInstance.Play(true);

        // Set colors once
        var r = sprayInstance.GetComponent<ParticleSystemRenderer>();
        r.GetPropertyBlock(mpb);
        mpb.SetColor(LightColorId, light);
        mpb.SetColor(DarkColorId, dark);
        r.SetPropertyBlock(mpb);

        // Emit from BOTH side positions but with the SAME forward direction d
        EmitBurstFrom(center + new Vector3(-halfWidthWorld, 0f, 0f), d);
        EmitBurstFrom(center + new Vector3(+halfWidthWorld, 0f, 0f), d);

        // Optional: stop emitting (not strictly needed if Looping is off)
        sprayInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    
}

private void EmitBurstFrom(Vector3 pos, Vector2 dir)
{
    float coneHalfAngle = 18f;
    var emitParams = new ParticleSystem.EmitParams();

    for (int i = 0; i < particlesPerSide; i++)
    {
        float angle = Random.Range(-coneHalfAngle, coneHalfAngle);
        Vector2 d = Rotate(dir, angle);

        Vector2 lift = new Vector2(0f, sprayUpward);
        float spd = Random.Range(2.5f, 4.5f) * spraySpeed;

        Vector2 v2 = (d + lift).normalized * spd;

        emitParams.position = pos; // IMPORTANT: per-particle spawn position
        emitParams.velocity = new Vector3(v2.x, v2.y, 0f);
        emitParams.startSize = Random.Range(0.18f, 0.28f);

        sprayInstance.Emit(emitParams, 1);
    }
}

private static Vector2 Rotate(Vector2 v, float degrees)
{
    float rad = degrees * Mathf.Deg2Rad;
    float s = Mathf.Sin(rad);
    float c = Mathf.Cos(rad);
    return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
}

public void Spray(Vector3 pos, Vector2 outwardDir, Color32 light, Color32 dark)
{
    // Ensure a fresh burst every call
    sprayInstance.transform.position = new Vector3(pos.x, pos.y, zDepth);
    sprayInstance.gameObject.SetActive(true);
    sprayInstance.Clear(true);
    sprayInstance.Simulate(0f, true, true);
    sprayInstance.Play(true);

    // Material colors
    var r = sprayInstance.GetComponent<ParticleSystemRenderer>();
    r.GetPropertyBlock(mpb);
    mpb.SetColor(LightColorId, light);
    mpb.SetColor(DarkColorId, dark);
    r.SetPropertyBlock(mpb);

    Vector2 baseDir = outwardDir.sqrMagnitude > 0.0001f ? outwardDir.normalized : Vector2.right;

    var emitParams = new ParticleSystem.EmitParams();

    float coneHalfAngle = 18f; // tune
    for (int i = 0; i < particlesPerSide; i++)
    {
        float angle = Random.Range(-coneHalfAngle, coneHalfAngle);
        Vector2 dir = Rotate(baseDir, angle);

        Vector2 lift = new Vector2(0f, sprayUpward);
        float spd = Random.Range(2.5f, 4.5f) * spraySpeed;

        Vector2 v2 = (dir + lift).normalized * spd;

        emitParams.velocity = new Vector3(v2.x, v2.y, 0f);
        emitParams.startSize = Random.Range(0.18f, 0.28f);
        sprayInstance.Emit(emitParams, 1);
    }
}

    private void OnParticleCollision(GameObject other)
    {
        // This method must be on the same GameObject as the ParticleSystem to receive events
        // In this setup, put BloodFx on the SAME object as sprayInstance OR route events (see note below).
    }

    // If you keep BloodFx separate from sprayInstance, put this method on the sprayInstance GameObject script instead.
    public void HandleParticleCollision(GameObject other)
    {
        int count = ParticlePhysicsExtensions.GetCollisionEvents(sprayInstance, other, collisionEvents);
        if (count == 0) return;

        // Grab the colors from the current particle renderer MPB
        var r = sprayInstance.GetComponent<ParticleSystemRenderer>();
        r.GetPropertyBlock(mpb);
        Color light = mpb.GetColor(LightColorId);
        Color dark  = mpb.GetColor(DarkColorId);

        for (int i = 0; i < count; i++)
        {
            Vector3 p = collisionEvents[i].intersection;

            var splat = splatPool.Get();
            float len = Random.Range(splatLengthMin, splatLengthMax);
            splat.Init(p, splatWidth, len, (Color32)light, (Color32)dark);
        }
    }

    public void ReleaseSplat(SplatterDecal s) => splatPool.Release(s);
}