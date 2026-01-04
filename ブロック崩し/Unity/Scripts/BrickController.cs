using UnityEngine;

/// <summary>
/// 個々のブロックの挙動を管理するコンポーネント
/// </summary>
public class BrickController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int hitPoints = 1;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private bool isDestructible = true;

    [Header("Visual")]
    [SerializeField] private Color brickColor = Color.white;
    [SerializeField] private ParticleSystem destroyEffect;

    private SpriteRenderer spriteRenderer;
    private int currentHitPoints;

    // Events
    public System.Action<int> OnDestroyed; // scoreValue を渡す
    public System.Action OnHit;

    public int ScoreValue => scoreValue;
    public bool IsActive => currentHitPoints > 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHitPoints = hitPoints;
        UpdateVisual();
    }

    /// <summary>
    /// ブロックにダメージを与える
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        if (!isDestructible) return;
        if (currentHitPoints <= 0) return;

        currentHitPoints -= damage;
        OnHit?.Invoke();

        if (currentHitPoints <= 0)
        {
            DestroyBrick();
        }
        else
        {
            UpdateVisual();
        }
    }

    /// <summary>
    /// ブロックを破壊する
    /// </summary>
    private void DestroyBrick()
    {
        OnDestroyed?.Invoke(scoreValue);

        // パーティクルエフェクト再生
        if (destroyEffect != null)
        {
            ParticleSystem effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            var main = effect.main;
            main.startColor = brickColor;
            Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// ビジュアルを更新（耐久度に応じて色を変更）
    /// </summary>
    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (hitPoints > 1)
        {
            // 耐久度に応じて暗くする
            float ratio = (float)currentHitPoints / hitPoints;
            Color adjustedColor = Color.Lerp(Color.gray, brickColor, ratio);
            spriteRenderer.color = adjustedColor;
        }
        else
        {
            spriteRenderer.color = brickColor;
        }
    }

    /// <summary>
    /// ブロックを初期化
    /// </summary>
    public void Initialize(Color color, int hp = 1, int score = 10)
    {
        brickColor = color;
        hitPoints = hp;
        scoreValue = score;
        currentHitPoints = hitPoints;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateVisual();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ブロックをリセット
    /// </summary>
    public void ResetBrick()
    {
        currentHitPoints = hitPoints;
        UpdateVisual();
        gameObject.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            TakeDamage();
        }
    }

    private void OnValidate()
    {
        // エディタで色が変更されたときにプレビュー
        if (spriteRenderer != null)
        {
            spriteRenderer.color = brickColor;
        }
    }
}
