using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // movement settings
    public float moveSpeed = 10f;
    public float jumpForce = 8f;
    
    //땅 Layer
    public LayerMask groundLayer;

    // ground check
    //public Transform groundCheck;
    //public float groundCheckRadius = 0.1f;

    public Transform groundCheckBoxCenter;
    public Vector2 groundBoxSize = new Vector2(2f, 0.1f); // 신발 전체 크기


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

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 5f;

    public float sustainedJumpForce = 20f; // 누르고 있는 동안 주는 추가 가속력
    public float maxSustainedJumpTime = 0.2f; // 누르고 있을 수 있는 최대 시간

    private float sustainedJumpTimer;
    private bool isJumping;





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

        //점프 시작 할때
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            groundLockCounter = groundLockTime; // 지면 판정 잠시 비활성화

            isJumping = true;
            sustainedJumpTimer = maxSustainedJumpTime;
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
            //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            isGrounded = Physics2D.OverlapBox(groundCheckBoxCenter.position, groundBoxSize, 0f, groundLayer);
        }

        // 코요테 타임 타이머
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // 이동
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // 점프 유지 입력: 점프 중에 점프 키를 짧게 누르면 낮게, 길게 누르면 높게
        if (rb.linearVelocity.y < 0) // 낙하 중
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpAction.IsPressed()) // 상승 중인데 점프키에서 손을 뗐을 때
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // 점프 유지 중이면 추가 상승 가속
        if (isJumping && jumpAction.IsPressed() && sustainedJumpTimer > 0f)
        {
            rb.linearVelocity += Vector2.up * sustainedJumpForce * Time.fixedDeltaTime;
            sustainedJumpTimer -= Time.fixedDeltaTime;
        }

        // 점프 중간에 키를 뗐거나 시간이 끝나면 유지 종료
        if (!jumpAction.IsPressed() || sustainedJumpTimer <= 0f)
        {
            isJumping = false;
        }

    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckBoxCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckBoxCenter.position, new Vector3(groundBoxSize.x, groundBoxSize.y, 0f));
        }

        // if (groundCheck != null)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        // }
    }
}
