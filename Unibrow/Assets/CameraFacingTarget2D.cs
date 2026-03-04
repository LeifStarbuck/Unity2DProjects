using UnityEngine;

public class CameraFacingTarget2D : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;      // Player child: CameraTarget
    [SerializeField] private SpriteRenderer sprite;       // SpriteRenderer that flipX's
    [SerializeField] private float lookAheadX = 2.0f;     // extra lead amount
    [SerializeField] private float smooth = 12f;

    [Header("Optional vertical bias (added to base Y)")]
    [SerializeField] private float jumpUpY = 0.8f;
    [SerializeField] private float fallDownY = -1.8f;
    [SerializeField] private float ySmooth = 8f;
    [SerializeField] private float yVelDeadband = 0.2f;
    [SerializeField] private Rigidbody2D rb;

    private Vector3 _baseLocal; // <-- your “8” lives here
    private float _x, _y;

    void Awake()
    {
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (cameraTarget) _baseLocal = cameraTarget.localPosition;
    }

    void OnEnable()
    {
        if (cameraTarget) _baseLocal = cameraTarget.localPosition;
        _x = 0f;
        _y = 0f;
    }

    void LateUpdate()
    {
        if (!cameraTarget || !sprite) return;

        // facing: flipX=true usually means facing left
        int facing = sprite.flipX ? -1 : 1;

        float targetX = _baseLocal.x + (lookAheadX * facing);

        _x = Mathf.Lerp(_x, targetX, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        // Optional Y bias based on vertical velocity
        float targetY = _baseLocal.y;
        if (rb)
        {
            float vy = rb.linearVelocity.y;
            if (vy > yVelDeadband) targetY = _baseLocal.y + jumpUpY;
            else if (vy < -yVelDeadband) targetY = _baseLocal.y + fallDownY;
        }

        _y = Mathf.Lerp(_y, targetY, 1f - Mathf.Exp(-ySmooth * Time.deltaTime));

        cameraTarget.localPosition = new Vector3(_x, _y, _baseLocal.z);
    }
}