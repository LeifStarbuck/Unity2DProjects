using UnityEngine;
using UnityEngine.InputSystem;

public class BallCatcher : MonoBehaviour
{
    [SerializeField] private Transform hands;
    [SerializeField] private float throwSpeed = 30f;

    [SerializeField] private Transform catchZone; 
    [SerializeField] private float recatchDelay = 0.25f;
    private float recatchTimer = 0f;

    [SerializeField] private float throwUpForce = 5f;







    private Vector3 handsLocalPosRight;
    private Vector3 catchZoneLocalPosRight;

    private Rigidbody2D heldBallRb;
    private Collider2D heldBallCol;

    private Collider2D playerCol;
    private int facing = 1; 

    void Awake()
    {
        playerCol = GetComponent<Collider2D>();

        handsLocalPosRight = hands.localPosition;
        if (catchZone != null) catchZoneLocalPosRight = catchZone.localPosition;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (recatchTimer > 0f) recatchTimer -= Time.deltaTime;


        // Throw on E
        if (kb.eKey.wasPressedThisFrame && heldBallRb != null)
            ThrowBall();

    }

    // Called by the CatchZone trigger (see next script) or you can place this directly on CatchZone.
    public void TryCatch(GameObject ball)
    {
        if (heldBallRb != null) return; // already holding one

        if (recatchTimer > 0f) return;  // check recatch cooldown for balls

        if (!ball.CompareTag("Basketball")) return;

        heldBallRb = ball.GetComponent<Rigidbody2D>();
        heldBallCol = ball.GetComponent<Collider2D>();
        if (heldBallRb == null || heldBallCol == null) { heldBallRb = null; return; }

        // Stop physics and "stick" to hands
        heldBallRb.linearVelocity = Vector2.zero;
        heldBallRb.angularVelocity = 0f;
        heldBallRb.simulated = false; // key line: removes it from physics while held

        // Parent to hands and snap position/rotation
        ball.transform.SetParent(hands);
        ball.transform.localPosition = Vector3.zero;
        ball.transform.localRotation = Quaternion.identity;

        // Prevent immediate self-collisions when thrown
        if (playerCol != null && heldBallCol != null)
            Physics2D.IgnoreCollision(heldBallCol, playerCol, true);
    }

    void ThrowBall()
    {
        GameObject ball = heldBallRb.gameObject;

        recatchTimer = recatchDelay;

        // Unparent
        ball.transform.SetParent(null);

        // Re-enable physics
        heldBallRb.simulated = true;

        // Put it slightly in front of the hands (prevents collision jitter)
        ball.transform.position = hands.position + new Vector3(0.6f * facing, 0f, 0f);

        // Launch
heldBallRb.linearVelocity = new Vector2(
    throwSpeed * facing,
    Mathf.Max(throwUpForce, heldBallRb.linearVelocity.y)
);



        // After a short time, re-enable collisions with player (optional)
        StartCoroutine(ReenableCollisionSoon());

        heldBallRb = null;
        heldBallCol = null;
    }

    System.Collections.IEnumerator ReenableCollisionSoon()
    {
        var col = heldBallCol;
        yield return new WaitForSeconds(0.15f);

        if (col != null && playerCol != null)
            Physics2D.IgnoreCollision(col, playerCol, false);
    }

    // Optional: let other scripts (PlayerMove) update facing so throw direction matches.
    public void SetFacing(int dir)
    {
        facing = (dir >= 0) ? 1 : -1;

        // Mirror Hands
        hands.localPosition = new Vector3(
            Mathf.Abs(handsLocalPosRight.x) * facing,
            handsLocalPosRight.y,
            handsLocalPosRight.z
        );

        // Mirror CatchZone too (optional but recommended)
        if (catchZone != null)
        {
            catchZone.localPosition = new Vector3(
                Mathf.Abs(catchZoneLocalPosRight.x) * facing,
                catchZoneLocalPosRight.y,
                catchZoneLocalPosRight.z
            );
        }
        Debug.Log($"[SetFacing] facing={facing} catchZone={catchZone.name} localX={catchZone.localPosition.x}");


}


    void OnCollisionEnter2D(Collision2D collision)
    {
        TryCatch(collision.gameObject);
    }

}
