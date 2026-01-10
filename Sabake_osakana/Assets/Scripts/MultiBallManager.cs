using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// マルチボール管理モジュール（疎結合設計）
/// - ブロック破壊でゲージが溜まる（10で発動）
/// - サブボールを生成（色違い包丁）
/// - サブボールはライフに影響しない
/// </summary>
public class MultiBallManager : MonoBehaviour
{
    public static MultiBallManager Instance { get; private set; }

    [Header("Gauge Settings")]
    public int gaugeMax = 10;           // ゲージ最大値
    public int gaugePerBrick = 1;       // ブロック1個あたりのゲージ増加量

    [Header("Sub Ball Settings")]
    public float subBallSpeed = 8f;     // サブボールの速度
    public float spawnOffsetY = 2f;     // 生成位置のY オフセット

    [Header("Rainbow Colors")]
    public Color[] rainbowColors = new Color[]
    {
        new Color(0.9f, 0.2f, 0.2f),    // 赤
        new Color(0.2f, 0.5f, 0.9f),    // 青
        new Color(0.9f, 0.8f, 0.2f),    // 黄色
        new Color(0.2f, 0.8f, 0.3f),    // 緑
        new Color(0.6f, 0.2f, 0.8f),    // 紫
        new Color(0.9f, 0.4f, 0.7f),    // ピンク
    };

    // 状態
    private int currentGauge = 0;
    private int nextColorIndex = 0;
    private List<GameObject> activeSubBalls = new List<GameObject>();
    private GameObject mainBall;

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

    void Start()
    {
        // メインボールを探す
        mainBall = GameObject.Find("Ball");
        if (mainBall == null)
        {
            Debug.LogWarning("[MultiBallManager] Main ball not found!");
        }
    }

    void OnEnable()
    {
        // ブロック破壊イベントを監視
        GameEvents.OnBrickDestroyed += OnBrickDestroyed;
        GameEvents.OnGameRestart += ResetState;
        GameEvents.OnReturnToTitle += ResetState;
    }

    void OnDisable()
    {
        GameEvents.OnBrickDestroyed -= OnBrickDestroyed;
        GameEvents.OnGameRestart -= ResetState;
        GameEvents.OnReturnToTitle -= ResetState;
    }

    /// <summary>
    /// ブロック破壊時のコールバック
    /// </summary>
    void OnBrickDestroyed(int kirimi)
    {
        // ゲージを増加
        currentGauge += gaugePerBrick;

        // ゲージ変更イベント発火
        GameEvents.TriggerMultiBallGaugeChanged(currentGauge);

        // ゲージがMAXに達したらサブボール生成
        if (currentGauge >= gaugeMax)
        {
            SpawnSubBall();
            currentGauge = 0;
            GameEvents.TriggerMultiBallGaugeChanged(currentGauge);
        }
    }

    /// <summary>
    /// サブボールを生成
    /// </summary>
    void SpawnSubBall()
    {
        if (mainBall == null) return;

        // 色を決定（虹色順）
        Color ballColor = rainbowColors[nextColorIndex % rainbowColors.Length];

        Debug.Log($"[MultiBallManager] Spawning sub ball with color index {nextColorIndex}");

        // サブボール生成位置（画面上部中央から落下）
        Camera cam = Camera.main;
        float spawnY = cam != null ? cam.orthographicSize - 1f : 4f;
        float spawnX = Random.Range(-2f, 2f);  // 少しランダムなX位置
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        // サブボールGameObject作成
        GameObject subBall = new GameObject($"SubBall_{nextColorIndex}");
        subBall.transform.position = spawnPos;
        // 注: タグは使用しない（ブロックはBallControllerまたはSubBallコンポーネントで判定）

        // SubBallコンポーネント追加
        SubBall subBallComp = subBall.AddComponent<SubBall>();
        subBallComp.Initialize(ballColor, subBallSpeed, nextColorIndex);

        activeSubBalls.Add(subBall);

        // イベント発火
        GameEvents.TriggerSubBallSpawned(nextColorIndex);

        // 次の色へ
        nextColorIndex++;
    }

    /// <summary>
    /// サブボールが消滅した時（SubBallから呼ばれる）
    /// </summary>
    public void OnSubBallDestroyed(GameObject subBall)
    {
        activeSubBalls.Remove(subBall);
        GameEvents.TriggerSubBallLost();
        Debug.Log($"[MultiBallManager] Sub ball destroyed. Remaining: {activeSubBalls.Count}");
    }

    /// <summary>
    /// 状態リセット
    /// </summary>
    void ResetState()
    {
        currentGauge = 0;
        nextColorIndex = 0;

        // 全サブボール削除
        foreach (var ball in activeSubBalls)
        {
            if (ball != null) Destroy(ball);
        }
        activeSubBalls.Clear();

        GameEvents.TriggerMultiBallGaugeChanged(0);
        Debug.Log("[MultiBallManager] State reset");
    }

    /// <summary>
    /// 現在のゲージ値を取得
    /// </summary>
    public int GetCurrentGauge() => currentGauge;

    /// <summary>
    /// アクティブなサブボール数を取得
    /// </summary>
    public int GetActiveSubBallCount() => activeSubBalls.Count;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
