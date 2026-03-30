using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SavePointGlowFx : MonoBehaviour
{
    [Header("Spawn Range")]
    [SerializeField] private float width = 2.5f;
    [SerializeField] private float height = 0.1f;

    [Header("Emission")]
    [SerializeField] private float particlesPerSecond = 10f;

    [Header("Motion")]
    [SerializeField] private Vector2 upwardVelocity = new Vector2(0.02f, 0.18f);
    [SerializeField] private float horizontalDrift = 0.03f;

    [Header("Lifetime / Size")]
    [SerializeField] private Vector2 lifetimeRange = new Vector2(1.8f, 3.0f);
    [SerializeField] private Vector2 sizeRange = new Vector2(0.04f, 0.10f);

    [Header("Fade")]
    [SerializeField] private float startAlpha = 0.9f;
    [SerializeField] private float midAlpha = 1.0f;
    [SerializeField] private float endAlpha = 0.0f;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ConfigureSystem();
    }

    private void OnValidate()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        if (ps != null)
            ConfigureSystem();
    }

private void ConfigureSystem()
{
    var main = ps.main;
    main.loop = true;
    main.playOnAwake = true;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
    main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
    main.startSpeed = 0f;
    main.maxParticles = 256;

    var emission = ps.emission;
    emission.enabled = true;
    emission.rateOverTime = particlesPerSecond;

    var shape = ps.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Box;
    shape.scale = new Vector3(width, height, 0.01f);

    var velocityOverLifetime = ps.velocityOverLifetime;
    velocityOverLifetime.enabled = true;
    velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-horizontalDrift, horizontalDrift);
    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(upwardVelocity.x, upwardVelocity.y);
    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 0f);

    var colorOverLifetime = ps.colorOverLifetime;
    colorOverLifetime.enabled = true;

    Gradient gradient = new Gradient();
    gradient.SetKeys(
        new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0f),
            new GradientColorKey(new Color(1f, 0.95f, 0.35f), 0.35f),
            new GradientColorKey(new Color(1f, 0.9f, 0.2f), 0.7f),
            new GradientColorKey(new Color(1f, 0.9f, 0.2f), 1f),
        },
        new GradientAlphaKey[]
        {
            new GradientAlphaKey(startAlpha, 0f),
            new GradientAlphaKey(midAlpha, 0.25f),
            new GradientAlphaKey(midAlpha, 0.65f),
            new GradientAlphaKey(endAlpha, 1f),
        }
    );

    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

    var noise = ps.noise;
    noise.enabled = true;
    noise.strength = 0.03f;
    noise.frequency = 0.4f;
    noise.scrollSpeed = 0.2f;

    var renderer = ps.GetComponent<ParticleSystemRenderer>();
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
}
}