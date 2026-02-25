using UnityEngine;

public class CatchZone : MonoBehaviour
{
    private BallCatcher catcher;

    void Awake()
    {
        catcher = GetComponentInParent<BallCatcher>();
        //Debug.Log($"[CatchZone] Awake. catcherFound={(catcher != null)}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"[CatchZone] ENTER other={other.name} tag={other.tag} layer={LayerMask.LayerToName(other.gameObject.layer)} isTrigger={other.isTrigger}");
        catcher?.TryCatch(other.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Important: if the ball starts overlapped (or enters very slowly), Stay makes catching reliable.
        catcher?.TryCatch(other.gameObject);
    }
}
