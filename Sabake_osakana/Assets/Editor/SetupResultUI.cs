using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// ResultScene UIを仕様書通りにセットアップ
/// 基準解像度: 800x600 (4:3)
/// </summary>
public class SetupResultUI : Editor
{
    const float REF_WIDTH = 800f;
    const float REF_HEIGHT = 600f;

    // 色定義（GameColorsと同期）
    static readonly Color COLOR_PANEL_BG = new Color(0.972f, 0.969f, 0.949f); // 仕様書準拠
    static readonly Color COLOR_BLACK = new Color(0.1f, 0.1f, 0.1f);
    static readonly Color COLOR_RANK = new Color(0.9f, 0.2f, 0.2f);
    static readonly Color COLOR_BUTTON_BG = new Color(0.4f, 0.65f, 0.5f); // 緑系ボタン
    static readonly Color COLOR_BUTTON_BORDER = new Color(0.6f, 0.2f, 0.2f);
    static readonly Color COLOR_BACKGROUND = new Color(0.508f, 0.509f, 0.493f); // ゲーム背景

    [MenuItem("Tools/Setup Result UI (Spec Compliant)")]
    public static void SetupUI()
    {
        // ResultCanvasを検索または作成
        GameObject canvasObj = GameObject.Find("ResultCanvas");
        if (canvasObj == null)
        {
            // 新規作成
            canvasObj = new GameObject("ResultCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        Canvas canvasComp = canvasObj.GetComponent<Canvas>();
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();

        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 既存の子をクリア
        ClearChildren(canvasObj.transform);

        // 背景
        CreateBackground(canvasObj.transform);

        // ClearPanel
        CreateClearPanel(canvasObj.transform);

        // GameOverPanel
        CreateGameOverPanel(canvasObj.transform);

        // ResultManagerをアタッチ
        if (canvasObj.GetComponent<ResultManager>() == null)
            canvasObj.AddComponent<ResultManager>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupResultUI] UI構築完了！");
        EditorUtility.DisplayDialog("完了", "ResultScene UIを構築しました。\nシーンを保存してください（Ctrl+S）", "OK");
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);
    }

    static void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        bg.transform.SetAsFirstSibling();

        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = bg.AddComponent<Image>();
        img.color = COLOR_BACKGROUND; // 仕様書準拠のグレー
    }

    static void CreateClearPanel(Transform parent)
    {
        // パネル
        GameObject panel = CreatePanel(parent, "ClearPanel", new Vector2(500, 450));

        // タイトル
        CreateText(panel.transform, "TitleText", "クリア結果",
            new Vector2(0, 180), new Vector2(400, 50), 36, TextAlignmentOptions.Center);

        // きりみ数
        CreateText(panel.transform, "KirimiText", "さばいたきりみ: 150",
            new Vector2(0, 100), new Vector2(400, 40), 24, TextAlignmentOptions.Center);

        // ランク
        var rankText = CreateText(panel.transform, "RankText", "あなたの実力は(おさしみ級)です",
            new Vector2(0, 40), new Vector2(450, 50), 28, TextAlignmentOptions.Center);
        rankText.color = COLOR_RANK;

        // フレーバーテキスト
        CreateText(panel.transform, "FlavorText", "見事なさばきっぷり！\n新鮮なおさしみができました！",
            new Vector2(0, -40), new Vector2(400, 80), 18, TextAlignmentOptions.Center);

        // 統計情報
        CreateText(panel.transform, "StatsText", "",
            new Vector2(0, -120), new Vector2(350, 80), 14, TextAlignmentOptions.Left);

        // ボタン: つづける
        CreateButton(panel.transform, "ContinueButton", "つづける",
            new Vector2(-90, -190), new Vector2(150, 50), 22);

        // ボタン: おわる
        CreateButton(panel.transform, "EndButton", "おわる",
            new Vector2(90, -190), new Vector2(150, 50), 22);

        Debug.Log("[SetupResultUI] ClearPanel作成完了");
    }

    static void CreateGameOverPanel(Transform parent)
    {
        // パネル
        GameObject panel = CreatePanel(parent, "GameOverPanel", new Vector2(450, 400));
        panel.SetActive(false); // 初期非表示

        // タイトル
        CreateText(panel.transform, "TitleText", "No sabaki...",
            new Vector2(0, 150), new Vector2(400, 60), 42, TextAlignmentOptions.Center);

        // 理由
        CreateText(panel.transform, "ReasonText", "ライフがなくなった",
            new Vector2(0, 60), new Vector2(400, 40), 20, TextAlignmentOptions.Center);

        // フレーバーテキスト
        CreateText(panel.transform, "FlavorText", "材料を用意できないなら、\nあなた自身が供されるほかないだろう",
            new Vector2(0, -10), new Vector2(400, 80), 16, TextAlignmentOptions.Center);

        // 統計情報
        CreateText(panel.transform, "StatsText", "",
            new Vector2(0, -80), new Vector2(350, 60), 14, TextAlignmentOptions.Left);

        // ボタン: リトライ
        CreateButton(panel.transform, "RetryButton", "リトライ",
            new Vector2(0, -140), new Vector2(160, 50), 22);

        // ボタン: タイトルへ
        CreateButton(panel.transform, "TitleButton", "タイトルへ",
            new Vector2(0, -200), new Vector2(160, 50), 22);

        Debug.Log("[SetupResultUI] GameOverPanel作成完了");
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = COLOR_PANEL_BG;

        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.4f);
        shadow.effectDistance = new Vector2(5, -5);

        return panel;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = COLOR_BLACK;

        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, int fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = COLOR_PANEL_BG;

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        Outline outline = obj.AddComponent<Outline>();
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

    [MenuItem("Tools/Create ResultScene")]
    public static void CreateResultScene()
    {
        // 新規シーン作成
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // EventSystemがなければ追加
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // UIをセットアップ
        SetupUI();

        // シーンを保存
        string scenePath = "Assets/Scenes/ResultScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log($"[SetupResultUI] ResultScene created at {scenePath}");
        EditorUtility.DisplayDialog("完了", $"ResultSceneを作成しました:\n{scenePath}\n\nBuild Settingsに追加してください", "OK");
    }
}
