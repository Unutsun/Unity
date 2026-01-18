using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 流れてくる魚の管理（ツムツム風）
/// ブロックが残り50%以下になったら発動
/// 時間経過でスポーン間隔が短くなる
/// </summary>
public class FlowingFishManager : MonoBehaviour
{
    public static FlowingFishManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 3f;     // 初期スポーン間隔
    public float minSpawnInterval = 0.8f;       // 最小スポーン間隔
    public float intervalDecreaseRate = 0.1f;   // スポーン間隔の減少速度（秒/秒）
    public float fishMoveSpeed = 2f;            // 魚の移動速度

    [Header("Activation")]
    public float activationThreshold = 0.5f;    // 残りブロック50%以下で発動

    [Header("Bonus Time（全ブロック破壊後）")]
    public float bonusSpawnInterval = 0.5f;     // ボーナスタイム中のスポーン間隔
    public float rainbowFishChance = 0.3f;      // 虹色魚の出現率（ボーナスタイム中）

    [Header("Fever Time（虹色魚破壊時）")]
    public float feverDuration = 5f;            // フィーバータイムの持続時間
    public int feverFishBurst = 10;             // フィーバー発動時に一気にスポーンする魚の数

    private bool isActive = false;
    private bool isBonusTime = false;
    private bool isFeverTime = false;
    private float feverTimer = 0f;
    private float currentSpawnInterval;
    private float spawnTimer = 0f;
    private float fieldLeft;
    private float fieldRight;
    private float spawnY;
    private float timeSinceActivation = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        GameEvents.OnAllBricksDestroyed += StartBonusTime;
    }

    void OnDisable()
    {
        GameEvents.OnAllBricksDestroyed -= StartBonusTime;
    }

    void StartBonusTime()
    {
        isBonusTime = true;
        isActive = true;  // ボーナス開始と同時にアクティブ化
        currentSpawnInterval = bonusSpawnInterval;
        spawnTimer = 0f;
        Debug.Log("[FlowingFishManager] BONUS TIME! Rainbow fish will appear!");
    }

    /// <summary>
    /// フィーバータイム発動（虹色魚破壊時に呼ばれる）
    /// </summary>
    public void TriggerFever()
    {
        if (isFeverTime) return;  // 既にフィーバー中なら無視

        isFeverTime = true;
        feverTimer = feverDuration;
        Debug.Log($"[FlowingFishManager] FEVER TIME! Spawning {feverFishBurst} fish!");

        // 一気に魚をスポーン
        for (int i = 0; i < feverFishBurst; i++)
        {
            SpawnFish();
        }
    }

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        CalculateFieldBounds();
    }

    void CalculateFieldBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // WallSetupのマージンを考慮
        float margin = 6.7f;  // WallSetup.leftMargin/rightMarginと同じ値
        fieldLeft = -camWidth / 2f + margin + 1f;  // 少し内側に
        fieldRight = camWidth / 2f - margin - 1f;

        // スポーン位置（画面上端より少し上）
        spawnY = cam.orthographicSize + 1f;
    }

    void Update()
    {
        if (GameState.Instance == null) return;
        // PlayingかBonus中のみ動作
        if (GameState.Instance.CurrentState != GameStateType.Playing &&
            GameState.Instance.CurrentState != GameStateType.Bonus) return;

        // まだアクティブでない場合、発動条件をチェック
        if (!isActive)
        {
            CheckActivation();
            return;
        }

        // フィーバータイマー処理
        if (isFeverTime)
        {
            feverTimer -= Time.deltaTime;
            if (feverTimer <= 0)
            {
                isFeverTime = false;
                Debug.Log("[FlowingFishManager] Fever time ended!");
            }
        }

        // スポーン処理
        timeSinceActivation += Time.deltaTime;
        UpdateSpawnInterval();
        SpawnFishIfNeeded();
    }

    void CheckActivation()
    {
        if (GameState.Instance.TotalBricks <= 0) return;

        float remainingRatio = 1f - (float)GameState.Instance.DestroyedBricks / GameState.Instance.TotalBricks;

        if (remainingRatio <= activationThreshold)
        {
            Activate();
        }
    }

    void Activate()
    {
        isActive = true;
        timeSinceActivation = 0f;
        currentSpawnInterval = initialSpawnInterval;
        spawnTimer = 0f;
        Debug.Log("[FlowingFishManager] Activated! Fish will start spawning.");
    }

    void UpdateSpawnInterval()
    {
        // 時間経過でスポーン間隔が短くなる
        currentSpawnInterval = Mathf.Max(
            minSpawnInterval,
            initialSpawnInterval - (timeSinceActivation * intervalDecreaseRate)
        );
    }

    void SpawnFishIfNeeded()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnFish();
        }
    }

    void SpawnFish()
    {
        // ランダムなX位置
        float x = Random.Range(fieldLeft, fieldRight);
        Vector3 spawnPos = new Vector3(x, spawnY, 0);

        // 魚を生成
        GameObject fishObj = new GameObject("FlowingFish");
        fishObj.transform.position = spawnPos;

        FlowingFish fish = fishObj.AddComponent<FlowingFish>();
        // スキル効果: SlowFish - 魚の速度を遅くする
        float speedMultiplier = SkillManager.Instance != null ? SkillManager.Instance.FishSpeedMultiplier : 1f;
        fish.moveSpeed = (fishMoveSpeed + Random.Range(-0.5f, 0.5f)) * speedMultiplier;

        // ボーナス中またはフィーバー中は虹色魚が混ざる
        if ((isBonusTime || isFeverTime) && Random.value < rainbowFishChance)
        {
            fish.fishType = -1;  // -1 = 虹色魚
            Debug.Log($"[FlowingFishManager] RAINBOW fish spawned at x={x:F1}!");
        }
        else
        {
            fish.fishType = Random.Range(0, 3);  // 🐟🐠🐡からランダム
        }

        Debug.Log($"[FlowingFishManager] Fish spawned at x={x:F1}, type={fish.fishType}, interval={currentSpawnInterval:F2}s");
    }

    /// <summary>
    /// 強制的にアクティブ化（デバッグ用）
    /// </summary>
    public void ForceActivate()
    {
        Activate();
    }

    /// <summary>
    /// 現在アクティブかどうか
    /// </summary>
    public bool IsActive => isActive;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
