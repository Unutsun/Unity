using UnityEngine;

/// <summary>
/// ボールの動きと衝突判定を管理するコンポーネント
/// </summary>
public class BallController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float initialSpeed = 8f;
    [SerializeField] private float maxSpeed = 15f;

    [Header("References")]
    [SerializeField] private PaddleController paddle;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Vector3 initialOffset;

    public bool IsLaunched => isLaunched;

    // Events
    public System.Action OnBallLost;
    public System.Action OnBrickHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (paddle != null)
        {
            initialOffset = transform.position - paddle.transform.position;
        }
        ResetBall();
    }

    private void Update()
    {
        if (!isLaunched)
        {
            // パドルに追従
            if (paddle != null)
            {
                transform.position = paddle.transform.position + initialOffset;
            }

            // 発射入力チェック
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                Launch();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isLaunched)
        {
            // 速度を一定に保つ
            float currentSpeed = rb.velocity.magnitude;
            if (currentSpeed > 0.1f)
            {
                rb.velocity = rb.velocity.normalized * Mathf.Clamp(currentSpeed, initialSpeed, maxSpeed);
            }
        }
    }

    /// <summary>
    /// ボールを発射する
    /// </summary>
    public void Launch()
    {
        if (isLaunched) return;

        isLaunched = true;

        // ランダムな方向に発射（上方向）
        float randomX = Random.Range(-0.5f, 0.5f);
        Vector2 direction = new Vector2(randomX, 1f).normalized;
        rb.velocity = direction * initialSpeed;

        Debug.Log("Ball launched!");
    }

    /// <summary>
    /// ボールをリセットする
    /// </summary>
    public void ResetBall()
    {
        isLaunched = false;
        rb.velocity = Vector2.zero;

        if (paddle != null)
        {
            transform.position = paddle.transform.position + initialOffset;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched) return;

        // パドルとの衝突
        if (collision.gameObject.CompareTag("Paddle"))
        {
            HandlePaddleCollision(collision);
        }
        // ブロックとの衝突
        else if (collision.gameObject.CompareTag("Brick"))
        {
            OnBrickHit?.Invoke();
        }
        // 壁との衝突（サウンドなど追加可能）
        else if (collision.gameObject.CompareTag("Wall"))
        {
            // 壁反射音などを追加可能
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 下の境界（ボール落下）
        if (other.CompareTag("DeadZone"))
        {
            OnBallLost?.Invoke();
            ResetBall();
        }
    }

    /// <summary>
    /// パドル衝突時の反射角度計算
    /// </summary>
    private void HandlePaddleCollision(Collision2D collision)
    {
        PaddleController paddleController = collision.gameObject.GetComponent<PaddleController>();
        if (paddleController == null) return;

        // パドル上の衝突位置を計算（-1 ~ 1）
        float hitPoint = (transform.position.x - collision.transform.position.x) / paddleController.Width;
        hitPoint = Mathf.Clamp(hitPoint, -1f, 1f);

        // 反射角度を計算（最大75度）
        float maxAngle = 75f;
        float angle = hitPoint * maxAngle;

        // 新しい速度ベクトルを設定
        float speed = rb.velocity.magnitude;
        float radian = angle * Mathf.Deg2Rad;
        Vector2 newVelocity = new Vector2(Mathf.Sin(radian), Mathf.Cos(radian)).normalized * speed;
        rb.velocity = newVelocity;
    }

    /// <summary>
    /// 初期位置オフセットを設定
    /// </summary>
    public void SetInitialOffset(Vector3 offset)
    {
        initialOffset = offset;
    }

    /// <summary>
    /// パドル参照を設定
    /// </summary>
    public void SetPaddle(PaddleController newPaddle)
    {
        paddle = newPaddle;
        if (paddle != null)
        {
            initialOffset = new Vector3(0, 0.5f, 0);
        }
    }
}
