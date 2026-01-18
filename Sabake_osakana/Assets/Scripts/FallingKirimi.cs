using UnityEngine;

/// <summary>
/// 落下きりみの挙動
/// ブロック破壊時にスポーンし、パドルで受け取るとスコア加算
/// 見た目は丸い切り身（サーモンピンク）
/// </summary>
public class FallingKirimi : MonoBehaviour
{
    [Header("Settings")]
    public bool isGaming = false;  // ゲーミングきりみ（虹色、10ポイント）
    public int normalPoints = 1;
    public int gamingPoints = 10;

    [Header("Visual")]
    public float size = 0.35f;  // きりみのサイズ
    public Color normalColor = new Color(0.98f, 0.5f, 0.45f);  // サーモンピンク

    [Header("Physics")]
    public float destroyY = -12f;  // この高さ以下で消滅

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer glowRenderer;  // ゲーミング用のオーラ
    private Rigidbody2D rb;
    private float hue = 0f;  // ゲーミング用色相
    private bool isCollected = false;
    private Vector2 pendingVelocity;  // Start前に設定された初速
    private float fieldLeft;
    private float fieldRight;

    void Start()
    {
        // Start()で初期化（isGamingが設定された後に実行される）
        CalculateFieldBounds();
        SetupVisual();
        SetupPhysics();
        SetupCollider();

        // 保存されていた初速を適用
        if (pendingVelocity.sqrMagnitude > 0 && rb != null)
        {
            rb.linearVelocity = pendingVelocity;
        }

        if (isGaming)
        {
            Debug.Log($"[FallingKirimi] Gaming kirimi initialized: gravityScale={rb.gravityScale}");
        }
    }

    void CalculateFieldBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // WallSetupのマージンと同じ値
        float margin = 6.7f;
        fieldLeft = -camWidth / 2f + margin + 0.2f;
        fieldRight = camWidth / 2f - margin - 0.2f;
    }

    void SetupVisual()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 15;

        // 円形スプライト作成（サーモンピンクの切り身）
        Texture2D tex = new Texture2D(64, 64);
        Vector2 center = new Vector2(32, 32);
        float radius = 30f;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    // 中心に近いほど明るく
                    float brightness = 1f - (dist / radius) * 0.3f;
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                }
                else if (dist < radius + 1.5f)
                {
                    // アンチエイリアス
                    float alpha = 1f - (dist - radius) / 1.5f;
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f / size);
        spriteRenderer.color = isGaming ? Color.white : normalColor;

        // ゲーミングきりみは虹色オーラを追加
        if (isGaming)
        {
            hue = Random.value;
            CreateRainbowGlow();
        }
    }

    void CreateRainbowGlow()
    {
        GameObject glowObj = new GameObject("RainbowGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localScale = Vector3.one * 1.3f;  // 少し大きく

        glowRenderer = glowObj.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = spriteRenderer.sprite;  // 同じ円形スプライト
        glowRenderer.sortingOrder = 14;  // 本体の後ろ
        glowRenderer.color = Color.HSVToRGB(hue, 0.8f, 1f);
    }

    void SetupPhysics()
    {
        rb = gameObject.AddComponent<Rigidbody2D>();
        // ゲーミングきりみは落下速度1/4（かなりゆっくり）
        rb.gravityScale = isGaming ? 0.3f : 1.2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void SetupCollider()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        // ゲーミングきりみは当たり判定2倍
        col.radius = isGaming ? size : size / 2f;
        col.isTrigger = true;
    }

    /// <summary>
    /// 初速を設定してスポーン
    /// </summary>
    public void Launch(Vector2 velocity)
    {
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            // Start()前に呼ばれた場合は保存しておく
            pendingVelocity = velocity;
        }
    }

    void Update()
    {
        // ゲーミングきりみの虹色アニメーション
        if (isGaming && glowRenderer != null)
        {
            hue += Time.deltaTime;  // 1秒で1周
            if (hue > 1f) hue -= 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f);
            glowRenderer.color = rainbowColor;

            // オーラのパルスアニメーション
            float pulse = 1.2f + Mathf.Sin(Time.time * 5f) * 0.15f;
            glowRenderer.transform.localScale = Vector3.one * pulse;
        }

        // フィールド内に制限（壁で跳ね返る）
        ConstrainToField();

        // 画面外で消滅
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void ConstrainToField()
    {
        if (rb == null) return;

        Vector3 pos = transform.position;
        Vector2 vel = rb.linearVelocity;
        bool bounced = false;

        // 左壁
        if (pos.x < fieldLeft)
        {
            pos.x = fieldLeft;
            vel.x = Mathf.Abs(vel.x) * 0.8f;  // 跳ね返り
            bounced = true;
        }
        // 右壁
        else if (pos.x > fieldRight)
        {
            pos.x = fieldRight;
            vel.x = -Mathf.Abs(vel.x) * 0.8f;  // 跳ね返り
            bounced = true;
        }

        if (bounced)
        {
            transform.position = pos;
            rb.linearVelocity = vel;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // パドルに当たったらスコア加算
        PaddleController paddle = other.GetComponent<PaddleController>();
        if (paddle != null)
        {
            CollectKirimi();
        }
    }

    void CollectKirimi()
    {
        isCollected = true;

        int points = isGaming ? gamingPoints : normalPoints;

        if (GameState.Instance != null)
        {
            GameState.Instance.AddKirimi(points);
        }

        if (isGaming)
        {
            Debug.Log($"[FallingKirimi] Gaming kirimi collected! +{points} points");
        }

        // 即座に消滅
        Destroy(gameObject);
    }
}
