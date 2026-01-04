using UnityEngine;

/// <summary>
/// ボールの動きを制御
/// イベント駆動で疎結合に実装
/// 包丁スプライトで表示（Resources/Sprites/knife）
/// </summary>
public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]
    public float speed = 8f;

    [Header("References")]
    [SerializeField] private Transform paddle;  // Inspectorで設定可能

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Vector3 offsetFromPaddle = new Vector3(0, 0.5f, 0);
    private Transform knifeVisual;  // 包丁の見た目（回転用）
    private const float KNIFE_ROTATION_OFFSET = -50f;  // 包丁画像の初期向きオフセット（Twemoji用）

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupRigidbody();
        SetupSprite();
        SetupTrail();
    }

    void OnEnable()
    {
        GameEvents.OnBallReset += ResetBall;
        GameEvents.OnGameRestart += ResetBall;
    }

    void OnDisable()
    {
        GameEvents.OnBallReset -= ResetBall;
        GameEvents.OnGameRestart -= ResetBall;
    }

    void Start()
    {
        // SerializeFieldで設定されていない場合はFind()でフォールバック
        if (paddle == null)
        {
            paddle = GameObject.Find("Paddle")?.transform;
        }

        if (paddle != null)
        {
            Collider2D col = paddle.GetComponent<Collider2D>();
            if (col != null)
            {
                Debug.Log($"[BallController] Paddle found: pos={paddle.position}, bounds={col.bounds}");
            }
            else
            {
                Debug.LogWarning("[BallController] Paddle has NO collider!");
            }
        }
        else
        {
            Debug.LogError("[BallController] Paddle NOT found!");
        }
        ResetBall();
    }

    void SetupRigidbody()
    {
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        // CircleCollider2D を設定
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }

        // 包丁サイズ設定（4倍に拡大）
        transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        col.radius = 0.3f; // コライダーは小さめに（見た目より当たり判定は小さく）

        // Physics Material 2D を設定
        PhysicsMaterial2D mat = new PhysicsMaterial2D("BallMat");
        mat.bounciness = 1f;
        mat.friction = 0f;
        col.sharedMaterial = mat;

        Debug.Log($"[BallController] Ball setup: scale={transform.localScale}, radius={col.radius}, bounds={col.bounds}");
    }

    void SetupSprite()
    {
        // 元のSpriteRendererを無効化
        SpriteRenderer originalSr = GetComponent<SpriteRenderer>();
        if (originalSr != null)
        {
            originalSr.enabled = false;
        }

        // 包丁用の子オブジェクトを作成（回転を独立させるため）
        GameObject knifeObj = new GameObject("KnifeVisual");
        knifeObj.transform.SetParent(transform);
        knifeObj.transform.localPosition = Vector3.zero;
        knifeObj.transform.localScale = Vector3.one * 2f;  // 大きめに表示
        knifeVisual = knifeObj.transform;

        SpriteRenderer sr = knifeObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        // 包丁スプライトをResourcesから読み込む
        Sprite knifeSprite = Resources.Load<Sprite>("Sprites/knife");

        if (knifeSprite != null)
        {
            sr.sprite = knifeSprite;
            sr.color = Color.white;
            Debug.Log("[BallController] Knife sprite loaded");
        }
        else
        {
            // フォールバック：白い丸を作成
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            Vector2 center = new Vector2(16, 16);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * 32 + x] = dist < 14 ? Color.white : Color.clear;
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            sr.color = Color.white;
            Debug.Log("[BallController] Fallback circle sprite created");
        }

        // 初期回転を設定（上向きが基準）
        UpdateKnifeRotation(Vector2.up);
    }

    void UpdateKnifeRotation(Vector2 direction)
    {
        if (knifeVisual == null || direction.magnitude < 0.01f) return;

        // 進行方向から角度を計算
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // オフセットを適用して切っ先が進行方向を向くように
        knifeVisual.rotation = Quaternion.Euler(0, 0, angle - KNIFE_ROTATION_OFFSET);
    }

    void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        // 軌跡の設定（2倍サイズ）
        trail.time = 0.3f;  // 軌跡が消えるまでの時間
        trail.startWidth = 0.4f;   // 0.2 → 0.4
        trail.endWidth = 0.1f;     // 0.05 → 0.1
        trail.material = new Material(Shader.Find("Sprites/Default"));

        // グラデーション（白→透明）
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(new Color(0.8f, 0.9f, 1f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        trail.colorGradient = gradient;
        trail.sortingOrder = 5;

        Debug.Log("[BallController] Trail setup complete");
    }

    void Update()
    {
        // ゲーム状態チェック
        if (GameState.Instance != null)
        {
            var state = GameState.Instance.CurrentState;
            // GameOver, GameClear, Countdown中は処理しない
            if (state == GameStateType.GameOver || state == GameStateType.GameClear || state == GameStateType.Countdown)
            {
                return;
            }
        }

        if (!isLaunched)
        {
            FollowPaddle();
            CheckLaunchInput();
        }
        else
        {
            MaintainSpeed();
        }
    }

    void FollowPaddle()
    {
        if (paddle != null)
        {
            transform.position = paddle.position + offsetFromPaddle;
        }
    }

    void CheckLaunchInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Launch();
        }
    }

    void MaintainSpeed()
    {
        // 画面外チェック（安全策）
        CheckOutOfBounds();

        // 速度を一定に保つ
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector2 vel = rb.linearVelocity.normalized;

            // 垂直方向の速度が小さすぎる場合（水平往復防止）
            float minVerticalRatio = 0.3f; // 最低30%の垂直成分を保証
            if (Mathf.Abs(vel.y) < minVerticalRatio)
            {
                // 垂直成分を強制的に追加（進行方向に応じて上か下）
                float sign = vel.y >= 0 ? 1f : -1f;
                vel.y = sign * minVerticalRatio;
                vel = vel.normalized;
            }

            rb.linearVelocity = vel * speed;

            // 包丁を進行方向に向ける
            UpdateKnifeRotation(vel);
        }
    }

    void CheckOutOfBounds()
    {
        // 画面外に出たらボールロスト
        Camera cam = Camera.main;
        if (cam == null) return;

        float maxY = cam.orthographicSize + 2f;
        float minY = -cam.orthographicSize - 2f;
        float maxX = cam.orthographicSize * cam.aspect + 2f;
        float minX = -maxX;

        Vector3 pos = transform.position;
        if (pos.y < minY || pos.y > maxY || pos.x < minX || pos.x > maxX)
        {
            Debug.Log($"[BallController] Ball out of bounds at {pos}, triggering BallLost");
            GameEvents.TriggerBallLost();
        }
    }

    void Launch()
    {
        if (isLaunched) return;

        isLaunched = true;

        // 45°〜135°の範囲でランダムに発射
        float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        rb.linearVelocity = direction * speed;

        // 包丁を発射方向に向ける
        UpdateKnifeRotation(direction);

        if (GameState.Instance != null)
        {
            GameState.Instance.SetState(GameStateType.Playing);
        }

        GameEvents.TriggerBallLaunched();
        Debug.Log("Ball launched!");
    }

    void FixedUpdate()
    {
        // パドル貫通防止チェック
        if (isLaunched && paddle != null)
        {
            Collider2D paddleCol = paddle.GetComponent<Collider2D>();
            if (paddleCol != null)
            {
                Bounds paddleBounds = paddleCol.bounds;
                float paddleTop = paddleBounds.max.y;
                float paddleBottom = paddleBounds.min.y;
                float paddleLeft = paddleBounds.min.x;
                float paddleRight = paddleBounds.max.x;

                // ボールがパドルの高さ付近にいて、下向きに移動している場合
                float ballY = transform.position.y;
                float ballX = transform.position.x;
                float ballRadius = 0.15f; // ボールの半径（推定値）

                // パドルのX範囲内にいるかチェック
                if (ballX >= paddleLeft - ballRadius && ballX <= paddleRight + ballRadius)
                {
                    // ボールがパドルを通過した（貫通）
                    if (ballY < paddleTop && ballY > paddleBottom - 0.5f && rb.linearVelocity.y < 0)
                    {
                        // 貫通した！強制的に上に跳ね返す
                        Debug.LogWarning($"[BallController] Penetration detected! Ball Y={ballY:F2}, PaddleTop={paddleTop:F2}");
                        transform.position = new Vector3(ballX, paddleTop + ballRadius + 0.05f, transform.position.z);

                        // パドルの当たり位置に応じた反射角度
                        float hitPoint = (ballX - paddle.position.x) / (paddleBounds.extents.x);
                        hitPoint = Mathf.Clamp(hitPoint, -1f, 1f);
                        float angle = Mathf.Lerp(150f, 30f, (hitPoint + 1f) / 2f) * Mathf.Deg2Rad;
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        rb.linearVelocity = direction * speed;
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched) return;

        // パドルに当たった場合、当たった位置で角度を変える
        if (collision.gameObject.CompareTag("Paddle") || collision.gameObject.name == "Paddle")
        {
            float hitPoint = (transform.position.x - collision.transform.position.x)
                           / (collision.collider.bounds.size.x / 2f);
            hitPoint = Mathf.Clamp(hitPoint, -1f, 1f);

            // 左端: 150°、中央: 90°、右端: 30°
            float angle = Mathf.Lerp(150f, 30f, (hitPoint + 1f) / 2f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // 常に上向きを保証
            if (direction.y < 0.3f)
            {
                direction.y = 0.3f;
                direction = direction.normalized;
            }

            rb.linearVelocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "BottomWall" || other.CompareTag("DeathZone"))
        {
            GameEvents.TriggerBallLost();
            // ResetBallはイベント経由で呼ばれる
        }
    }

    public void ResetBall()
    {
        isLaunched = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (paddle != null)
        {
            transform.position = paddle.position + offsetFromPaddle;
        }

        // 包丁を上向きにリセット
        UpdateKnifeRotation(Vector2.up);
    }
}
