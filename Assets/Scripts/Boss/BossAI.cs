using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public Transform player;           // 따라갈 플레이어
    public float chaseRange = 8f;      // 추적 시작 거리
    public float moveSpeed = 2f;       // 추적 속도

    public float attackInterval = 4f;  // 돌진 주기
    public float dashSpeed = 6f;       // 돌진 속도
    public float dashDuration = 0.5f;  // 돌진 시간

    private Rigidbody2D rb;            // Rigidbody2D 컴포넌트
    private SpriteRenderer sr;         // 스프라이트 렌더러 (색상, 좌우 반전용)

    private enum State { Idle, Chase, Dash } // 보스 상태 정의
    private State currentState = State.Idle; // 현재 상태

    private Vector2 movement;          // 추적 방향 저장
    private bool isDashing = false;    // 돌진 중인지 여부

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();               // Rigidbody 연결
        sr = GetComponent<SpriteRenderer>();            // SpriteRenderer 연결
        InvokeRepeating("TryDash", 2f, attackInterval); // 일정 시간마다 돌진 시도
    }

    void Update()
    {
        if (isDashing) return; // 돌진 중일 땐 아무 것도 안 함

        float distance = Vector2.Distance(transform.position, player.position); // 보스와 플레이어 거리 계산

        if (distance <= chaseRange)
        {
            currentState = State.Chase; // 일정 거리 안에 있으면 추적 상태
        }
        else
        {
            currentState = State.Idle; // 멀리 있으면 멈춤
        }

        if (currentState == State.Chase)
        {
            Vector2 dir = (player.position - transform.position).normalized; // 방향 계산
            movement = dir;

            if (dir.x != 0)
                sr.flipX = dir.x > 0; // 방향에 따라 좌우 반전
        }
        else
        {
            movement = Vector2.zero; // 정지
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return; // 돌진 중일 땐 이동 금지

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void TryDash()
    {
        if (isDashing || player == null) return; // 이미 돌진 중이면 무시

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= chaseRange)
        {
            StartCoroutine(Dash()); // 코루틴 실행
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        currentState = State.Dash;

        // 🔄 플레이어를 향해 x축 방향만 계산 (y축 무시)
        float dirX = player.position.x - transform.position.x;
        Vector2 dashDir = new Vector2(Mathf.Sign(dirX), 0); // 좌우 방향만 계산 (정규화된 벡터)

        if (dashDir.x != 0)
            sr.flipX = dashDir.x > 0; // 방향에 따라 좌우 반전

        // 🔴 준비 시간: 빨간색 표시
        sr.color = Color.red;
        yield return new WaitForSeconds(1f); // 1초 대기

        // 🟢 돌진 시작
        float timer = 0f;
        while (timer < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed; // x축으로만 돌진
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;        // 멈춤
        sr.color = Color.white;            // 색상 복원
        isDashing = false;
        currentState = State.Idle;
    }


    // ⚠️ 나중에 충돌 시 데미지 주기용 코드
    /*
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(10);
        }
    }
    */
}
