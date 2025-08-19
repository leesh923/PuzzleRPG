using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 3f;

    public Vector2 Velocity { get; private set; }     // 현재 속도
    public Vector2 RawInputDir { get; private set; }    // 대각선 포함한 입력 (MoveX/Y) 
    public Vector2 LastInputAxisSnap { get; private set; } // 마지막 입력 방향(LastMoveX/Y)

    Rigidbody2D rb;
    Vector2 input;
    Vector2 desiredVelocity;

    private const float INPUT_EPS = 0.0001f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;            // 탑다운이면 보통 0
        rb.freezeRotation = true;        // 회전 고정
    }

    void Update()
    {
        // 입력
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        input = new Vector2(x,y);

        if (input.sqrMagnitude > 1f) input.Normalize();

        // 외부 공개용
        RawInputDir = input;
        
        // 애니메이션 참조용 데이터
        if (input.sqrMagnitude > INPUT_EPS)
        {
            // 수평 우선 정리: Idle_Side 진입 정확도 향상
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                LastInputAxisSnap = new Vector2(Mathf.Sign(input.x), 0f);
            else
                LastInputAxisSnap = new Vector2(0f, Mathf.Sign(input.y));
        }

        // 이동 의도
        desiredVelocity = input * moveSpeed;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = desiredVelocity;
        Velocity = rb.linearVelocity; // 애니에서 읽어감
    }
}
