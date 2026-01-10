using UnityEngine;
using System.Collections;

/// <summary>
/// お助けアイテム - 🐟絵文字
/// 包丁が当たると切り身+2
/// </summary>
public class PowerUpItem : MonoBehaviour
{
    [Header("Settings")]
    public int kirimiBonus = 2;  // 切り身ボーナス

    // グリッド上の位置（BrickManagerから設定される）
    [HideInInspector] public int gridRow;
    [HideInInspector] public int gridCol;
    [HideInInspector] public BrickManager brickManager;

    private bool isUsed = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        CreateFishVisual();
        SetupCollider();
    }

    void CreateFishVisual()
    {
        // 魚のスプライトを表示
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 5;

        // きりみ画像をロード
        Sprite fishSprite = Resources.Load<Sprite>("Sprites/kirimi");
        if (fishSprite != null)
        {
            spriteRenderer.sprite = fishSprite;
            // サイズ調整（2倍に）
            float scale = 1.6f / (fishSprite.texture.width / fishSprite.pixelsPerUnit);
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
        col.radius = 1.5f;  // 当たり判定を広げる
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isUsed) return;

        if (other.GetComponent<BallController>() != null)
        {
            ActivatePowerUp();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isUsed) return;

        if (collision.gameObject.GetComponent<BallController>() != null)
        {
            ActivatePowerUp();
        }
    }

    void ActivatePowerUp()
    {
        isUsed = true;
        Debug.Log($"[PowerUpItem] Fish caught! +{kirimiBonus} kirimi");

        // 切り身+2
        if (GameState.Instance != null)
        {
            GameState.Instance.AddKirimi(kirimiBonus);
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
