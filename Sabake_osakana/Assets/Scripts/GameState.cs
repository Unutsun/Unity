using UnityEngine;

/// <summary>
/// ゲームの状態を管理するenum
/// </summary>
public enum GameStateType
{
    Title,      // タイトル画面
    Countdown,  // カウントダウン中（3,2,1,さばけ！）
    Ready,      // ゲーム開始待ち（ボール発射前）
    Playing,    // プレイ中
    Paused,     // 一時停止
    GameOver,   // ゲームオーバー
    GameClear   // クリア
}

/// <summary>
/// ゲーム状態を保持するシングルトン
/// 「さばけ！おさかな」対応版
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("Current State")]
    [SerializeField] private GameStateType currentState = GameStateType.Ready;

    [Header("Game Data")]
    [SerializeField] private int kirimi = 0;           // 旧score → きりみ
    [SerializeField] private int lives = 5;            // ライフ5に変更
    [SerializeField] private float remainingTime = 90f; // 残り時間
    [SerializeField] private int totalBricks = 0;
    [SerializeField] private int destroyedBricks = 0;

    [Header("Settings")]
    public int initialLives = 5;
    public float timeLimit = 90f;  // 制限時間（秒）

    public GameStateType CurrentState => currentState;
    public int Kirimi => kirimi;
    public int Lives => lives;
    public float RemainingTime => remainingTime;
    public int TotalBricks => totalBricks;
    public int DestroyedBricks => destroyedBricks;

    void Awake()
    {
        Debug.Log($"[GameState] Awake: Initializing singleton");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log($"[GameState] Awake: Singleton set");
        }
        else
        {
            Debug.LogWarning($"[GameState] Awake: Duplicate instance, destroying");
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        Debug.Log($"[GameState] OnEnable: Registering event listeners");
        GameEvents.OnBrickDestroyed += HandleBrickDestroyed;
        GameEvents.OnBallLost += HandleBallLost;
        GameEvents.OnGameRestart += ResetGame;
        GameEvents.OnTimeUp += HandleTimeUp;
    }

    void OnDisable()
    {
        Debug.Log($"[GameState] OnDisable: Unregistering event listeners");
        GameEvents.OnBrickDestroyed -= HandleBrickDestroyed;
        GameEvents.OnBallLost -= HandleBallLost;
        GameEvents.OnGameRestart -= ResetGame;
        GameEvents.OnTimeUp -= HandleTimeUp;
    }

    void Start()
    {
        Debug.Log($"[GameState] Start: Resetting game");
        ResetGame();
    }

    public void SetState(GameStateType newState)
    {
        Debug.Log($"[GameState] SetState: {currentState} -> {newState}");
        currentState = newState;

        switch (newState)
        {
            case GameStateType.Playing:
                Time.timeScale = 1f;
                break;
            case GameStateType.Countdown:
            case GameStateType.Ready:
                Time.timeScale = 1f;  // カウントダウン・Ready中もtimeScale=1
                break;
            case GameStateType.Paused:
            case GameStateType.GameOver:
            case GameStateType.GameClear:
                Time.timeScale = 0f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }

    public void SetTotalBricks(int count)
    {
        totalBricks = count;
        destroyedBricks = 0;
        Debug.Log($"[GameState] SetTotalBricks: {count}");
    }

    public void SetTimeLimit(int seconds)
    {
        remainingTime = seconds;
        Debug.Log($"[GameState] SetTimeLimit: {seconds} seconds");
    }

    /// <summary>
    /// 切り身を追加（フィーバー魚やパワーアップ用）
    /// </summary>
    public void AddKirimi(int amount)
    {
        kirimi += amount;
        Debug.Log($"[GameState] AddKirimi: +{amount}, total={kirimi}");
        GameEvents.TriggerKirimiChanged(kirimi);
    }

    /// <summary>
    /// 破壊ブロック数を追加
    /// </summary>
    public void AddDestroyedBricks(int count)
    {
        destroyedBricks += count;
        Debug.Log($"[GameState] AddDestroyedBricks: +{count}, total={destroyedBricks}/{totalBricks}");

        if (destroyedBricks >= totalBricks && totalBricks > 0)
        {
            Debug.Log($"[GameState] All bricks destroyed! Game Clear!");
            SetState(GameStateType.GameClear);
            GameEvents.TriggerGameClear();
        }
    }

    public void UpdateTime(float deltaTime)
    {
        if (currentState != GameStateType.Playing) return;

        remainingTime -= deltaTime;
        GameEvents.TriggerTimeChanged(remainingTime);

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            GameEvents.TriggerTimeUp();
        }
    }

    void HandleBrickDestroyed(int points)
    {
        kirimi += points;
        destroyedBricks++;
        Debug.Log($"[GameState] HandleBrickDestroyed: kirimi={kirimi}, destroyed={destroyedBricks}/{totalBricks}");
        GameEvents.TriggerKirimiChanged(kirimi);

        if (destroyedBricks >= totalBricks)
        {
            Debug.Log($"[GameState] HandleBrickDestroyed: All bricks destroyed! Game Clear!");
            SetState(GameStateType.GameClear);
            GameEvents.TriggerGameClear();
        }
    }

    void HandleBallLost()
    {
        lives--;
        Debug.Log($"[GameState] HandleBallLost: lives={lives}");
        GameEvents.TriggerLivesChanged(lives);

        if (lives <= 0)
        {
            Debug.Log($"[GameState] HandleBallLost: No lives left! Game Over!");
            SetState(GameStateType.GameOver);
            GameEvents.TriggerGameOver();
        }
        else
        {
            SetState(GameStateType.Ready);
            GameEvents.TriggerBallReset();
        }
    }

    void HandleTimeUp()
    {
        // タイムアップは腕前評価画面へ（ボタン押下待ち）
        Debug.Log($"[GameState] HandleTimeUp: Time's up! Show ranking panel and wait for button.");
        SetState(GameStateType.GameClear);
        Time.timeScale = 0f;  // ゲームを停止（ボールも止まる）
        // GameClearイベントはパネル表示用。自動遷移はしない。
        GameEvents.TriggerGameClear();
    }

    void ResetGame()
    {
        Debug.Log($"[GameState] ResetGame: Resetting all values");
        kirimi = 0;
        lives = initialLives;
        remainingTime = timeLimit;
        destroyedBricks = 0;
        SetState(GameStateType.Countdown);  // カウントダウンから開始
        GameEvents.TriggerKirimiChanged(kirimi);
        GameEvents.TriggerLivesChanged(lives);
        GameEvents.TriggerTimeChanged(remainingTime);
    }

    /// <summary>
    /// クリア率を取得（0-100%）
    /// </summary>
    public float GetClearPercentage()
    {
        if (totalBricks <= 0) return 0f;
        return (float)destroyedBricks / totalBricks * 100f;
    }

    /// <summary>
    /// ランクを計算（きりみ数に応じて）
    /// </summary>
    public string GetRank()
    {
        if (destroyedBricks >= totalBricks) return "おさしみ級";
        float ratio = (float)destroyedBricks / totalBricks;
        if (ratio >= 0.8f) return "たたき級";
        if (ratio >= 0.6f) return "にぎり級";
        if (ratio >= 0.4f) return "あら煮級";
        return "エサ級";
    }
}
