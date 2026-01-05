using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// フィーバーシステム管理
/// - 20秒ごとにターゲット魚🐟が出現
/// - 包丁が当たるとフィーバータイム発動
/// - フィーバー中：魚群が泳ぎ、スピード2倍、パドル2倍、魚に当たると切り身カウント
/// </summary>
public class FeverManager : MonoBehaviour
{
    [Header("Target Fish Settings")]
    public float targetSpawnInterval = 20f;  // ターゲット魚の出現間隔
    public float targetFishSpeed = 8f;       // ターゲット魚の移動速度
    public float targetFishSize = 1.5f;      // ターゲット魚のサイズ

    [Header("Fever Settings")]
    public float feverDuration = 10f;        // フィーバータイム持続時間
    public float feverSpeedMultiplier = 2f;  // ボール速度倍率
    public float feverPaddleMultiplier = 2f; // パドル長さ倍率

    [Header("Fish School Settings")]
    public int fishPerWave = 10;             // 1波あたりの魚数
    public float fishSpawnInterval = 0.3f;   // 魚の出現間隔
    public float schoolFishSpeed = 6f;       // 魚群の泳ぐ速度

    [Header("References")]
    public BallController ballController;
    public PaddleController paddleController;

    private bool isFeverActive = false;
    private float feverTimer = 0f;
    private float targetSpawnTimer = 0f;
    private GameObject currentTargetFish;
    private GameObject feverUIText;
    private Vector3 originalPaddleScale;
    private float originalBallSpeed;

    // 魚群管理
    private GameObject fishSchoolParent;
    private Coroutine fishSpawnCoroutine;

    void Start()
    {
        // 参照を自動取得
        if (ballController == null)
            ballController = FindFirstObjectByType<BallController>();
        if (paddleController == null)
            paddleController = FindFirstObjectByType<PaddleController>();

        if (paddleController != null)
            originalPaddleScale = paddleController.transform.localScale;

        targetSpawnTimer = targetSpawnInterval;

        CreateFeverUI();
    }

    void Update()
    {
        if (GameState.Instance == null || GameState.Instance.CurrentState != GameStateType.Playing)
            return;

        // フィーバー中
        if (isFeverActive)
        {
            feverTimer -= Time.deltaTime;
            UpdateFeverUI();

            if (feverTimer <= 0)
            {
                EndFever();
            }
        }
        else
        {
            // ターゲット魚の出現タイマー
            targetSpawnTimer -= Time.deltaTime;
            if (targetSpawnTimer <= 0)
            {
                SpawnTargetFish();
                targetSpawnTimer = targetSpawnInterval;
            }
        }
    }

    void CreateFeverUI()
    {
        // フィーバー表示用UIを作成
        feverUIText = new GameObject("FeverText");
        feverUIText.transform.SetParent(transform);
        feverUIText.transform.position = new Vector3(0, 8, 0);

        TextMeshPro tmp = feverUIText.AddComponent<TextMeshPro>();
        tmp.text = "";
        tmp.fontSize = 8;
        tmp.color = Color.yellow;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/Meiryo-Japanese SDF");
        if (font != null) tmp.font = font;

        feverUIText.SetActive(false);
    }

