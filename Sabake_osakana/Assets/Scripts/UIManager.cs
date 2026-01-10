using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI全体の管理（CSVテキストデータ参照版）
/// テキストは全てTextDataManager経由で取得
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Debug Settings")]
    public bool enableDebugLog = true;

    [Header("HUD Elements")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI kirimiText;
    public Button pauseButton;

    [Header("Result Panel (Clear)")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultKirimiText;
    public TextMeshProUGUI resultRankText;
    public TextMeshProUGUI resultFlavorText;
    public Button continueButton;
    public Button endButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverFlavorText;
    public Button retryButton;
    public Button titleButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public TextMeshProUGUI pauseTitleText;
    public Button resumeButton;
    public Button pauseTitleButton;

    [Header("Ready Message")]
    public TextMeshProUGUI readyMessageText;

    // マルチボールゲージUI（円形）
    private GameObject multiBallGaugeContainer;
    private Image multiBallGaugeFill;
    private const int GAUGE_MAX = 10;

    // テキストデータ参照用
    private TextDataManager textManager => TextDataManager.Instance;

    // レイアウトデータ参照用
    private UILayoutManager layoutManager => UILayoutManager.Instance;

    /// <summary>
    /// RectTransformにレイアウトを適用（UILayoutManager経由）
    /// </summary>
    bool ApplyLayout(RectTransform rt, string elementName)
    {
        if (layoutManager != null && layoutManager.IsLoaded)
        {
            return layoutManager.ApplyLayout(rt, elementName);
        }
        return false;
    }

    /// <summary>
    /// レイアウトデータを取得（UILayoutManager経由）
    /// </summary>
    UILayoutData GetLayoutData(string elementName)
    {
        if (layoutManager != null && layoutManager.IsLoaded)
        {
            return layoutManager.GetLayout(elementName);
        }
        return null;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            AutoSetupUIReferences();
            // パネルを即座に非表示（Start()より前に実行）
            HideAllPanelsImmediate();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void HideAllPanelsImmediate()
    {
        // Awake時点で見つかったパネルを即座に非表示
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null) return;

        string[] panelNames = { "ResultPanel", "GameOverPanel", "PausePanel" };
        foreach (string name in panelNames)
        {
            Transform t = canvas.transform.Find(name);
            if (t != null && t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(false);
                Debug.Log($"[UIManager] Awake: {name} hidden immediately");
            }
        }
    }

    void AutoSetupUIReferences()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null) return;

        // HUD
        if (livesText == null) livesText = FindUIElement<TextMeshProUGUI>(canvas, "LivesText");
        if (timerText == null) timerText = FindUIElement<TextMeshProUGUI>(canvas, "TimerText");
        if (kirimiText == null) kirimiText = FindUIElement<TextMeshProUGUI>(canvas, "KirimiText");
        if (pauseButton == null) pauseButton = FindUIElement<Button>(canvas, "PauseButton");
        if (readyMessageText == null) readyMessageText = FindUIElement<TextMeshProUGUI>(canvas, "ReadyMessageText");

        // ResultPanel
        if (resultPanel == null)
        {
            Transform t = canvas.transform.Find("ResultPanel");
            if (t != null)
            {
                resultPanel = t.gameObject;
                resultTitleText = FindUIElement<TextMeshProUGUI>(resultPanel, "ResultTitleText");
                resultKirimiText = FindUIElement<TextMeshProUGUI>(resultPanel, "ResultKirimiText");
                resultRankText = FindUIElement<TextMeshProUGUI>(resultPanel, "ResultRankText");
                resultFlavorText = FindUIElement<TextMeshProUGUI>(resultPanel, "ResultFlavorText");
                continueButton = FindUIElement<Button>(resultPanel, "ContinueButton");
                endButton = FindUIElement<Button>(resultPanel, "EndButton");
            }
        }

        // GameOverPanel
        if (gameOverPanel == null)
        {
            Transform t = canvas.transform.Find("GameOverPanel");
            if (t != null)
            {
                gameOverPanel = t.gameObject;
                gameOverTitleText = FindUIElement<TextMeshProUGUI>(gameOverPanel, "GameOverTitleText");
                gameOverFlavorText = FindUIElement<TextMeshProUGUI>(gameOverPanel, "GameOverFlavorText");
                retryButton = FindUIElement<Button>(gameOverPanel, "RetryButton");
                titleButton = FindUIElement<Button>(gameOverPanel, "TitleButton");
            }
        }

        // PausePanel
        if (pausePanel == null)
        {
            Transform t = canvas.transform.Find("PausePanel");
            if (t != null)
            {
                pausePanel = t.gameObject;
                pauseTitleText = FindUIElement<TextMeshProUGUI>(pausePanel, "PauseTitleText");
                resumeButton = FindUIElement<Button>(pausePanel, "ResumeButton");
                pauseTitleButton = FindUIElement<Button>(pausePanel, "PauseTitleButton");
            }
        }
    }

    T FindUIElement<T>(GameObject parent, string name) where T : Component
    {
        // 直下を検索
        Transform found = parent.transform.Find(name);
        if (found != null)
        {
            T comp = found.GetComponent<T>();
            if (comp != null) return comp;
        }

        // 子階層を検索
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

    void OnEnable()
    {
        GameEvents.OnKirimiChanged += UpdateKirimiDisplay;
        GameEvents.OnLivesChanged += UpdateLivesDisplay;
        GameEvents.OnTimeChanged += UpdateTimerDisplay;
        GameEvents.OnGameClear += ShowResultPanel;
        GameEvents.OnGameOver += ShowGameOverPanel;
        GameEvents.OnGamePause += ShowPausePanel;
        GameEvents.OnGameResume += HidePausePanel;
        GameEvents.OnBallReset += ShowReadyMessage;
        GameEvents.OnBallLaunched += HideReadyMessage;
        GameEvents.OnMultiBallGaugeChanged += UpdateMultiBallGauge;
    }

    void OnDisable()
    {
        GameEvents.OnKirimiChanged -= UpdateKirimiDisplay;
        GameEvents.OnLivesChanged -= UpdateLivesDisplay;
        GameEvents.OnTimeChanged -= UpdateTimerDisplay;
        GameEvents.OnGameClear -= ShowResultPanel;
        GameEvents.OnGameOver -= ShowGameOverPanel;
        GameEvents.OnGamePause -= ShowPausePanel;
        GameEvents.OnGameResume -= HidePausePanel;
        GameEvents.OnBallReset -= ShowReadyMessage;
        GameEvents.OnBallLaunched -= HideReadyMessage;
        GameEvents.OnMultiBallGaugeChanged -= UpdateMultiBallGauge;
    }

    void Start()
    {
        DebugLog("=== UIManager Start ===");
        DebugLogScreenInfo();
        DebugLogPanelStates("Before HideAllPanels");

        HideAllPanels();

        DebugLogPanelStates("After HideAllPanels");

        SetupButtonListeners();
        CreateDebugButtons();
        ShowReadyMessage();
        InitializeHUD();

        DebugLogUIElementSizes();
        DebugLog("=== UIManager Start Complete ===");
    }

    void Update()
    {
        // クリック位置デバッグ
        if (enableDebugLog && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            DebugLog($"[Click] Screen Position: ({mousePos.x:F0}, {mousePos.y:F0})");

            // UIのレイキャストでヒットしたオブジェクトを表示
            UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointerData.position = mousePos;
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current?.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                DebugLog($"[Click] Hit UI: {results[0].gameObject.name}");
            }
        }
    }

    void DebugLog(string message)
    {
        if (enableDebugLog)
            Debug.Log($"[UIManager] {message}");
    }

    void DebugLogScreenInfo()
    {
        DebugLog($"Screen Size: {Screen.width} x {Screen.height}");
        DebugLog($"Screen DPI: {Screen.dpi}");

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = GameObject.Find("GameCanvas");
            if (canvasObj != null) canvas = canvasObj.GetComponent<Canvas>();
        }

        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                DebugLog($"Canvas Scaler Mode: {scaler.uiScaleMode}");
                DebugLog($"Reference Resolution: {scaler.referenceResolution}");
                DebugLog($"Match Width/Height: {scaler.matchWidthOrHeight}");
            }
            DebugLog($"Canvas Scale Factor: {canvas.scaleFactor}");
        }
    }

    void DebugLogPanelStates(string context)
    {
        DebugLog($"--- Panel States ({context}) ---");
        DebugLog($"resultPanel: {(resultPanel != null ? (resultPanel.activeSelf ? "ACTIVE" : "inactive") : "NULL")}");
        DebugLog($"gameOverPanel: {(gameOverPanel != null ? (gameOverPanel.activeSelf ? "ACTIVE" : "inactive") : "NULL")}");
        DebugLog($"pausePanel: {(pausePanel != null ? (pausePanel.activeSelf ? "ACTIVE" : "inactive") : "NULL")}");
    }

    void DebugLogUIElementSizes()
    {
        DebugLog("--- UI Element Sizes ---");

        if (livesText != null)
        {
            RectTransform rt = livesText.GetComponent<RectTransform>();
            DebugLog($"LivesText: pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) fontSize={livesText.fontSize}");
        }

        if (timerText != null)
        {
            RectTransform rt = timerText.GetComponent<RectTransform>();
            DebugLog($"TimerText: pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) fontSize={timerText.fontSize}");
        }

        if (kirimiText != null)
        {
            RectTransform rt = kirimiText.GetComponent<RectTransform>();
            DebugLog($"KirimiText: pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) fontSize={kirimiText.fontSize}");
        }

        if (readyMessageText != null)
        {
            RectTransform rt = readyMessageText.GetComponent<RectTransform>();
            DebugLog($"ReadyMessageText: pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) fontSize={readyMessageText.fontSize}");
        }

        if (gameOverTitleText != null)
        {
            RectTransform rt = gameOverTitleText.GetComponent<RectTransform>();
            DebugLog($"GameOverTitleText: pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) fontSize={gameOverTitleText.fontSize}");
        }

        if (resultTitleText != null)
        {
            RectTransform rt = resultTitleText.GetComponent<RectTransform>();
            DebugLog($"ResultTitleText: pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) fontSize={resultTitleText.fontSize}");
        }
    }

    void InitializeHUD()
    {
        // きりみ表示をライフの下（左上）に移動
        if (kirimiText != null)
        {
            RectTransform rt = kirimiText.GetComponent<RectTransform>();
            // レイアウトデータから取得（なければフォールバック）
            if (!ApplyLayout(rt, "KirimiText"))
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(20, -60);
                rt.sizeDelta = new Vector2(250, 40);
            }
            kirimiText.alignment = TextAlignmentOptions.Left;
        }

        // マルチボールゲージUIを作成
        CreateMultiBallGaugeUI();

        if (GameState.Instance != null)
        {
            UpdateKirimiDisplay(GameState.Instance.Kirimi);
            UpdateLivesDisplay(GameState.Instance.Lives);
            UpdateTimerDisplay(GameState.Instance.RemainingTime);
        }
        else
        {
            UpdateKirimiDisplay(0);
            UpdateLivesDisplay(5);
            UpdateTimerDisplay(90f);
        }

        // ゲージ初期化
        UpdateMultiBallGauge(0);
    }

    void SetupButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        if (endButton != null)
        {
            endButton.onClick.RemoveAllListeners();
            endButton.onClick.AddListener(OnEndButtonClicked);
        }
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(OnTitleButtonClicked);
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }
        if (pauseTitleButton != null)
        {
            pauseTitleButton.onClick.RemoveAllListeners();
            pauseTitleButton.onClick.AddListener(OnTitleButtonClicked);
        }
    }

    void HideAllPanels()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // ========== HUD更新 ==========

    void UpdateKirimiDisplay(int kirimi)
    {
        if (kirimiText != null)
        {
            // 仕様書フォーマット: 「きりみ: 150」
            kirimiText.text = $"きりみ: {kirimi}";
            kirimiText.color = GameColors.TextHUD;
            DebugLog($"UpdateKirimiDisplay: {kirimi}");
        }
        else
        {
            DebugLog("UpdateKirimiDisplay: kirimiText is NULL!");
        }
    }

    void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            // ハートのみ表示（ライフ：を削除）
            string redHeart = $"<color=#{ColorUtility.ToHtmlStringRGB(GameColors.TextRed)}>♥</color>";
            string grayHeart = $"<color=#{ColorUtility.ToHtmlStringRGB(GameColors.HeartEmpty)}>♥</color>";

            string hearts = "";
            for (int i = 0; i < lives; i++) hearts += redHeart;
            for (int i = lives; i < 5; i++) hearts += grayHeart;

            livesText.text = hearts;
            livesText.color = GameColors.TextHUD;
            livesText.alignment = TextAlignmentOptions.Left;
        }
    }

    void UpdateTimerDisplay(float time)
    {
        if (timerText != null)
        {
            // 仕様書フォーマット: 大きな中央タイマー「17:13」（黒文字、残り10秒以下で赤）
            time = Mathf.Max(0f, time);
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            timerText.color = (time <= 10f) ? GameColors.TextRed : GameColors.TextHUD;
        }
    }

    // ========== マルチボールゲージ（円形） ==========

    void CreateMultiBallGaugeUI()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null) return;

        // レイアウトデータから取得（なければデフォルト値）
        UILayoutData layout = GetLayoutData("MultiBallGauge");
        float gaugeSize = layout?.width ?? 60f;
        Vector2 position = layout?.Position ?? new Vector2(50, -130);

        // コンテナ作成（きりみの下に配置）
        multiBallGaugeContainer = new GameObject("MultiBallGauge");
        multiBallGaugeContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = multiBallGaugeContainer.AddComponent<RectTransform>();
        if (layout != null)
        {
            containerRect.anchorMin = layout.AnchorMin;
            containerRect.anchorMax = layout.AnchorMax;
            containerRect.pivot = layout.Pivot;
            containerRect.anchoredPosition = layout.Position;
            containerRect.sizeDelta = layout.Size;
        }
        else
        {
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = position;
            containerRect.sizeDelta = new Vector2(gaugeSize, gaugeSize);
        }

        // 背景リング（グレー・常に表示）
        GameObject bgRingObj = new GameObject("GaugeBGRing");
        bgRingObj.transform.SetParent(multiBallGaugeContainer.transform, false);

        RectTransform bgRingRect = bgRingObj.AddComponent<RectTransform>();
        bgRingRect.anchorMin = Vector2.zero;
        bgRingRect.anchorMax = Vector2.one;
        bgRingRect.offsetMin = Vector2.zero;
        bgRingRect.offsetMax = Vector2.zero;

        Image bgRingImage = bgRingObj.AddComponent<Image>();
        bgRingImage.sprite = CreateRingSprite(64, 8);
        bgRingImage.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        bgRingImage.type = Image.Type.Simple;
        bgRingImage.preserveAspect = true;

        // 円形ゲージ（Filled Image）
        GameObject fillObj = new GameObject("GaugeFill");
        fillObj.transform.SetParent(multiBallGaugeContainer.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        multiBallGaugeFill = fillObj.AddComponent<Image>();
        multiBallGaugeFill.sprite = CreateRingSprite(64, 8);
        multiBallGaugeFill.color = new Color(0.3f, 0.8f, 1f);  // 水色
        multiBallGaugeFill.type = Image.Type.Filled;
        multiBallGaugeFill.fillMethod = Image.FillMethod.Radial360;
        multiBallGaugeFill.fillOrigin = (int)Image.Origin360.Top;
        multiBallGaugeFill.fillClockwise = true;
        multiBallGaugeFill.fillAmount = 0f;  // 最初は0
        multiBallGaugeFill.preserveAspect = true;

        // 中央に包丁画像
        GameObject knifeObj = new GameObject("KnifeIcon");
        knifeObj.transform.SetParent(multiBallGaugeContainer.transform, false);

        RectTransform knifeRect = knifeObj.AddComponent<RectTransform>();
        knifeRect.anchorMin = new Vector2(0.5f, 0.5f);
        knifeRect.anchorMax = new Vector2(0.5f, 0.5f);
        knifeRect.pivot = new Vector2(0.5f, 0.5f);
        knifeRect.anchoredPosition = Vector2.zero;
        knifeRect.sizeDelta = new Vector2(gaugeSize * 0.6f, gaugeSize * 0.6f);

        Image knifeImage = knifeObj.AddComponent<Image>();
        Sprite knifeSprite = Resources.Load<Sprite>("Sprites/knife");
        if (knifeSprite != null)
        {
            knifeImage.sprite = knifeSprite;
            knifeImage.preserveAspect = true;
        }
        else
        {
            // フォールバック：白い円
            knifeImage.color = Color.white;
        }

        DebugLog("MultiBallGaugeUI (circular) created");
    }

    /// <summary>
    /// リング形状のスプライトを動的に生成
    /// </summary>
    Sprite CreateRingSprite(int size, int thickness)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float center = size / 2f;
        float outerRadius = size / 2f - 1;
        float innerRadius = outerRadius - thickness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist >= innerRadius && dist <= outerRadius)
                {
                    // アンチエイリアス
                    float alpha = 1f;
                    if (dist > outerRadius - 1) alpha = outerRadius - dist + 1;
                    if (dist < innerRadius + 1) alpha = Mathf.Min(alpha, dist - innerRadius + 1);
                    pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(alpha));
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void UpdateMultiBallGauge(int gauge)
    {
        if (multiBallGaugeFill == null) return;

        // 円形ゲージの塗りつぶし量を更新
        float fillPercent = (float)gauge / GAUGE_MAX;
        multiBallGaugeFill.fillAmount = fillPercent;

        // ゲージが満タンに近づくと色が変化
        if (gauge >= 8)
        {
            multiBallGaugeFill.color = new Color(1f, 0.3f, 0.3f);  // 赤（もうすぐ発動）
        }
        else if (gauge >= 5)
        {
            multiBallGaugeFill.color = new Color(1f, 0.8f, 0.3f);  // 黄色
        }
        else
        {
            multiBallGaugeFill.color = new Color(0.3f, 0.8f, 1f);  // 水色
        }

        DebugLog($"UpdateMultiBallGauge: {gauge}/{GAUGE_MAX} ({fillPercent:P0})");
    }

    // ========== Ready Message ==========

    private GameObject readyMessageBackground;

    void ShowReadyMessage()
    {
        if (readyMessageText != null)
        {
            if (textManager != null)
                readyMessageText.text = textManager.GetText("game", "hud", "ready_message", "クリックで発射！");
            else
                readyMessageText.text = "クリックで発射！";

            // 白背景・黒文字・カウントダウンの上に配置
            readyMessageText.color = Color.black;
            readyMessageText.alignment = TextAlignmentOptions.Center;

            // RectTransformをカウントダウンの上に配置
            RectTransform rt = readyMessageText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 120);  // カウントダウンより上
            rt.sizeDelta = new Vector2(300, 60);

            // 白背景を追加（まだなければ）
            if (readyMessageBackground == null)
            {
                readyMessageBackground = new GameObject("ReadyMessageBackground");
                readyMessageBackground.transform.SetParent(readyMessageText.transform.parent, false);
                readyMessageBackground.transform.SetSiblingIndex(readyMessageText.transform.GetSiblingIndex());

                RectTransform bgRect = readyMessageBackground.AddComponent<RectTransform>();
                bgRect.anchorMin = new Vector2(0.5f, 0.5f);
                bgRect.anchorMax = new Vector2(0.5f, 0.5f);
                bgRect.pivot = new Vector2(0.5f, 0.5f);
                bgRect.anchoredPosition = new Vector2(0, 120);  // テキストと同じ位置
                bgRect.sizeDelta = new Vector2(320, 70);

                Image bgImage = readyMessageBackground.AddComponent<Image>();
                bgImage.color = Color.white;

                // 枠線を追加
                Outline outline = readyMessageBackground.AddComponent<Outline>();
                outline.effectColor = new Color(0.3f, 0.3f, 0.3f);
                outline.effectDistance = new Vector2(2, -2);
            }

            readyMessageBackground.SetActive(true);
            readyMessageText.gameObject.SetActive(true);
        }
    }

    void HideReadyMessage()
    {
        if (readyMessageText != null)
            readyMessageText.gameObject.SetActive(false);
        if (readyMessageBackground != null)
            readyMessageBackground.SetActive(false);
    }

    // ========== リザルト画面 ==========

    void ShowResultPanel()
    {
        DebugLog($"ShowResultPanel called");
        if (resultPanel == null)
        {
            DebugLog("ShowResultPanel: resultPanel is NULL!");
            return;
        }
        resultPanel.SetActive(true);

        // パネルとボタンの色を強制設定
        ApplyPanelColors(resultPanel, continueButton, endButton);

        // GameStateから直接値を取得
        if (GameState.Instance != null)
        {
            int kirimi = GameState.Instance.Kirimi;
            int destroyed = GameState.Instance.DestroyedBricks;
            int total = GameState.Instance.TotalBricks;
            bool isPerfectClear = destroyed >= total && total > 0;
            float clearPercent = total > 0 ? (float)destroyed / total * 100f : 0f;

            // クリア率ベースでランク判定
            string rankName;
            string flavor;
            string titleText = "クリア結果";

            if (isPerfectClear)
            {
                rankName = "おさしみ級";
                flavor = "おめでとう！完全クリア！\n先ほどまで生きていた新鮮なおさしみ。";
                titleText = "パーフェクト！";
            }
            else if (clearPercent >= 80f)
            {
                rankName = "たたき級";
                flavor = "なかなかの腕前！\nおいしいたたきになりました！";
            }
            else if (clearPercent >= 60f)
            {
                rankName = "にぎり級";
                flavor = "まずまずの出来！\nにぎり寿司にしましょう！";
            }
            else if (clearPercent >= 40f)
            {
                rankName = "あら煮級";
                flavor = "もう少しがんばろう！\nあら煮にして食べよう！";
            }
            else
            {
                rankName = "エサ級";
                flavor = "練習あるのみ！\n...エサにするしかない...";
            }

            // 仕様書フォーマット
            if (resultTitleText != null)
            {
                resultTitleText.text = titleText;
                resultTitleText.color = isPerfectClear ? GameColors.TextRed : GameColors.TextBlack;
            }

            if (resultKirimiText != null)
            {
                resultKirimiText.text = $"さばいたきりみ: {kirimi}";
                resultKirimiText.color = GameColors.TextBlack;
            }

            if (resultRankText != null)
            {
                // ランク部分を赤色で強調（リッチテキスト）
                string redColor = ColorUtility.ToHtmlStringRGB(GameColors.TextRed);
                resultRankText.text = $"あなたの実力は(<color=#{redColor}>{rankName}</color>)です";
                resultRankText.color = GameColors.TextBlack;
            }

            if (resultFlavorText != null)
            {
                resultFlavorText.text = flavor;
                resultFlavorText.color = GameColors.TextBlack;
            }

            // ボタンテキストを設定
            SetButtonText(continueButton, "つづける");
            SetButtonText(endButton, "おわる");
        }
    }

    void SetButtonText(Button btn, string text)
    {
        if (btn == null) return;
        var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = text;
            btnText.color = GameColors.TextBlack;
        }
    }

    // ========== ゲームオーバー画面 ==========

    void ShowGameOverPanel()
    {
        DebugLog($"ShowGameOverPanel called");

        if (gameOverPanel == null)
        {
            DebugLog("ShowGameOverPanel: gameOverPanel is NULL!");
            return;
        }
        gameOverPanel.SetActive(true);

        // パネルとボタンの色を強制設定
        ApplyPanelColors(gameOverPanel, titleButton, null);

        // 仕様書フォーマット: タイトル「No sabaki...」、フレーバーテキスト
        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = "No sabaki...";
            gameOverTitleText.color = GameColors.TextBlack;
        }

        if (gameOverFlavorText != null)
        {
            // 仕様書: 「材料を用意できないなら、あなた自身が供されるほかないだろう」
            gameOverFlavorText.text = "材料を用意できないなら、\nあなた自身が供されるほかないだろう";
            gameOverFlavorText.color = GameColors.TextBlack;
        }

        // ボタンテキストを設定
        if (titleButton != null)
        {
            var btnText = titleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = "タイトルへ";
                btnText.color = GameColors.TextBlack;
            }
        }
    }

    // ========== ポーズ画面 ==========

    void ShowPausePanel()
    {
        if (pausePanel == null) return;
        pausePanel.SetActive(true);

        // パネルとボタンの色を強制設定
        ApplyPanelColors(pausePanel, resumeButton, pauseTitleButton);

        if (textManager != null && pauseTitleText != null)
            pauseTitleText.text = textManager.GetText("game", "pause", "panel_title", "ポーズ");
    }

    void HidePausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // ========== パネル色設定 ==========

    void ApplyPanelColors(GameObject panel, Button btn1, Button btn2)
    {
        // パネル背景をライトグレーに
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = GameColors.PanelBackground;
        }

        // パネルにダークグレーのフチを追加
        Outline panelOutline = panel.GetComponent<Outline>();
        if (panelOutline == null)
        {
            panelOutline = panel.AddComponent<Outline>();
        }
        panelOutline.effectColor = GameColors.ButtonBorder;  // ダークグレー
        panelOutline.effectDistance = new Vector2(3, -3);

        // 2つ目のOutlineで太くする
        Outline[] outlines = panel.GetComponents<Outline>();
        if (outlines.Length < 2)
        {
            Outline outline2 = panel.AddComponent<Outline>();
            outline2.effectColor = GameColors.ButtonBorder;
            outline2.effectDistance = new Vector2(-3, 3);
        }

        // ボタンの色とOutlineを設定
        ApplyButtonStyle(btn1);
        ApplyButtonStyle(btn2);
    }

    void ApplyButtonStyle(Button btn)
    {
        if (btn == null) return;

        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = GameColors.ButtonBackground;
        }

        // Outlineがなければ追加、あれば色を設定
        Outline outline = btn.GetComponent<Outline>();
        if (outline == null)
        {
            outline = btn.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = GameColors.ButtonBorder;
        outline.effectDistance = new Vector2(2, -2);
    }

    // ========== ボタンコールバック ==========

    void OnPauseButtonClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    void OnResumeButtonClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    void OnContinueButtonClicked()
    {
        // 次のステージへ進む（StageManager経由）
        Debug.Log("[UIManager] Continue clicked - Go to next stage");
        if (StageManager.Instance != null)
        {
            StageManager.Instance.GoToNextStage();
        }
        else
        {
            // フォールバック：現在のシーンをリロード
            Debug.LogWarning("[UIManager] StageManager not found, reloading current scene");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    void OnRetryButtonClicked()
    {
        GameEvents.TriggerGameRestart();
    }

    void OnEndButtonClicked()
    {
        GameEvents.TriggerReturnToTitle();
    }

    void OnTitleButtonClicked()
    {
        GameEvents.TriggerReturnToTitle();
    }

    // ========== デバッグ用ボタン ==========

    void CreateDebugButtons()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null) return;

        // タイムアウトボタン
        CreateDebugButton(canvas.transform, "TimeoutBtn", "TIME\nOUT",
            new Vector2(-60, -100), OnDebugTimeoutClicked);

        // 全ブロック破壊ボタン
        CreateDebugButton(canvas.transform, "ClearBtn", "ALL\nCLEAR",
            new Vector2(-60, -180), OnDebugClearClicked);

        Debug.Log("[UIManager] Debug buttons created");
        #endif
    }

    void CreateDebugButton(Transform parent, string name, string label, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        // RectTransform設定（右端に配置）
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(50, 50);

        // 背景Image
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

        // Button
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        // テキスト
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 10;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
    }

    void OnDebugTimeoutClicked()
    {
        Debug.Log("[UIManager] Debug: Force Timeout!");
        if (GameState.Instance != null)
        {
            // 残り時間を0にしてタイムアップを発火
            GameEvents.TriggerTimeUp();
        }
    }

    void OnDebugClearClicked()
    {
        Debug.Log("[UIManager] Debug: Force Clear!");
        if (GameState.Instance != null)
        {
            GameState.Instance.SetState(GameStateType.GameClear);
            GameEvents.TriggerGameClear();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
