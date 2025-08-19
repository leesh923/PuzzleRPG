using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimController : MonoBehaviour
{
    [Header("Damping")]
    [SerializeField] private float damp = 0.0f; // 대각선 반응 (0 권장)

    Animator animator;
    SpriteRenderer spriteRenderer;
    PlayerController playerControl;

    // 전환 기준과 동일하게 유지
    private const float SPEED_EPS = 0.01f;          // 애니메이터 전환기준
    private const float INPUT_EPS = 0.0001f;        // 입력 전환(마지막 방향 갱신용도)

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerControl = GetComponent<PlayerController>();


        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        // 시작 시 방향 초기화
        animator.SetFloat("LastMoveX", 0f);
        animator.SetFloat("LastMoveY", -1f);
        animator.SetFloat("Speed", 0f);
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(inputX, inputY);
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = playerControl ? playerControl.Velocity.sqrMagnitude : (inputX*inputX + inputY*inputY);
        animator.SetFloat("Speed", speed);

        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);

        // 블렌드용 방향은 입력 그대로 (댐핑은 0 또는 아주 작게)
        if (damp > 0f)
        {
            animator.SetFloat("MoveX", input.x, damp, Time.deltaTime);
            animator.SetFloat("MoveY", input.y, damp, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("MoveX", input.x);
            animator.SetFloat("MoveY", input.y);
        }

        // Idle용 마지막 방향
        if (input.sqrMagnitude > INPUT_EPS)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            {
                animator.SetFloat("LastMoveX", Mathf.Sign(input.x));
                animator.SetFloat("LastMoveY", 0f);
            }
            else
            {
                animator.SetFloat("LastMoveX", 0f);
                animator.SetFloat("LastMoveY", Mathf.Sign(input.y));
            }
        }

    }

    void LateUpdate()
    {
        float speed = animator.GetFloat("Speed");
        float mx = animator.GetFloat("MoveX");
        float lastX = animator.GetFloat("LastMoveX");

        // 걷는 중엔 MoveX, 정지 시엔 LastMoveX 기준으로 좌/우 판정
        float basis = (speed > 0.01f) ? mx : lastX;

        // 왼쪽 성분만 있으면 좌로 뒤집기 (수평 우세 조건 제거)
        spriteRenderer.flipX = basis < -0.0001f;
    }
}