    void UpdateFeverUI()
    {
        if (feverUIText != null && isFeverActive)
        {
            TextMeshPro tmp = feverUIText.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = $"🐟 FEVER! {feverTimer:F1}s 🐟";
                // 点滅効果
                tmp.color = Color.Lerp(Color.yellow, Color.red, Mathf.PingPong(Time.time * 3, 1));
            }
        }
    }

    /// <summary>
    /// ターゲット魚を上下左右からランダムに出現させる
    /// </summary>
    void SpawnTargetFish()
    {
        if (currentTargetFish != null) return;

        currentTargetFish = new GameObject("TargetFish");

        // 出現方向をランダムに決定 (0:左, 1:右, 2:上, 3:下)
        int direction = Random.Range(0, 4);
        Vector3 startPos = Vector3.zero;
        Vector3 targetPos = Vector3.zero;
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float screenHeight = Camera.main.orthographicSize;

        switch (direction)
        {
            case 0: // 左から右へ
                startPos = new Vector3(-screenWidth - 2, Random.Range(-screenHeight * 0.5f, screenHeight * 0.5f), 0);
                targetPos = new Vector3(screenWidth + 2, startPos.y, 0);
                break;
            case 1: // 右から左へ
                startPos = new Vector3(screenWidth + 2, Random.Range(-screenHeight * 0.5f, screenHeight * 0.5f), 0);
                targetPos = new Vector3(-screenWidth - 2, startPos.y, 0);
                break;
            case 2: // 上から下へ
                startPos = new Vector3(Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f), screenHeight + 2, 0);
                targetPos = new Vector3(startPos.x, -screenHeight - 2, 0);
                break;
            case 3: // 下から上へ
                startPos = new Vector3(Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f), -screenHeight - 2, 0);
                targetPos = new Vector3(startPos.x, screenHeight + 2, 0);
                break;
        }

        currentTargetFish.transform.position = startPos;
        currentTargetFish.transform.localScale = Vector3.one * targetFishSize;

        // 魚の見た目（スプライト表示）
        SpriteRenderer sr = currentTargetFish.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 50;
        Sprite fishSprite = Resources.Load<Sprite>("Sprites/sakana_normal");
        if (fishSprite != null)
        {
            sr.sprite = fishSprite;
            float scale = targetFishSize / (fishSprite.texture.width / fishSprite.pixelsPerUnit);
            currentTargetFish.transform.localScale = Vector3.one * scale;
        }

        // 進行方向に応じて反転
        if (direction == 1) // 右から左
        {
            Vector3 s = currentTargetFish.transform.localScale;
            currentTargetFish.transform.localScale = new Vector3(-s.x, s.y, s.z);
        }

        // コライダー追加
        CircleCollider2D col = currentTargetFish.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;

        // TargetFishコンポーネント追加
        TargetFish tf = currentTargetFish.AddComponent<TargetFish>();
        tf.targetPosition = targetPos;
        tf.speed = targetFishSpeed;
        tf.feverManager = this;

        Debug.Log($"[FeverManager] TargetFish spawned from direction {direction}");
    }

    /// <summary>
    /// フィーバータイム開始
    /// </summary>
    public void StartFever()
    {
        if (isFeverActive) return;

        isFeverActive = true;
        feverTimer = feverDuration;

        Debug.Log("[FeverManager] FEVER TIME START!");

        // ターゲット魚を削除
        if (currentTargetFish != null)
        {
            Destroy(currentTargetFish);
            currentTargetFish = null;
        }

        // UI表示
        if (feverUIText != null)
            feverUIText.SetActive(true);

        // ボール速度2倍
        if (ballController != null)
        {
            originalBallSpeed = ballController.speed;
            ballController.speed *= feverSpeedMultiplier;
        }

        // パドル長さ2倍
        if (paddleController != null)
        {
            originalPaddleScale = paddleController.transform.localScale;
            paddleController.transform.localScale = new Vector3(
                originalPaddleScale.x * feverPaddleMultiplier,
                originalPaddleScale.y,
                originalPaddleScale.z
            );
        }

        // 魚群スポーン開始
        fishSpawnCoroutine = StartCoroutine(SpawnFishSchool());
    }

    /// <summary>
    /// フィーバータイム終了
    /// </summary>
    void EndFever()
    {
        isFeverActive = false;

        Debug.Log("[FeverManager] FEVER TIME END!");

        // UI非表示
        if (feverUIText != null)
            feverUIText.SetActive(false);

        // ボール速度を元に戻す
        if (ballController != null)
        {
            ballController.speed = originalBallSpeed;
        }

        // パドル長さを元に戻す
        if (paddleController != null)
        {
            paddleController.transform.localScale = originalPaddleScale;
        }

        // 魚群スポーン停止
        if (fishSpawnCoroutine != null)
        {
            StopCoroutine(fishSpawnCoroutine);
            fishSpawnCoroutine = null;
        }

        // 残っている魚群を削除
        if (fishSchoolParent != null)
        {
            Destroy(fishSchoolParent);
        }

        // タイマーリセット
        targetSpawnTimer = targetSpawnInterval;
    }

    /// <summary>
    /// フィーバー中に魚群を左から右へスポーン
    /// </summary>
    IEnumerator SpawnFishSchool()
    {
        if (fishSchoolParent != null)
            Destroy(fishSchoolParent);

        fishSchoolParent = new GameObject("FishSchool");

        float screenHeight = Camera.main.orthographicSize;
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;

        while (isFeverActive)
        {
            // ランダムなY位置から魚を出現
            float y = Random.Range(-screenHeight * 0.8f, screenHeight * 0.8f);
            Vector3 startPos = new Vector3(-screenWidth - 2, y, 0);

            GameObject fish = new GameObject("SchoolFish");
            fish.transform.position = startPos;
            fish.transform.SetParent(fishSchoolParent.transform);

            SpriteRenderer fishSr = fish.AddComponent<SpriteRenderer>();
            fishSr.sortingOrder = 40;
            Sprite fishSprite = Resources.Load<Sprite>("Sprites/sakana_normal");
            if (fishSprite != null)
            {
                fishSr.sprite = fishSprite;
                float scale = 0.8f / (fishSprite.texture.width / fishSprite.pixelsPerUnit);
                fish.transform.localScale = Vector3.one * scale;
            }

            CircleCollider2D col = fish.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            SchoolFish sf = fish.AddComponent<SchoolFish>();
            sf.speed = schoolFishSpeed + Random.Range(-1f, 1f);
            sf.targetX = screenWidth + 3;

            yield return new WaitForSeconds(fishSpawnInterval);
        }
    }

    public bool IsFeverActive => isFeverActive;
}

