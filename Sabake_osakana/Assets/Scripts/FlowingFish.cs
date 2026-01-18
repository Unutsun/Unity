using UnityEngine;

/// <summary>
/// 流れてくる魚 - ブロックの2倍サイズ
/// 上から下に向かって流れてきて、包丁で破壊 or パドルで受け取るとスコア加算
/// ツムツム風のシステム
/// </summary>
public class FlowingFish : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 2f;  // 下への移動速度
    public int kirimiValue = 1;   // 取得時のきりみ数
    public float size = 1.8f;     // ブロックの2倍サイズ
    public int fishType = 0;      // 魚の種類（0-2）

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer glowRenderer;  // 虹色オーラ用
    private bool isDestroyed = false;
    private float destroyY = -12f;
    private bool isRainbow = false;
    private float hue = 0f;
    private float pulseTimer = 0f;

    // 魚の色パターン
    private static readonly Color[] fishColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1f),    // 青 🐟
        new Color(1f, 0.6f, 0.2f),    // オレンジ 🐠
        new Color(0.8f, 0.7f, 0.5f),  // ベージュ 🐡
    };

    void Start()
    {
        SetupVisual();
        SetupCollider();
    }

    void SetupVisual()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 12;

        // 虹色魚の場合
        if (fishType < 0)
        {
            isRainbow = true;
            hue = Random.value;  // ランダムな色相からスタート
            kirimiValue = 5;  // 虹色魚は5倍のきりみ

            // ランダムな魚スプライトを使用（通常魚と同じPNG）
            int randomType = Random.Range(1, 4);
            string spriteName = $"Sprites/fish{randomType}";
            Sprite fishSprite = Resources.Load<Sprite>(spriteName);
            if (fishSprite != null)
            {
                spriteRenderer.sprite = fishSprite;
                // 虹色オーラを作成（魚の後ろに表示）
                CreateRainbowGlow(fishSprite);
            }
            else
            {
                CreateFallbackSprite();
            }
            Debug.Log($"[FlowingFish] Rainbow fish created with sprite: {spriteName}");
        }
        else
        {
            // Twemoji魚スプライトをロード（🐟🐠🐡）
            string spriteName = $"Sprites/fish{(fishType % 3) + 1}";
            Sprite fishSprite = Resources.Load<Sprite>(spriteName);

            if (fishSprite != null)
            {
                spriteRenderer.sprite = fishSprite;
                Debug.Log($"[FlowingFish] Loaded emoji sprite: {spriteName}");
            }
            else
            {
                Debug.LogWarning($"[FlowingFish] Failed to load {spriteName}, using fallback");
                CreateFallbackSprite();
            }
        }

        // サイズ設定
        transform.localScale = Vector3.one * size;
    }

    void CreateRainbowGlow(Sprite fishSprite)
    {
        // 子オブジェクトに虹色オーラを作成
        GameObject glowObj = new GameObject("RainbowGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localScale = Vector3.one * 1.3f;  // 少し大きめ

        glowRenderer = glowObj.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = fishSprite;  // 同じスプライトを使用
        glowRenderer.sortingOrder = 11;  // 魚の後ろに表示
        glowRenderer.color = new Color(1f, 1f, 1f, 0.6f);  // 半透明
    }

    void CreateFallbackSprite()
    {
        // フォールバック：シンプルな円
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
        spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        spriteRenderer.color = fishColors[fishType % fishColors.Length];
    }

    void SetupCollider()
    {
        // 物理判定用コライダー
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.5f, 1f);
        col.isTrigger = false;

        // Rigidbody2Dを追加（Kinematicで移動制御）
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (isDestroyed) return;

        // 虹色魚のアニメーション（オーラが虹色に光る + 脈動）
        if (isRainbow && glowRenderer != null)
        {
            hue += Time.deltaTime;  // 1秒で1周
            if (hue > 1f) hue -= 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f);
            rainbowColor.a = 0.7f;  // 半透明
            glowRenderer.color = rainbowColor;

            // 脈動エフェクト
            pulseTimer += Time.deltaTime * 3f;
            float pulse = 1.2f + Mathf.Sin(pulseTimer) * 0.15f;
            glowRenderer.transform.localScale = Vector3.one * pulse;
        }

        // 下に向かって移動
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        // 画面外で消滅
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;

        // ボール（包丁）で破壊
        if (BallHelper.IsBall(collision))
        {
            DestroyFish();
            return;
        }

        // パドルに当たっても破壊
        if (collision.gameObject.GetComponent<PaddleController>() != null)
        {
            DestroyFish();
        }
    }

    void DestroyFish()
    {
        isDestroyed = true;

        // 落下きりみをスポーン（虹色魚は5個、通常は3個）
        int kirimiCount = isRainbow ? 5 : 3;
        Debug.Log($"[FlowingFish] {(isRainbow ? "Rainbow " : "")}Fish destroyed! Spawning {kirimiCount} kirimi");

        if (KirimiSpawner.Instance != null)
        {
            KirimiSpawner.Instance.SpawnKirimi(transform.position, kirimiCount);
        }

        // 虹色魚を破壊したらフィーバータイム発動
        if (isRainbow && FlowingFishManager.Instance != null)
        {
            FlowingFishManager.Instance.TriggerFever();
        }

        // 即座に消滅
        Destroy(gameObject);
    }
}
