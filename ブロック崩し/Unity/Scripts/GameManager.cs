using UnityEngine;

/// <summary>
/// ゲーム全体の状態管理を行うコンポーネント
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallController ball;
    [SerializeField] private PaddleController paddle;
    [SerializeField] private BrickManager brickManager;
    [SerializeField] private UIManager uiManager;

    [Header("Game Settings")]
    [SerializeField] private int initialLives = 3;
    [SerializeField] private int scorePerBrick = 10;

    // Game State
    private GameState currentState = GameState.Ready;
    private int score = 0;
    private int lives = 3;

    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;

    public enum GameState
    {
        Ready,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public static GameManager Instance { get; private set; }

    public GameState CurrentState => currentState;
    public int Score => score;
    public int Lives => lives;

    private void Awake()
    {
        // Singleton
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

    private void Start()
    {
        SetupEventListeners();
        InitializeGame();
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// イベントリスナーを設定
    /// </summary>
    private void SetupEventListeners()
    {
        if (ball != null)
        {
            ball.OnBallLost += HandleBallLost;
            ball.OnBrickHit += HandleBrickHit;
        }

        if (brickManager != null)
        {
            brickManager.OnBrickDestroyed += AddScore;
            brickManager.OnAllBricksDestroyed += HandleVictory;
        }
    }

    /// <summary>
    /// ゲームを初期化
    /// </summary>
    public void InitializeGame()
    {
        score = 0;
        lives = initialLives;

        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(lives);

        SetGameState(GameState.Ready);
        Debug.Log("Game initialized");
    }

    /// <summary>
    /// 入力を処理
    /// </summary>
    private void HandleInput()
    {
        // ゲーム開始
        if (currentState == GameState.Ready)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                StartGame();
            }
        }

        // ポーズ
        if (currentState == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                PauseGame();
            }
        }
        else if (currentState == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                ResumeGame();
            }
        }

        // リスタート
        if (currentState == GameState.GameOver || currentState == GameState.Victory)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
        }
    }

    /// <summary>
    /// ゲームを開始
    /// </summary>
    public void StartGame()
    {
        if (currentState != GameState.Ready) return;

        SetGameState(GameState.Playing);
        Debug.Log("Game started!");
    }

    /// <summary>
    /// ゲームをポーズ
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
        Debug.Log("Game paused");
    }

    /// <summary>
    /// ゲームを再開
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        Time.timeScale = 1f;
        SetGameState(GameState.Playing);
        Debug.Log("Game resumed");
    }

    /// <summary>
    /// ゲームをリスタート
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;

        // リセット
        score = 0;
        lives = initialLives;

        ball?.ResetBall();
        paddle?.ResetPosition();
        brickManager?.GenerateBricks();

        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(lives);

        SetGameState(GameState.Ready);
        Debug.Log("Game restarted");
    }

    /// <summary>
    /// ゲーム状態を設定
    /// </summary>
    private void SetGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// スコアを加算
    /// </summary>
    private void AddScore(int points)
    {
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// ボール落下時の処理
    /// </summary>
    private void HandleBallLost()
    {
        lives--;
        OnLivesChanged?.Invoke(lives);

        Debug.Log($"Ball lost! Lives remaining: {lives}");

        if (lives <= 0)
        {
            HandleGameOver();
        }
    }

    /// <summary>
    /// ブロック破壊時の処理
    /// </summary>
    private void HandleBrickHit()
    {
        // 追加の処理があればここに
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    private void HandleGameOver()
    {
        SetGameState(GameState.GameOver);
        Debug.Log($"Game Over! Final score: {score}");
    }

    /// <summary>
    /// 勝利処理
    /// </summary>
    private void HandleVictory()
    {
        SetGameState(GameState.Victory);
        Debug.Log($"Victory! Final score: {score}");
    }

    /// <summary>
    /// ライフを追加
    /// </summary>
    public void AddLife(int amount = 1)
    {
        lives += amount;
        OnLivesChanged?.Invoke(lives);
    }

    private void OnDestroy()
    {
        // イベントリスナーの解除
        if (ball != null)
        {
            ball.OnBallLost -= HandleBallLost;
            ball.OnBrickHit -= HandleBrickHit;
        }

        if (brickManager != null)
        {
            brickManager.OnBrickDestroyed -= AddScore;
            brickManager.OnAllBricksDestroyed -= HandleVictory;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
