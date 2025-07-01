using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // movement settings
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    // ground check
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    private PolygonCollider2D polyCollider;
    private Vector2[] originalPoints;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
        polyCollider = GetComponent<PolygonCollider2D>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        // 콜라이더 좌우반전용
        originalPoints = polyCollider.points;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        // 좌우 입력 방향으로 반전 및 콜라이더 갱신
        if (moveInput.x != 0)
        {
            bool shouldFlip = moveInput.x < 0;

            if (sr.flipX != shouldFlip)
            {
                sr.flipX = shouldFlip;

                // flip 시에만 콜라이더 점 반전
                Vector2[] flippedPoints = new Vector2[originalPoints.Length];
                for (int i = 0; i < originalPoints.Length; i++)
                {
                    flippedPoints[i] = new Vector2(originalPoints[i].x * (shouldFlip ? -1 : 1), originalPoints[i].y);
                }
                polyCollider.points = flippedPoints;
            }
        }

        // 점프
        if (jumpAction.triggered && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 애니메이션 갱신
        anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        anim.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        // 땅 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 이동
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
}