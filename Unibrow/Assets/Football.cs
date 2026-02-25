using UnityEngine;

public class Football : MonoBehaviour
{
    [SerializeField] private float throwSpeed = 18f;
    [SerializeField] private float throwUp = 4f;

    [Header("Spin / Spiral")]
    [SerializeField] private float spinDegPerSec = 720f; // 360–1200
    [SerializeField] private float wobbleTorque = 0.5f;  // small randomness

    [Header("Impact feel")]
    [SerializeField] private float bounceSpinBoost = 0.25f; // adds a bit of tumble on hits

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Throw(int dir)
    {
        // Launch with a slight arc
        rb.linearVelocity = new Vector2(throwSpeed * dir, throwUp);

        // Add spiral spin (Unity uses degrees/sec for angularVelocity)
        rb.angularVelocity = -dir * spinDegPerSec;
    }

    void FixedUpdate()
    {
        // Tiny wobble so it doesn't look perfectly mechanical
        rb.AddTorque(Random.Range(-wobbleTorque, wobbleTorque), ForceMode2D.Force);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        // On impact, add a little extra tumble proportional to hit strength
        float hit = c.relativeVelocity.magnitude;
        rb.angularVelocity += Random.Range(-1f, 1f) * hit * spinDegPerSec * bounceSpinBoost;
    }
}
