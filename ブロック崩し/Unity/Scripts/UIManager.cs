using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI表示を管理するコンポーネント
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI newHighScoreText;

    [Header("Victory UI")]
    [SerializeField] private TextMeshProUGUI victoryScoreText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private int highScore = 0;
    private const string HIGH_SCORE_KEY = "BreakoutHighScore";

    private void Start()
    {
        LoadHighScore();
        SetupButtons();
        SetupGameManagerEvents();
        ShowPanel(GameManager.GameState.Ready);
    }

    /// <summary>
    /// ボタンのイベントを設定
    /// </summary>
    private void SetupButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    /// <summary>
    /// GameManagerのイベントを購読
    /// </summary>
    private void SetupGameManagerEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    /// <summary>
    /// スコア表示を更新
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {score}";
        }
    }

    /// <summary>
    /// 残機表示を更新
    /// </summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"残機: {lives}";
        }
    }

    /// <summary>
    /// ハイスコア表示を更新
    /// </summary>
    public void UpdateHighScore(int score)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"ハイスコア: {score}";
        }
    }

    /// <summary>
    /// ゲーム状態変更を処理
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState state)
    {
        ShowPanel(state);

        if (state == GameManager.GameState.GameOver || state == GameManager.GameState.Victory)
        {
            int finalScore = GameManager.Instance.Score;
            ShowFinalScore(finalScore);
            CheckHighScore(finalScore);
        }
    }

    /// <summary>
    /// 適切なパネルを表示
    /// </summary>
    private void ShowPanel(GameManager.GameState state)
    {
        // 全パネルを非表示
        if (startPanel != null) startPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // 状態に応じたパネルを表示
        switch (state)
        {
            case GameManager.GameState.Ready:
                if (startPanel != null) startPanel.SetActive(true);
                break;
            case GameManager.GameState.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
            case GameManager.GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                break;
            case GameManager.GameState.Victory:
                if (victoryPanel != null) victoryPanel.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 最終スコアを表示
    /// </summary>
    private void ShowFinalScore(int score)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"最終スコア: {score}";
        }

        if (victoryScoreText != null)
        {
            victoryScoreText.text = $"スコア: {score}";
        }
    }

    /// <summary>
    /// ハイスコアをチェック・更新
    /// </summary>
    private void CheckHighScore(int score)
    {
        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
            UpdateHighScore(highScore);

            if (newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(true);
                newHighScoreText.text = "NEW HIGH SCORE!";
            }
        }
        else
        {
            if (newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ハイスコアを保存
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ハイスコアを読み込み
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateHighScore(highScore);
    }

    // ボタンイベントハンドラ
    private void OnStartClicked()
    {
        GameManager.Instance?.StartGame();
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // イベントの購読解除
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
}
