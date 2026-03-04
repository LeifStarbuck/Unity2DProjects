using UnityEngine;

public class SpiderHeadTrigger : MonoBehaviour
{
    [SerializeField] private SpiderPatrolSquish spider;

    void Reset()
    {
        spider = GetComponentInParent<SpiderPatrolSquish>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (spider != null)
            spider.TryStompFromPlayer(other);
    }
}