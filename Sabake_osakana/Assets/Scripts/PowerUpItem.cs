using UnityEngine;
using System.Collections;

/// <summary>
/// お助けアイテム - 魚絵文字（fish1/2/3.png）
/// 包丁が当たると落下きりみをスポーン
/// </summary>
public class PowerUpItem : MonoBehaviour
{
    [Header("Settings")]
    public int kirimiBonus = 2;  // 切り身ボーナス
    public float size = 1.2f;    // 魚のサイズ

    // グリッド上の位置（BrickManagerから設定される）
    [HideInInspector] public int gridRow;
    [HideInInspector] public int gridCol;
    [HideInInspector] public BrickManager brickManager;

    private bool isUsed = false;
    private SpriteRenderer spriteRenderer;
    private int fishType = 0;

    // 魚スプライト
    private static Sprite[] fishSprites;

    void Start()
    {
        CreateFishVisual();
        SetupCollider();
    }

    void CreateFishVisual()
    {
        // 魚スプライトを読み込み（初回のみ）
        if (fishSprites == null)
        {
            fishSprites = new Sprite[3];
            fishSprites[0] = Resources.Load<Sprite>("Sprites/fish1");
            fishSprites[1] = Resources.Load<Sprite>("Sprites/fish2");
            fishSprites[2] = Resources.Load<Sprite>("Sprites/fish3");
        }

        // ランダムに魚を選択
        fishType = Random.Range(0, 3);
        Sprite selectedFish = fishSprites[fishType];

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 5;

        if (selectedFish != null)
        {
            spriteRenderer.sprite = selectedFish;
            // サイズ調整
            float scale = size / (selectedFish.texture.width / selectedFish.pixelsPerUnit);
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            // フォールバック：青い丸
            spriteRenderer.sprite = CreateCircleSprite();
            spriteRenderer.color = new Color(0.3f, 0.6f, 1f);
        }
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        float radius = size / 2f - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void SetupCollider()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;  // 適切なサイズに縮小
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isUsed) return;

        // ボール（メインまたはサブ）のみ反応
        if (BallHelper.IsBall(other))
        {
            Debug.Log($"[PowerUpItem] Hit by ball: {other.gameObject.name}");
            ActivatePowerUp();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isUsed) return;

        // ボール（メインまたはサブ）のみ反応
        if (BallHelper.IsBall(collision))
        {
            Debug.Log($"[PowerUpItem] Collision with ball: {collision.gameObject.name}");
            ActivatePowerUp();
        }
    }

    void ActivatePowerUp()
    {
        isUsed = true;
        Debug.Log($"[PowerUpItem] Fish caught! Spawning falling kirimi");

        // 落下きりみをスポーン（スコアは落下きりみ取得時に加算）
        if (KirimiSpawner.Instance != null)
        {
            KirimiSpawner.Instance.SpawnKirimi(transform.position);
        }

        // エフェクト
        StartCoroutine(DestroyEffect());
    }

    IEnumerator DestroyEffect()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = startScale * (1f + t * 0.5f);

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
