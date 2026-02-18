using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private FeetGroundCheck feet;

    [Header("Jump Tuning")]
    [SerializeField] private float jumpImpulse = 10f;      // initial kick
    [SerializeField] private float jumpCutMultiplier = 0.5f; // how much to reduce upward speed on early release (0.3–0.7)
    [SerializeField] private float holdForce = 20f;        // extra upward force while holding
    [SerializeField] private float maxHoldTime = 0.15f;    // seconds you can “hold to go higher”


    private bool isGrounded;
    private int facing = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private float holdTimer = 0f;
private bool isJumping = false;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Move();
        Jump();
    }


    void Move()
    {

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;

        if (kb.leftArrowKey.isPressed) x = -1f;
        if (kb.rightArrowKey.isPressed) x = 1f;

        //Apply movement
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        //Flip sprite
        if (x > 0)
        {
            sr.flipX = false;
            facing = 1; //facing right
        }
        else if (x < 0)
        {
            sr.flipX = true;
            facing = -1; //facing left
        }

    }

void Jump()
{
    var kb = Keyboard.current;
    if (kb == null) return;

    // Jump start
    if (kb.spaceKey.wasPressedThisFrame && feet != null && feet.IsGrounded)
    {
        // Reset vertical velocity so jump is consistent
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Initial jump impulse
        rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);

        // Enable hold-to-jump-higher window
        isJumping = true;
        holdTimer = maxHoldTime;
    }

    // Hold: apply a bit of upward force while the player holds Space (limited time)
    if (isJumping && kb.spaceKey.isPressed && holdTimer > 0f)
    {
        rb.AddForce(Vector2.up * holdForce * Time.deltaTime, ForceMode2D.Force);
        holdTimer -= Time.deltaTime;
    }

    // Early release: cut jump short by reducing upward velocity
    if (kb.spaceKey.wasReleasedThisFrame)
    {
        if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        isJumping = false;
        holdTimer = 0f;
    }

    // Stop holding once you start falling
    if (rb.linearVelocity.y <= 0f)
    {
        isJumping = false;
    }
}




}
