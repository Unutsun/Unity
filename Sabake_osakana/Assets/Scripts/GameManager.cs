using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// ゲーム全体の管理（タイマー含む）
/// ResultScene遷移対応版
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    public bool useTimer = true;

    void Awake()
    {
        Debug.Log("[GameManager] Awake: Initializing singleton");
        if (Instance == null)
        {
            Instance = this;

            // GameDataが存在しなければ作成
            if (GameData.Instance == null)
            {
                GameObject obj = new GameObject("GameData");
                obj.AddComponent<GameData>();
            }

            // FeverManagerが存在しなければ作成
            if (FindFirstObjectByType<FeverManager>() == null)
            {
                GameObject feverObj = new GameObject("FeverManager");
                feverObj.AddComponent<FeverManager>();
                Debug.Log("[GameManager] FeverManager created");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        GameEvents.OnReturnToTitle += GoToTitle;
        GameEvents.OnGameRestart += RestartGame;
        GameEvents.OnGameClear += OnGameClear;
        GameEvents.OnGameOver += OnGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnReturnToTitle -= GoToTitle;
        GameEvents.OnGameRestart -= RestartGame;
        GameEvents.OnGameClear -= OnGameClear;
        GameEvents.OnGameOver -= OnGameOver;
    }

    void Start()
    {
        Debug.Log("[GameManager] Start: Game ready.");

        // 背景色を強制設定
        SetupBackgroundColor();

        // カウントダウンを開始（1フレーム待ってから）
        StartCoroutine(StartCountdownCoroutine());
    }

    IEnumerator StartCountdownCoroutine()
    {
        // 他のスクリプトの初期化を待つ
        yield return null;

        Debug.Log($"[GameManager] StartCountdownCoroutine: CountdownManager.Instance = {CountdownManager.Instance}");

        if (CountdownManager.Instance != null)
        {
            Debug.Log("[GameManager] StartCountdown: Starting countdown");
            CountdownManager.Instance.StartCountdown(OnCountdownComplete);
        }
        else
        {
            Debug.LogWarning("[GameManager] StartCountdown: CountdownManager not found, skipping countdown");
            OnCountdownComplete();
        }
    }

    void OnCountdownComplete()
    {
        Debug.Log("[GameManager] OnCountdownComplete: Countdown finished, setting Ready state");
        if (GameState.Instance != null)
        {
            GameState.Instance.SetState(GameStateType.Ready);
        }
    }

    void SetupBackgroundColor()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = GameColors.Background;
            cam.clearFlags = CameraClearFlags.SolidColor;
            Debug.Log($"[GameManager] Background color set to: {GameColors.Background}");
        }
    }

    void Update()
    {
        if (useTimer && GameState.Instance != null)
        {
            GameState.Instance.UpdateTime(Time.deltaTime);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// クリア時の処理
    /// </summary>
    void OnGameClear()
    {
        Debug.Log("[GameManager] OnGameClear: Saving data, waiting for button press");

        if (GameState.Instance != null && GameData.Instance != null)
        {
            GameData.Instance.SaveClearData(
                GameState.Instance.Kirimi,
                GameState.Instance.TotalBricks,
                GameState.Instance.DestroyedBricks,
                GameState.Instance.RemainingTime,
                GameState.Instance.Lives
            );
        }

        // 自動遷移しない。UIManagerのパネルでボタン押下を待つ。
    }

    /// <summary>
    /// ゲームオーバー時の処理
    /// UIManagerがGameOverPanelを表示するため、シーン遷移は不要
    /// </summary>
    void OnGameOver()
    {
        Debug.Log("[GameManager] OnGameOver: Saving data (UIManager will show GameOverPanel)");

        if (GameState.Instance != null && GameData.Instance != null)
        {
            bool isTimeout = GameState.Instance.RemainingTime <= 0;
            GameData.Instance.SaveGameOverData(
                GameState.Instance.Kirimi,
                GameState.Instance.TotalBricks,
                GameState.Instance.DestroyedBricks,
                GameState.Instance.RemainingTime,
                GameState.Instance.Lives,
                isTimeout
            );
        }

        // UIManagerがOnGameOverイベントでGameOverPanelを表示するため、シーン遷移は不要
    }

    /// <summary>
    /// シーンをリロード
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[GameManager] RestartGame: Reloading scene");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// タイトルシーンへ遷移
    /// </summary>
    public void GoToTitle()
    {
        Debug.Log("[GameManager] GoToTitle: Loading TitleScene");
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ポーズ切り替え
    /// </summary>
    public void TogglePause()
    {
        if (GameState.Instance == null) return;

        if (GameState.Instance.CurrentState == GameStateType.Playing)
        {
            GameState.Instance.SetState(GameStateType.Paused);
            GameEvents.TriggerGamePause();
        }
        else if (GameState.Instance.CurrentState == GameStateType.Paused)
        {
            GameState.Instance.SetState(GameStateType.Playing);
            GameEvents.TriggerGameResume();
        }
    }
}
