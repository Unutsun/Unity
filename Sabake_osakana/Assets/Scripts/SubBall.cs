using UnityEngine;

/// <summary>
/// サブボール（色違い包丁）
/// - パドルで打ち返せる
/// - ブロックを破壊できる
/// - 画面外に出たら消滅（ライフに影響しない）
/// </summary>
public class SubBall : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 8f;
    public int colorIndex = 0;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color ballColor;
    private bool isLaunched = false;
    private GameObject knifeVisual;
    private TrailRenderer trail;

    /// <summary>
    /// 初期化（MultiBallManagerから呼ばれる）
    /// </summary>
    public void Initialize(Color color, float ballSpeed, int index)
    {
        ballColor = color;
        speed = ballSpeed;
        colorIndex = index;

        SetupComponents();
        SetupVisual();
        SetupCollider();

        // 下向きに発射
        LaunchDownward();
    }

    void SetupComponents()
    {
        // Rigidbody2D
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Physics Material
        PhysicsMaterial2D mat = new PhysicsMaterial2D("SubBallMat");
        mat.bounciness = 1f;
        mat.friction = 0f;

        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.sharedMaterial = mat;
    }

    void SetupVisual()
    {
        // 包丁スプライトを読み込み
        Sprite knifeSprite = Resources.Load<Sprite>("Sprites/knife");

        // 包丁ビジュアル（子オブジェクト）
        knifeVisual = new GameObject("KnifeVisual");
        knifeVisual.transform.SetParent(transform);
        knifeVisual.transform.localPosition = Vector3.zero;

        spriteRenderer = knifeVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 15;

        if (knifeSprite != null)
        {
            spriteRenderer.sprite = knifeSprite;
            float targetSize = 0.8f;
            float scale = targetSize / (knifeSprite.texture.width / knifeSprite.pixelsPerUnit);
            knifeVisual.transform.localScale = Vector3.one * scale;
        }
        else
        {
            // フォールバック：色付き丸
            Texture2D tex = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    tex.SetPixel(x, y, dist <= 14 ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            knifeVisual.transform.localScale = Vector3.one * 0.5f;
        }

        // 色を適用
        spriteRenderer.color = ballColor;

        // トレイルエフェクト
        SetupTrail();
    }

    void SetupTrail()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.15f;
        trail.startWidth = 0.3f;
        trail.endWidth = 0.05f;
        trail.material = new Material(Shader.Find("Sprites/Default"));

        // グラデーション（色に合わせる）
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(ballColor, 0f),
                new GradientColorKey(ballColor * 0.5f, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;
        trail.sortingOrder = 14;
    }

    void SetupCollider()
    {
        // すでにSetupComponentsで設定済み
    }

    void LaunchDownward()
    {
        // 下向きに発射
        Vector2 direction = new Vector2(Random.Range(-0.3f, 0.3f), -1f).normalized;
        rb.linearVelocity = direction * speed;
        isLaunched = true;
        UpdateKnifeRotation();
    }

    void Update()
    {
        if (!isLaunched) return;

        UpdateKnifeRotation();
        CheckOutOfBounds();
    }

    // 包丁画像の初期向きオフセット（メインボールと同じ値）
    private const float KNIFE_ROTATION_OFFSET = -50f;

    void UpdateKnifeRotation()
    {
        if (knifeVisual == null || rb == null) return;

        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            // メインボールと同じ回転式を使用
            knifeVisual.transform.rotation = Quaternion.Euler(0, 0, angle - KNIFE_ROTATION_OFFSET);
        }
    }

    void CheckOutOfBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float margin = 2f;
        float maxY = cam.orthographicSize + margin;
        float minY = -cam.orthographicSize - margin;
        float maxX = cam.orthographicSize * cam.aspect + margin;

        Vector3 pos = transform.position;
        if (pos.y < minY || pos.y > maxY || Mathf.Abs(pos.x) > maxX)
        {
            // 画面外に出た→消滅（ライフに影響なし）
            DestroySelf();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // パドルとの衝突
        if (collision.gameObject.name == "Paddle" ||
            collision.gameObject.GetComponent<PaddleController>() != null)
        {
            // 打ち返し - 速度を維持しつつ反射
            Vector2 newVel = rb.linearVelocity;
            if (newVel.y < 0)
            {
                newVel.y = Mathf.Abs(newVel.y);  // 上向きに
            }
            rb.linearVelocity = newVel.normalized * speed;
            Debug.Log("[SubBall] Bounced off paddle");
        }
    }

    void DestroySelf()
    {
        Debug.Log($"[SubBall] Destroyed (colorIndex={colorIndex})");

        // MultiBallManagerに通知
        if (MultiBallManager.Instance != null)
        {
            MultiBallManager.Instance.OnSubBallDestroyed(gameObject);
        }

        Destroy(gameObject);
    }
}
