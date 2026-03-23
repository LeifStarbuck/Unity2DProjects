using UnityEngine;
using UnityEngine.InputSystem;

public class BallCatcher : MonoBehaviour
{
    [Header("Throw")]
    [SerializeField] private Transform hands;
    [SerializeField] private float throwSpeed = 30f;
    [SerializeField] private float throwUpForce = 5f;

    [Header("Lob")]
    [SerializeField] private float lobSpeed = 10f;
    [SerializeField] private float lobUpForce = 30f;

    [SerializeField] private Transform catchZone;
    [SerializeField] private float recatchDelay = 0.25f;
    private float recatchTimer = 0f;

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

        if (recatchTimer > 0f)
            recatchTimer -= Time.deltaTime;

        if (kb.eKey.wasPressedThisFrame && heldBallRb != null)
        {
            bool lob = kb.wKey.isPressed || kb.upArrowKey.isPressed;
            ThrowBall(lob);
        }
    }

    public void TryCatch(GameObject ball)
    {
        if (heldBallRb != null) return;
        if (recatchTimer > 0f) return;
        if (!ball.CompareTag("Basketball")) return;

        heldBallRb = ball.GetComponent<Rigidbody2D>();
        heldBallCol = ball.GetComponent<Collider2D>();

        if (heldBallRb == null || heldBallCol == null)
        {
            heldBallRb = null;
            return;
        }

        heldBallRb.linearVelocity = Vector2.zero;
        heldBallRb.angularVelocity = 0f;
        heldBallRb.simulated = false;

        ball.transform.SetParent(hands);
        ball.transform.localPosition = Vector3.zero;
        ball.transform.localRotation = Quaternion.identity;

        if (playerCol != null && heldBallCol != null)
            Physics2D.IgnoreCollision(heldBallCol, playerCol, true);
    }

    void ThrowBall(bool lob)
    {
        GameObject ball = heldBallRb.gameObject;
        Collider2D thrownCol = heldBallCol;

        recatchTimer = recatchDelay;

        ball.transform.SetParent(null);
        heldBallRb.simulated = true;

        ball.transform.position = hands.position + new Vector3(0.6f * facing, 0f, 0f);

        float xSpeed = lob ? lobSpeed : throwSpeed;
        float ySpeed = lob ? lobUpForce : throwUpForce;

        heldBallRb.linearVelocity = new Vector2(
            xSpeed * facing,
            ySpeed
        );

        StartCoroutine(ReenableCollisionSoon(thrownCol));

        heldBallRb = null;
        heldBallCol = null;
    }

    System.Collections.IEnumerator ReenableCollisionSoon(Collider2D col)
    {
        yield return new WaitForSeconds(0.15f);

        if (col != null && playerCol != null)
            Physics2D.IgnoreCollision(col, playerCol, false);
    }

    public void SetFacing(int dir)
    {
        facing = (dir >= 0) ? 1 : -1;

        hands.localPosition = new Vector3(
            Mathf.Abs(handsLocalPosRight.x) * facing,
            handsLocalPosRight.y,
            handsLocalPosRight.z
        );

        if (catchZone != null)
        {
            catchZone.localPosition = new Vector3(
                Mathf.Abs(catchZoneLocalPosRight.x) * facing,
                catchZoneLocalPosRight.y,
                catchZoneLocalPosRight.z
            );
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryCatch(collision.gameObject);
    }
}