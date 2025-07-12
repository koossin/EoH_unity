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

    // jump buffer & coyote time
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    private float coyoteCounter;
    private float jumpBufferCounter;

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

    private float groundLockTime = 0.1f; // 점프 직후 지면 판정 무시할 시간
    private float groundLockCounter = 0f;


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

        // 방향 반전 및 콜라이더 좌우 반전
        if (moveInput.x != 0)
        {
            bool shouldFlip = moveInput.x < 0;

            if (sr.flipX != shouldFlip)
            {
                sr.flipX = shouldFlip;

                Vector2[] flippedPoints = new Vector2[originalPoints.Length];
                for (int i = 0; i < originalPoints.Length; i++)
                {
                    flippedPoints[i] = new Vector2(originalPoints[i].x * (shouldFlip ? -1 : 1), originalPoints[i].y);
                }
                polyCollider.points = flippedPoints;
            }
        }

        // 점프 입력 기억 (버퍼)
        if (jumpAction.triggered)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 점프 조건
        // if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        // {
        //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //     jumpBufferCounter = 0f;
        //     coyoteCounter = 0f;
        // }

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            groundLockCounter = groundLockTime; // 지면 판정 잠시 비활성화
        }


        // 애니메이션 업데이트
        anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        anim.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        if (groundLockCounter > 0f)
        {
            groundLockCounter -= Time.fixedDeltaTime;
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        // 땅에 닿았는지 체크
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 코요테 타임 타이머
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // 이동
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
