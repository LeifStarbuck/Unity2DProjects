using UnityEngine;

public class ParticleCollisionBridge : MonoBehaviour
{
    [SerializeField] private BloodFx fx;

    private void Awake()
    {
        if (!fx) fx = FindFirstObjectByType<BloodFx>();
    }

    private void OnParticleCollision(GameObject other)
    {
        fx.HandleParticleCollision(other);
    }
}