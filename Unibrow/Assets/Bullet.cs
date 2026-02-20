using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifeSeconds = 2f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Fire(int dir)
    {
        rb.linearVelocity = new Vector2(dir * speed, 0f);
        Destroy(gameObject, lifeSeconds);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
