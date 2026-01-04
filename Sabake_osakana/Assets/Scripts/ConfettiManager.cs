using UnityEngine;
using System.Collections;

/// <summary>
/// 全ブロック破壊時の紙吹雪パーティクル演出
/// </summary>
public class ConfettiManager : MonoBehaviour
{
    public static ConfettiManager Instance { get; private set; }

    [Header("Confetti Settings")]
    public int confettiCount = 100;
    public float spawnDuration = 0.5f;
    public float confettiLifetime = 3f;
    public float fallSpeed = 2f;
    public float spreadWidth = 12f;

    [Header("Colors - Unicode Square Emojis")]
    // 🟥🟧🟨🟩🟦🟪 の色
    public Color[] confettiColors = new Color[]
    {
        new Color(0.92f, 0.28f, 0.28f),  // 🟥 赤
        new Color(0.96f, 0.60f, 0.18f),  // 🟧 オレンジ
        new Color(0.99f, 0.87f, 0.25f),  // 🟨 黄
        new Color(0.47f, 0.78f, 0.35f),  // 🟩 緑
        new Color(0.33f, 0.55f, 0.89f),  // 🟦 青
        new Color(0.66f, 0.40f, 0.80f),  // 🟪 紫
    };

    private GameObject confettiContainer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ConfettiManager] Awake: Singleton set");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        GameEvents.OnGameClear += OnGameClear;
    }

    void OnDisable()
    {
        GameEvents.OnGameClear -= OnGameClear;
    }

    void OnGameClear()
    {
        // 全ブロック破壊時のみ紙吹雪を表示
        if (GameState.Instance != null &&
            GameState.Instance.DestroyedBricks >= GameState.Instance.TotalBricks)
        {
            Debug.Log("[ConfettiManager] All blocks destroyed! Spawning confetti!");
            StartCoroutine(SpawnConfetti());
        }
    }

    IEnumerator SpawnConfetti()
    {
        // コンテナを作成
        if (confettiContainer != null)
        {
            Destroy(confettiContainer);
        }
        confettiContainer = new GameObject("ConfettiContainer");

        float interval = spawnDuration / confettiCount;
        Camera cam = Camera.main;
        float topY = cam != null ? cam.orthographicSize + 1f : 6f;

        for (int i = 0; i < confettiCount; i++)
        {
            CreateConfettiPiece(topY);

            if (i % 5 == 0)  // 5個ごとにフレームを分ける
            {
                yield return null;
            }
        }
    }

    void CreateConfettiPiece(float startY)
    {
        GameObject confetti = new GameObject("Confetti");
        confetti.transform.SetParent(confettiContainer.transform);

        // ランダムな位置
        float x = Random.Range(-spreadWidth / 2f, spreadWidth / 2f);
        confetti.transform.position = new Vector3(x, startY, 0);

        // ランダムな回転
        confetti.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // ランダムなサイズ
        float size = Random.Range(0.15f, 0.35f);
        confetti.transform.localScale = new Vector3(size, size * Random.Range(0.5f, 1.5f), 1);

        // SpriteRenderer
        SpriteRenderer sr = confetti.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = confettiColors[Random.Range(0, confettiColors.Length)];
        sr.sortingOrder = 100;

        // アニメーション用コンポーネント
        ConfettiPiece piece = confetti.AddComponent<ConfettiPiece>();
        piece.fallSpeed = fallSpeed * Random.Range(0.7f, 1.3f);
        piece.swayAmount = Random.Range(0.5f, 2f);
        piece.rotationSpeed = Random.Range(-180f, 180f);
        piece.lifetime = confettiLifetime + Random.Range(0f, 1f);
    }

    Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

/// <summary>
/// 個々の紙吹雪の動き
/// </summary>
public class ConfettiPiece : MonoBehaviour
{
    public float fallSpeed = 2f;
    public float swayAmount = 1f;
    public float rotationSpeed = 90f;
    public float lifetime = 3f;

    private float elapsed = 0f;
    private float swayOffset;

    void Start()
    {
        swayOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;

        // 落下
        float sway = Mathf.Sin(elapsed * 3f + swayOffset) * swayAmount;
        transform.position += new Vector3(sway * Time.unscaledDeltaTime, -fallSpeed * Time.unscaledDeltaTime, 0);

        // 回転
        transform.Rotate(0, 0, rotationSpeed * Time.unscaledDeltaTime);

        // 寿命
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
