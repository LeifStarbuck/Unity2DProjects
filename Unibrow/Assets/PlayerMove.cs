using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    [SerializeField] private ParticleSystem dustPrefab;
    //[SerializeField] private Transform feet;
    [SerializeField] private FeetGroundCheck feet;

    [SerializeField] private Vector2 dustOffset = new Vector2(0f, -0.5f);

    [Header("Player Sprite")]
    [SerializeField] private SpriteRenderer sr;


    [Header("Jump Tuning")]
    [SerializeField] private float jumpImpulse = 10f;      // initial kick
    [SerializeField] private float jumpCutMultiplier = 0.5f; // how much to reduce upward speed on early release (0.3–0.7)
    [SerializeField] private float holdForce = 20f;        // extra upward force while holding
    [SerializeField] private float maxHoldTime = 0.15f;    // seconds you can “hold to go higher”
    [SerializeField] private float coyoteTime = 0.12f;


    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.4f;

    [SerializeField] private float jumpBufferTime = 0.12f;
    private float jumpBufferCounter = 0f;

    [Header("Gun")]
[SerializeField] private GameObject bulletPrefab;
[SerializeField] private Transform firePoint;
[SerializeField] private float fireCooldown = 0.15f;
private float fireCooldownLeft = 0f;


    private bool isGrounded;
    private int facing = 1;

    private Rigidbody2D rb;

    private float holdTimer = 0f;
    private bool isJumping = false;
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;

    private bool canDash = true;


    private float coyoteTimer = 0f;




private float moveX;
private PlayerHurtResponse hurt;
private BallCatcher catcher;

void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    hurt = GetComponent<PlayerHurtResponse>();
    catcher = GetComponent<BallCatcher>();
}

void Update()
{
    var kb = Keyboard.current;
    if (kb == null) return;

    moveX = 0f;
    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX = -1f;
    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX = 1f;

    if (moveX > 0)
    {
        sr.flipX = false;
        facing = 1;
    }
    else if (moveX < 0)
    {
        sr.flipX = true;
        facing = -1;
    }

    if (catcher != null) catcher.SetFacing(facing);

    if (feet != null && feet.IsGrounded) canDash = true;

    jumpBufferCounter -= Time.deltaTime;
    if (kb.spaceKey.wasPressedThisFrame) jumpBufferCounter = jumpBufferTime;

    if (feet != null && feet.IsGrounded) coyoteTimer = coyoteTime;
    else coyoteTimer -= Time.deltaTime;

    fireCooldownLeft -= Time.deltaTime;

    Dash();
    Jump();
    Shoot();
}

void FixedUpdate()
{
    if (isDashing)
    {
        rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);
        return;
    }

    bool lockX = (hurt != null && hurt.LockHorizontal);
    if (!lockX)
    {
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }
}

    void Move()
    {

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x = 1f;

        // If player is currently being hurt, lock horizontal movement
        var hurt = GetComponent<PlayerHurtResponse>();
        bool lockX = (hurt != null && hurt.LockHorizontal);

        if (!lockX)
        {
            rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
        }

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

        //Flip ball if held
        var catcher = GetComponent<BallCatcher>();
        if (catcher != null) catcher.SetFacing(facing);

    }

    void Jump()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (jumpBufferCounter > 0f && coyoteTimer > 0f && !isJumping)
        {

            SpawnDust();

            // Reset vertical velocity so jump is consistent
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // Initial jump impulse
            rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);

            // Enable hold-to-jump-higher window
            isJumping = true;
            holdTimer = maxHoldTime;

            jumpBufferCounter = 0f;   // ✅ consume buffer here

            coyoteTimer = 0f;
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

    void Dash()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // tick cooldown
        if (dashCooldownLeft > 0f)
            dashCooldownLeft -= Time.deltaTime;

        // start dash on C
        if (!isDashing && canDash && dashCooldownLeft <= 0f && kb.cKey.wasPressedThisFrame)
        {
            SpawnDust();
            canDash = false;
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownLeft = dashCooldown;

            // optional: flatten vertical motion so dash is clean
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        // during dash: force horizontal velocity in facing direction
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;

            rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);

            if (dashTimeLeft <= 0f)
            {
                isDashing = false;
            }
        }
    }

    void SpawnDust()
    {
        if (dustPrefab == null || feet == null) return;

        Vector3 spawnPos = feet.transform.position + (Vector3)dustOffset;

        ParticleSystem dust = Instantiate(dustPrefab, spawnPos, Quaternion.identity);
        dust.Play();
        Destroy(dust.gameObject, 1f);
    }
void Shoot()
{
    var kb = Keyboard.current;
    if (kb == null) return;

    fireCooldownLeft -= Time.deltaTime;

    if (kb.kKey.wasPressedThisFrame && fireCooldownLeft <= 0f)
    {
        // Spawn
        GameObject obj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Prevent bullet from knocking the player
        var bulletCol = obj.GetComponent<Collider2D>();
        var playerCol = GetComponent<Collider2D>();
        if (bulletCol != null && playerCol != null)
            Physics2D.IgnoreCollision(bulletCol, playerCol);

        // Fire
        var bullet = obj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Fire(facing);

        fireCooldownLeft = fireCooldown;
    }
}


}
