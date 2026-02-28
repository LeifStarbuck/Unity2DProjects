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
    [SerializeField] private float splatLengthMin = 0.10f;
    [SerializeField] private float splatLengthMax = 0.30f;
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

    public void Spray(Vector3 pos, Vector2 outwardDir, Color32 light, Color32 dark)
    {
        sprayInstance.transform.position = new Vector3(pos.x, pos.y, zDepth);
        sprayInstance.gameObject.SetActive(true);

        // Set circle material colors (procedural concentric circles)
        var r = sprayInstance.GetComponent<ParticleSystemRenderer>();
        r.GetPropertyBlock(mpb);
        mpb.SetColor(LightColorId, light);
        mpb.SetColor(DarkColorId, dark);
        r.SetPropertyBlock(mpb);

        // Emit with directional bias (so you can do left/right “from sides”)
        var emitParams = new ParticleSystem.EmitParams();
        for (int i = 0; i < particlesPerSide; i++)
        {
            Vector2 rand = Random.insideUnitCircle * spraySpread;
            Vector2 dir = (outwardDir + rand).normalized;

            Vector3 vel = new Vector3(dir.x, dir.y + sprayUpward, 0f) * spraySpeed;
            emitParams.velocity = vel;

            // Experimental
            emitParams.velocity = new Vector3(dir.x, dir.y + sprayUpward, 0f) * Random.Range(2.5f, 4.5f);

            sprayInstance.Emit(emitParams, 1);
        }

        // stop soon; particles themselves have lifetime
        sprayInstance.Stop(withChildren: true, stopBehavior: ParticleSystemStopBehavior.StopEmitting);
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