/// <summary>
/// ターゲット魚（当たるとフィーバー発動）
/// 虹色に光る
/// </summary>
public class TargetFish : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed;
    public FeverManager feverManager;

    private SpriteRenderer spriteRenderer;
    private float hue = 0f;
    public float rainbowSpeed = 2f;  // 虹色変化速度

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 目標位置に向かって移動
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 虹色に光らせる（HSVでHueを循環）
        if (spriteRenderer != null)
        {
            hue += Time.deltaTime * rainbowSpeed;
            if (hue > 1f) hue -= 1f;
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f);
        }

        // 画面外に出たら削除
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ボール（包丁）が当たった
        if (other.GetComponent<BallController>() != null)
        {
            Debug.Log("[TargetFish] HIT! Starting Fever!");
            if (feverManager != null)
            {
                feverManager.StartFever();
            }
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// フィーバー中の魚群の魚（当たると切り身カウント）
/// </summary>
public class SchoolFish : MonoBehaviour
{
    public float speed;
    public float targetX;

    void Update()
    {
        // 右に泳ぐ
        transform.position += Vector3.right * speed * Time.deltaTime;

        // 画面外に出たら削除
        if (transform.position.x > targetX)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ボール（包丁）が当たった
        if (other.GetComponent<BallController>() != null)
        {
            Debug.Log("[SchoolFish] Sliced!");

            // 切り身カウント
            if (GameState.Instance != null)
            {
                GameState.Instance.AddKirimi(1);
            }

            // エフェクト（簡易）
            StartCoroutine(DestroyEffect());
        }
    }

    IEnumerator DestroyEffect()
    {
        // コライダー無効化（二重判定防止）
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null) col.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // スケールアップしてフェードアウト
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = startScale * (1f + t * 0.5f);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
