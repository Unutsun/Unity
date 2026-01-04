using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// ゲームUIを仕様書通りにセットアップするエディタスクリプト
/// 基準解像度: 800x600 (4:3)
/// </summary>
public class SetupGameUI : Editor
{
    // 基準解像度
    const float REF_WIDTH = 800f;
    const float REF_HEIGHT = 600f;

    // 色定義（GameColorsと同期）
    static readonly Color COLOR_WHITE = Color.white;
    static readonly Color COLOR_BLACK = new Color(0.1f, 0.1f, 0.1f);
    static readonly Color COLOR_HEART_RED = new Color(0.9f, 0.2f, 0.2f);
    static readonly Color COLOR_PANEL_BG = new Color(0.972f, 0.969f, 0.949f); // 仕様書準拠
    static readonly Color COLOR_BUTTON_BG = new Color(0.4f, 0.65f, 0.5f); // 緑系ボタン
    static readonly Color COLOR_BUTTON_BORDER = new Color(0.6f, 0.2f, 0.2f); // 赤茶

    [MenuItem("Tools/Setup Game UI (Spec Compliant)")]
    public static void SetupUI()
    {
        // GameCanvasを検索または作成
        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (canvasObj == null)
        {
            Debug.LogError("[SetupGameUI] GameCanvasが見つかりません。SampleSceneを開いてください。");
            return;
        }

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();

        // Canvas設定
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // CanvasScaler設定 - 4:3基準でスケール
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f; // 幅と高さの中間でマッチ

        Debug.Log($"[SetupGameUI] Canvas設定完了: {REF_WIDTH}x{REF_HEIGHT}");

        // 既存のUI要素をクリア（子オブジェクトを削除）
        ClearChildren(canvasObj.transform);

        // HUD要素を作成
        CreateHUD(canvasObj.transform);

        // ResultPanelを作成
        CreateResultPanel(canvasObj.transform);

        // GameOverPanelを作成
        CreateGameOverPanel(canvasObj.transform);

        // PausePanelを作成
        CreatePausePanel(canvasObj.transform);

        // シーンを変更済みとしてマーク
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupGameUI] UI構築完了！シーンを保存してください。");
        EditorUtility.DisplayDialog("完了", "GameScene UIを構築しました。\nシーンを保存してください（Ctrl+S）", "OK");
    }

    static void ClearChildren(Transform parent)
    {
        // 逆順で削除（削除中のインデックスずれを防ぐ）
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    static void CreateHUD(Transform parent)
    {
        // === 左上エリア: ライフ、制限時間、きりみ ===
        // 仕様: ライフ:♥♥♥♥♥ / 制限時間: 120 / きりみ: 150

        // ライフテキスト（左上）
        var livesText = CreateTextElement(parent, "LivesText",
            "ライフ:♥♥♥♥♥",
            new Vector2(10, -10), new Vector2(200, 30),
            TextAlignmentOptions.Left, 18,
            new Vector2(0, 1), new Vector2(0, 1)); // 左上アンカー

        // 制限時間テキスト（左上、ライフの下）
        var timeLabel = CreateTextElement(parent, "TimeLabelText",
            "制限時間:",
            new Vector2(10, -40), new Vector2(100, 25),
            TextAlignmentOptions.Left, 16,
            new Vector2(0, 1), new Vector2(0, 1));

        // きりみテキスト（左上、制限時間の下）
        var kirimiText = CreateTextElement(parent, "KirimiText",
            "きりみ: 0",
            new Vector2(10, -65), new Vector2(150, 25),
            TextAlignmentOptions.Left, 16,
            new Vector2(0, 1), new Vector2(0, 1));

        // === 中央上: 大きいタイマー ===
        var timerText = CreateTextElement(parent, "TimerText",
            "01:30",
            new Vector2(0, -20), new Vector2(200, 60),
            TextAlignmentOptions.Center, 48,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1)); // 上中央アンカー

        // === 右上: ポーズボタン ===
        var pauseButton = CreateButton(parent, "PauseButton",
            "||",
            new Vector2(-10, -10), new Vector2(50, 50),
            new Vector2(1, 1), new Vector2(1, 1), // 右上アンカー
            24);

        // === 中央: Ready Message ===
        var readyMessage = CreateTextElement(parent, "ReadyMessageText",
            "スペースキーで発射！",
            new Vector2(0, -100), new Vector2(400, 40),
            TextAlignmentOptions.Center, 24,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)); // 中央

        // ライフ表示を別テキストに変更（HUDで更新しやすいように）
        var livesDisplay = CreateTextElement(parent, "LivesText",
            "♥♥♥♥♥",
            new Vector2(70, -10), new Vector2(150, 30),
            TextAlignmentOptions.Left, 20,
            new Vector2(0, 1), new Vector2(0, 1));
        livesDisplay.color = COLOR_HEART_RED;

        // 元のライフラベルを修正
        DestroyImmediate(livesText.gameObject);
        var livesLabel = CreateTextElement(parent, "LivesLabel",
            "ライフ:",
            new Vector2(10, -10), new Vector2(60, 30),
            TextAlignmentOptions.Left, 16,
            new Vector2(0, 1), new Vector2(0, 1));

        Debug.Log("[SetupGameUI] HUD作成完了");
    }

    static void CreateResultPanel(Transform parent)
    {
        // 白背景パネル（中央）
        var panel = CreatePanel(parent, "ResultPanel",
            new Vector2(0, 0), new Vector2(500, 400),
            COLOR_PANEL_BG);
        panel.SetActive(false);

        Transform panelT = panel.transform;

        // タイトル: "クリア結果"
        CreateTextElement(panelT, "ResultTitleText",
            "クリア結果",
            new Vector2(0, 150), new Vector2(400, 50),
            TextAlignmentOptions.Center, 32,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // さばいたきりみ
        CreateTextElement(panelT, "ResultKirimiText",
            "さばいたきりみ: 150",
            new Vector2(0, 80), new Vector2(400, 30),
            TextAlignmentOptions.Center, 20,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // ランク
        var rankText = CreateTextElement(panelT, "ResultRankText",
            "おさしみ級",
            new Vector2(0, 40), new Vector2(400, 40),
            TextAlignmentOptions.Center, 28,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rankText.color = COLOR_HEART_RED;

        // フレーバーテキスト
        CreateTextElement(panelT, "ResultFlavorText",
            "見事なさばきっぷり！\n新鮮なおさしみができました！",
            new Vector2(0, -30), new Vector2(400, 80),
            TextAlignmentOptions.Center, 16,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // ボタン: つづける、おわる
        CreateButton(panelT, "ContinueButton",
            "つづける",
            new Vector2(-80, -140), new Vector2(140, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 20);

        CreateButton(panelT, "EndButton",
            "おわる",
            new Vector2(80, -140), new Vector2(140, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 20);

        Debug.Log("[SetupGameUI] ResultPanel作成完了");
    }

    static void CreateGameOverPanel(Transform parent)
    {
        // 白背景パネル（中央）
        var panel = CreatePanel(parent, "GameOverPanel",
            new Vector2(0, 0), new Vector2(450, 350),
            COLOR_PANEL_BG);
        panel.SetActive(false);

        Transform panelT = panel.transform;

        // タイトル: "No sabaki..."
        CreateTextElement(panelT, "GameOverTitleText",
            "No sabaki...",
            new Vector2(0, 120), new Vector2(400, 50),
            TextAlignmentOptions.Center, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // フレーバーテキスト
        CreateTextElement(panelT, "GameOverFlavorText",
            "材料を用意できないなら、\nあなた自身が供されるほかないだろう",
            new Vector2(0, 20), new Vector2(400, 80),
            TextAlignmentOptions.Center, 16,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // ボタン: タイトルへ
        CreateButton(panelT, "TitleButton",
            "タイトルへ",
            new Vector2(0, -100), new Vector2(160, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 20);

        // リトライボタン追加
        CreateButton(panelT, "RetryButton",
            "リトライ",
            new Vector2(0, -40), new Vector2(160, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 20);

        Debug.Log("[SetupGameUI] GameOverPanel作成完了");
    }

    static void CreatePausePanel(Transform parent)
    {
        // 半透明背景パネル（中央）
        var panel = CreatePanel(parent, "PausePanel",
            new Vector2(0, 0), new Vector2(350, 280),
            COLOR_PANEL_BG);
        panel.SetActive(false);

        Transform panelT = panel.transform;

        // タイトル: "ポーズ"
        CreateTextElement(panelT, "PauseTitleText",
            "ポーズ",
            new Vector2(0, 80), new Vector2(300, 50),
            TextAlignmentOptions.Center, 32,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // ボタン: つづける
        CreateButton(panelT, "ResumeButton",
            "つづける",
            new Vector2(0, 0), new Vector2(160, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 20);

        // ボタン: タイトルへ
        CreateButton(panelT, "PauseTitleButton",
            "タイトルへ",
            new Vector2(0, -70), new Vector2(160, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 20);

        Debug.Log("[SetupGameUI] PausePanel作成完了");
    }

    // ========== ヘルパー関数 ==========

    static TextMeshProUGUI CreateTextElement(Transform parent, string name,
        string text, Vector2 position, Vector2 size,
        TextAlignmentOptions alignment, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin; // ピボットをアンカーに合わせる
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = COLOR_BLACK;

        // フォントは後で適用される想定
        return tmp;
    }

    static Button CreateButton(Transform parent, string name,
        string text, Vector2 position, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        // 背景Image
        Image img = obj.AddComponent<Image>();
        img.color = COLOR_PANEL_BG;

        // Button
        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        // 枠線（Outline）用の子オブジェクト
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(obj.transform, false);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        Outline outline = borderObj.AddComponent<Outline>();
        outline.effectColor = COLOR_BUTTON_BORDER;
        outline.effectDistance = new Vector2(2, 2);

        // テキスト
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = COLOR_BLACK;

        return btn;
    }

    static GameObject CreatePanel(Transform parent, string name,
        Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        // 影効果
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.3f);
        shadow.effectDistance = new Vector2(4, -4);

        return obj;
    }
}
