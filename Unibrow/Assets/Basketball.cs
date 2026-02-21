using UnityEngine;

public class Basketball : MonoBehaviour
{

    [SerializeField] private GameObject dustFXPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnCollisionEnter2D(Collision2D collision)
    {       
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && collision.relativeVelocity.magnitude > 8f)
        {
            Vector2 hitPoint = collision.GetContact(0).point;
            Instantiate(dustFXPrefab, hitPoint, Quaternion.identity);
        }
    }

}
