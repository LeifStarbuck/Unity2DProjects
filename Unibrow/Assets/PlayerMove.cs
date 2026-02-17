using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private FeetGroundCheck feet;

    private bool isGrounded;
    private int facing = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;


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
        
        if (kb.spaceKey.wasPressedThisFrame)
        {
            Debug.Log($"SPACE: feetRef={(feet != null)} grounded={(feet != null && feet.IsGrounded)}");
        }

        if (kb.spaceKey.wasPressedThisFrame && feet != null && feet.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }



}
