using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// TitleScene UIを仕様書通りにセットアップするエディタスクリプト
/// 基準解像度: 800x600 (4:3)
/// </summary>
public class SetupTitleUI : Editor
{
    // 基準解像度
    const float REF_WIDTH = 800f;
    const float REF_HEIGHT = 600f;

    // 色定義
    static readonly Color COLOR_BLACK = Color.black;
    static readonly Color COLOR_WHITE = Color.white;
    static readonly Color COLOR_TITLE_BLUE = new Color(0.2f, 0.5f, 0.8f); // タイトルの青
    static readonly Color COLOR_TITLE_YELLOW = new Color(1f, 0.85f, 0.2f); // タイトルの黄色（!）
    static readonly Color COLOR_BUTTON_BG = new Color(0.95f, 0.93f, 0.88f); // クリーム色
    static readonly Color COLOR_BUTTON_BORDER = new Color(0.6f, 0.2f, 0.2f); // 赤茶

    [MenuItem("Tools/Setup Title UI (Spec Compliant)")]
    public static void SetupUI()
    {
        // TitleCanvasを検索または作成
        GameObject canvasObj = GameObject.Find("TitleCanvas");
        if (canvasObj == null)
        {
            Debug.LogError("[SetupTitleUI] TitleCanvasが見つかりません。TitleSceneを開いてください。");
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
        scaler.matchWidthOrHeight = 0.5f;

        Debug.Log($"[SetupTitleUI] Canvas設定完了: {REF_WIDTH}x{REF_HEIGHT}");

        // 既存のUI要素をクリア（子オブジェクトを削除）
        ClearChildren(canvasObj.transform);

        // 背景（魚のシルエット模様 - 後でImageで設定可能）
        CreateBackground(canvasObj.transform);

        // タイトルテキスト: "さばけ！おさかな"
        CreateTitle(canvasObj.transform);

        // スタートボタン: "さばく！"
        CreateStartButton(canvasObj.transform);

        // シーンを変更済みとしてマーク
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupTitleUI] UI構築完了！シーンを保存してください。");
        EditorUtility.DisplayDialog("完了", "TitleScene UIを構築しました。\nシーンを保存してください（Ctrl+S）", "OK");
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    static void CreateBackground(Transform parent)
    {
        // 背景パネル（後で画像を設定できる）
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent, false);
        bgObj.transform.SetAsFirstSibling(); // 最背面に

        RectTransform rect = bgObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = bgObj.AddComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 0.92f); // 薄いクリーム色

        Debug.Log("[SetupTitleUI] 背景作成完了");
    }

    static void CreateTitle(Transform parent)
    {
        // タイトルコンテナ
        GameObject titleContainer = new GameObject("TitleContainer");
        titleContainer.transform.SetParent(parent, false);

        RectTransform containerRect = titleContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, 80); // 中央より少し上
        containerRect.sizeDelta = new Vector2(600, 100);

        // "さばけ！" 部分
        GameObject sabakeObj = new GameObject("TitleText_Sabake");
        sabakeObj.transform.SetParent(titleContainer.transform, false);

        RectTransform sabakeRect = sabakeObj.AddComponent<RectTransform>();
        sabakeRect.anchorMin = new Vector2(0, 0.5f);
        sabakeRect.anchorMax = new Vector2(0.5f, 0.5f);
        sabakeRect.pivot = new Vector2(1, 0.5f);
        sabakeRect.anchoredPosition = new Vector2(0, 0);
        sabakeRect.sizeDelta = new Vector2(250, 80);

        TextMeshProUGUI sabakeTMP = sabakeObj.AddComponent<TextMeshProUGUI>();
        sabakeTMP.text = "さばけ！";
        sabakeTMP.fontSize = 56;
        sabakeTMP.fontStyle = FontStyles.Bold;
        sabakeTMP.alignment = TextAlignmentOptions.Right;
        sabakeTMP.color = COLOR_BLACK;

        // "おさかな" 部分
        GameObject osakanaObj = new GameObject("TitleText_Osakana");
        osakanaObj.transform.SetParent(titleContainer.transform, false);

        RectTransform osakanaRect = osakanaObj.AddComponent<RectTransform>();
        osakanaRect.anchorMin = new Vector2(0.5f, 0.5f);
        osakanaRect.anchorMax = new Vector2(1, 0.5f);
        osakanaRect.pivot = new Vector2(0, 0.5f);
        osakanaRect.anchoredPosition = new Vector2(10, 0);
        osakanaRect.sizeDelta = new Vector2(250, 80);

        TextMeshProUGUI osakanaTMP = osakanaObj.AddComponent<TextMeshProUGUI>();
        osakanaTMP.text = "おさかな";
        osakanaTMP.fontSize = 56;
        osakanaTMP.fontStyle = FontStyles.Bold;
        osakanaTMP.alignment = TextAlignmentOptions.Left;
        osakanaTMP.color = COLOR_BLACK;

        // 統合版タイトル（シンプル版）
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(parent, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 100);
        titleRect.sizeDelta = new Vector2(600, 100);

        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "さばけ！おさかな";
        titleTMP.fontSize = 64;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = COLOR_BLACK;

        // コンテナ版は削除（シンプル版を使用）
        DestroyImmediate(titleContainer);

        Debug.Log("[SetupTitleUI] タイトル作成完了");
    }

    static void CreateStartButton(Transform parent)
    {
        // ボタンコンテナ
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -50); // 中央より少し下
        rect.sizeDelta = new Vector2(200, 70);

        // 背景Image（角丸風）
        Image img = btnObj.AddComponent<Image>();
        img.color = COLOR_BUTTON_BG;

        // Button
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        // ボタンの色設定
        ColorBlock colors = btn.colors;
        colors.normalColor = COLOR_BUTTON_BG;
        colors.highlightedColor = new Color(1f, 0.95f, 0.85f);
        colors.pressedColor = new Color(0.9f, 0.85f, 0.75f);
        btn.colors = colors;

        // 枠線効果
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = COLOR_BUTTON_BORDER;
        outline.effectDistance = new Vector2(3, 3);

        // ボタンテキスト
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "さばく！";
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = COLOR_BLACK;

        Debug.Log("[SetupTitleUI] スタートボタン作成完了");
    }
}
