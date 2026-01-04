using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ResultScene管理（CSV対応版）
/// クリア/ゲームオーバーを分岐表示
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("Clear Panel")]
    public GameObject clearPanel;
    public TextMeshProUGUI clearTitleText;
    public TextMeshProUGUI kirimiText;
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI flavorText;
    public Button continueButton;
    public Button endButton;

    [Header("GameOver Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverReasonText;
    public TextMeshProUGUI gameOverFlavorText;
    public Button retryButton;
    public Button titleButton;

    [Header("Stats (Optional)")]
    public TextMeshProUGUI statsText;

    private TextDataManager textManager => TextDataManager.Instance;
    private GameData gameData => GameData.Instance;

    void Awake()
    {
        // TextDataManagerが存在しなければ作成
        if (TextDataManager.Instance == null)
        {
            GameObject obj = new GameObject("TextDataManager");
            obj.AddComponent<TextDataManager>();
        }
    }

    void Start()
    {
        Debug.Log("[ResultManager] Start");

        AutoSetupUIReferences();
        SetupButtonListeners();

        // データに応じて表示を切り替え
        if (gameData != null)
        {
            if (gameData.isGameClear)
                ShowClearResult();
            else
                ShowGameOverResult();
        }
        else
        {
            Debug.LogWarning("[ResultManager] GameData not found, showing default clear");
            ShowClearResult();
        }
    }

    void AutoSetupUIReferences()
    {
        GameObject canvas = GameObject.Find("ResultCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("[ResultManager] ResultCanvas not found");
            return;
        }

        // Clear Panel
        if (clearPanel == null)
        {
            Transform t = canvas.transform.Find("ClearPanel");
            if (t != null)
            {
                clearPanel = t.gameObject;
                clearTitleText = FindChild<TextMeshProUGUI>(clearPanel, "TitleText");
                kirimiText = FindChild<TextMeshProUGUI>(clearPanel, "KirimiText");
                rankText = FindChild<TextMeshProUGUI>(clearPanel, "RankText");
                flavorText = FindChild<TextMeshProUGUI>(clearPanel, "FlavorText");
                continueButton = FindChild<Button>(clearPanel, "ContinueButton");
                endButton = FindChild<Button>(clearPanel, "EndButton");
            }
        }

        // GameOver Panel
        if (gameOverPanel == null)
        {
            Transform t = canvas.transform.Find("GameOverPanel");
            if (t != null)
            {
                gameOverPanel = t.gameObject;
                gameOverTitleText = FindChild<TextMeshProUGUI>(gameOverPanel, "TitleText");
                gameOverReasonText = FindChild<TextMeshProUGUI>(gameOverPanel, "ReasonText");
                gameOverFlavorText = FindChild<TextMeshProUGUI>(gameOverPanel, "FlavorText");
                retryButton = FindChild<Button>(gameOverPanel, "RetryButton");
                titleButton = FindChild<Button>(gameOverPanel, "TitleButton");
            }
        }

        // Stats
        if (statsText == null)
            statsText = FindChild<TextMeshProUGUI>(canvas, "StatsText");
    }

    T FindChild<T>(GameObject parent, string name) where T : Component
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                T comp = child.GetComponent<T>();
                if (comp != null) return comp;
            }
        }
        return null;
    }

    void SetupButtonListeners()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        if (endButton != null)
        {
            endButton.onClick.RemoveAllListeners();
            endButton.onClick.AddListener(OnEndClicked);
        }
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }
        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(OnTitleClicked);
        }
    }

    /// <summary>
    /// クリア結果を表示
    /// </summary>
    void ShowClearResult()
    {
        Debug.Log("[ResultManager] ShowClearResult");

        if (clearPanel != null) clearPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (textManager == null) return;

        // タイトル
        if (clearTitleText != null)
            clearTitleText.text = textManager.GetText("result", "clear", "panel_title", "クリア結果");

        if (gameData != null)
        {
            // きりみ数
            if (kirimiText != null)
            {
                string label = textManager.GetText("result", "clear", "kirimi_label", "さばいたきりみ:");
                kirimiText.text = $"{label} {gameData.finalKirimi}";
            }

            // ランク
            string rankKey = gameData.GetRankKey();
            string rankName = textManager.GetText("result", "rank", rankKey, rankKey);
            if (rankText != null)
            {
                string prefix = textManager.GetText("result", "clear", "rank_label", "あなたの実力は");
                string suffix = textManager.GetText("result", "clear", "rank_suffix", "です");
                rankText.text = $"{prefix}({rankName}){suffix}";
            }

            // フレーバーテキスト
            if (flavorText != null)
            {
                flavorText.text = textManager.GetText("result", "flavor", rankKey, "");
            }

            // 統計情報
            ShowStats();
        }
    }

    /// <summary>
    /// ゲームオーバー結果を表示
    /// </summary>
    void ShowGameOverResult()
    {
        Debug.Log("[ResultManager] ShowGameOverResult");

        if (clearPanel != null) clearPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (textManager == null) return;

        // タイトル
        if (gameOverTitleText != null)
            gameOverTitleText.text = textManager.GetText("result", "gameover", "panel_title", "No sabaki...");

        if (gameData != null)
        {
            // 理由に応じた分岐
            string stateKey = gameData.isTimeOut ? "gameover_time" : "gameover_life";

            // 理由テキスト
            if (gameOverReasonText != null)
            {
                gameOverReasonText.text = textManager.GetText("result", stateKey, "reason", "");
            }

            // フレーバーテキスト
            if (gameOverFlavorText != null)
            {
                gameOverFlavorText.text = textManager.GetText("result", stateKey, "flavor", "");
            }

            // 統計情報
            ShowStats();
        }
    }

    /// <summary>
    /// 統計情報を表示
    /// </summary>
    void ShowStats()
    {
        if (statsText == null || gameData == null || textManager == null) return;

        string stats = "";
        stats += $"{textManager.GetText("result", "stats", "destroyed", "破壊数:")} {gameData.destroyedBricks}/{gameData.totalBricks}\n";
        stats += $"{textManager.GetText("result", "stats", "clear_rate", "クリア率:")} {gameData.GetClearPercentage():F1}%\n";

        if (gameData.isGameClear)
        {
            int min = Mathf.FloorToInt(gameData.remainingTime / 60f);
            int sec = Mathf.FloorToInt(gameData.remainingTime % 60f);
            stats += $"{textManager.GetText("result", "stats", "time_remaining", "残り時間:")} {min:00}:{sec:00}\n";
            stats += $"{textManager.GetText("result", "stats", "lives_remaining", "残りライフ:")} {gameData.remainingLives}";
        }

        statsText.text = stats;
    }

    // ========== ボタンコールバック ==========

    void OnContinueClicked()
    {
        Debug.Log("[ResultManager] Continue clicked - Restart game");
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    void OnRetryClicked()
    {
        Debug.Log("[ResultManager] Retry clicked - Restart game");
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    void OnEndClicked()
    {
        Debug.Log("[ResultManager] End clicked - Go to title");
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    void OnTitleClicked()
    {
        Debug.Log("[ResultManager] Title clicked - Go to title");
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}
