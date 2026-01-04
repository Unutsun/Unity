using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// SampleSceneのUI構造を仕様書通りに再構築するエディタスクリプト
/// </summary>
public class SceneCleanup : Editor
{
    [MenuItem("Tools/Cleanup SampleScene UI")]
    public static void CleanupUI()
    {
        Debug.Log("[SceneCleanup] Starting UI cleanup...");

        // 1. 不要なGameCanvasとUIManagerを削除
        DeleteDuplicateObjects();

        // 2. メインのGameCanvasを取得
        GameObject mainCanvas = GetOrCreateMainCanvas();
        if (mainCanvas == null)
        {
            Debug.LogError("[SceneCleanup] Failed to get or create main canvas!");
            return;
        }

        // 3. 不要な子要素を削除
        CleanupCanvasChildren(mainCanvas);

        // 4. 正しいUI構造を作成
        SetupCorrectUIStructure(mainCanvas);

        // 5. パネルを非アクティブに設定
        SetPanelsInactive(mainCanvas);

        Debug.Log("[SceneCleanup] UI cleanup complete!");
        EditorUtility.SetDirty(mainCanvas);
    }

    static void DeleteDuplicateObjects()
    {
        // 重複したGameCanvasを削除（UIManagerがないもの）
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        List<GameObject> toDelete = new List<GameObject>();

        foreach (var canvas in canvases)
        {
            if (canvas.gameObject.name == "GameCanvas")
            {
                var uiManager = canvas.GetComponent<UIManager>();
                if (uiManager == null)
                {
                    toDelete.Add(canvas.gameObject);
                    Debug.Log($"[SceneCleanup] Marking duplicate GameCanvas for deletion: {canvas.gameObject.GetInstanceID()}");
                }
            }
        }

        // 独立したUIManagerオブジェクトを削除
        var uiManagers = Object.FindObjectsByType<UIManager>(FindObjectsSortMode.None);
        foreach (var uim in uiManagers)
        {
            if (uim.gameObject.name == "UIManager" && uim.GetComponent<Canvas>() == null)
            {
                toDelete.Add(uim.gameObject);
                Debug.Log("[SceneCleanup] Marking standalone UIManager for deletion");
            }
        }

        foreach (var obj in toDelete)
        {
            DestroyImmediate(obj);
        }
    }

    static GameObject GetOrCreateMainCanvas()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.gameObject.name == "GameCanvas" && canvas.GetComponent<UIManager>() != null)
            {
                return canvas.gameObject;
            }
        }
        return null;
    }

    static void CleanupCanvasChildren(GameObject canvas)
    {
        // 残すべき要素の名前
        HashSet<string> keepNames = new HashSet<string>
        {
            "LivesText", "TimerText", "KirimiText", "PauseButton", "ReadyMessageText",
            "ResultPanel", "GameOverPanel", "PausePanel"
        };

        List<GameObject> toDelete = new List<GameObject>();

        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);
            if (!keepNames.Contains(child.name))
            {
                toDelete.Add(child.gameObject);
                Debug.Log($"[SceneCleanup] Marking for deletion: {child.name}");
            }
        }

        foreach (var obj in toDelete)
        {
            DestroyImmediate(obj);
        }

        // 壊れたLivesText（Transformのみ）を削除
        var livesTexts = canvas.GetComponentsInChildren<Transform>(true);
        foreach (var t in livesTexts)
        {
            if (t.name == "LivesText" && t.GetComponent<RectTransform>() == null)
            {
                DestroyImmediate(t.gameObject);
                Debug.Log("[SceneCleanup] Deleted broken LivesText");
            }
        }
    }

    static void SetupCorrectUIStructure(GameObject canvas)
    {
        // HUD要素の設定
        SetupHUDElement(canvas, "LivesText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(200, 50), "♥♥♥♥♥", 36, TextAlignmentOptions.Left);
        SetupHUDElement(canvas, "TimerText", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(200, 50), "01:30", 48, TextAlignmentOptions.Center);
        SetupHUDElement(canvas, "KirimiText", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(250, 50), "きりみ: 0", 36, TextAlignmentOptions.Right);
        SetupHUDElement(canvas, "ReadyMessageText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(400, 60), "スペースキーで発射！", 36, TextAlignmentOptions.Center);

        // PauseButton
        SetupButton(canvas.transform, "PauseButton", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(50, 50), "||");

        // ResultPanel
        SetupPanel(canvas, "ResultPanel", new string[] { "ResultTitleText", "ResultKirimiText", "ResultRankText", "ResultFlavorText" },
            new string[] { "おさかな さばけた！", "さばいたきりみ: 0", "おさしみ級", "見事なさばきっぷり！" },
            new string[] { "ContinueButton", "EndButton" },
            new string[] { "つづける", "おわる" });

        // GameOverPanel
        SetupPanel(canvas, "GameOverPanel", new string[] { "GameOverTitleText", "GameOverFlavorText" },
            new string[] { "No sabaki...", "残念..." },
            new string[] { "RetryButton", "TitleButton" },
            new string[] { "リトライ", "タイトルへ" });

        // PausePanel
        SetupPanel(canvas, "PausePanel", new string[] { "PauseTitleText" },
            new string[] { "ポーズ" },
            new string[] { "ResumeButton", "PauseTitleButton" },
            new string[] { "つづける", "タイトルへ" });
    }

    static void SetupHUDElement(GameObject canvas, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, string text, int fontSize, TextAlignmentOptions alignment)
    {
        Transform t = canvas.transform.Find(name);
        GameObject obj;
        if (t != null)
        {
            obj = t.gameObject;
        }
        else
        {
            obj = new GameObject(name);
            obj.transform.SetParent(canvas.transform, false);
            obj.AddComponent<CanvasRenderer>();
            obj.AddComponent<TextMeshProUGUI>();
        }

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null) rt = obj.AddComponent<RectTransform>();

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        obj.layer = LayerMask.NameToLayer("UI");
    }

    static void SetupButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, string text)
    {
        Transform t = parent.Find(name);
        GameObject obj;

        if (t != null && t.GetComponent<Button>() != null)
        {
            obj = t.gameObject;
        }
        else
        {
            if (t != null) DestroyImmediate(t.gameObject);

            obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<CanvasRenderer>();
            var img = obj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            obj.AddComponent<Button>();

            // ボタンテキスト
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            textObj.AddComponent<CanvasRenderer>();
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            textObj.layer = LayerMask.NameToLayer("UI");
        }

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        obj.layer = LayerMask.NameToLayer("UI");
    }

    static void SetupPanel(GameObject canvas, string panelName, string[] textNames, string[] textValues, string[] buttonNames, string[] buttonTexts)
    {
        Transform panelT = canvas.transform.Find(panelName);
        GameObject panel;

        if (panelT != null)
        {
            panel = panelT.gameObject;
        }
        else
        {
            panel = new GameObject(panelName);
            panel.transform.SetParent(canvas.transform, false);
            panel.AddComponent<CanvasRenderer>();
            var img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.85f);
        }

        RectTransform panelRt = panel.GetComponent<RectTransform>();
        if (panelRt == null) panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        panel.layer = LayerMask.NameToLayer("UI");

        // パネル内の既存子要素を削除して再構築
        List<GameObject> toDelete = new List<GameObject>();
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            toDelete.Add(panel.transform.GetChild(i).gameObject);
        }
        foreach (var obj in toDelete)
        {
            DestroyImmediate(obj);
        }

        // テキスト要素を作成
        float yOffset = 100f;
        for (int i = 0; i < textNames.Length; i++)
        {
            var textObj = new GameObject(textNames[i]);
            textObj.transform.SetParent(panel.transform, false);
            textObj.AddComponent<CanvasRenderer>();
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = textValues[i];
            tmp.fontSize = i == 0 ? 48 : 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, yOffset);
            rt.sizeDelta = new Vector2(600, 60);

            textObj.layer = LayerMask.NameToLayer("UI");
            yOffset -= 60f;
        }

        // ボタン要素を作成
        float buttonY = -80f;
        float buttonSpacing = 70f;
        for (int i = 0; i < buttonNames.Length; i++)
        {
            SetupButton(panel.transform, buttonNames[i],
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, buttonY - i * buttonSpacing),
                new Vector2(200, 50), buttonTexts[i]);
        }
    }

    static void SetPanelsInactive(GameObject canvas)
    {
        string[] panelNames = { "ResultPanel", "GameOverPanel", "PausePanel" };
        foreach (string name in panelNames)
        {
            Transform t = canvas.transform.Find(name);
            if (t != null)
            {
                t.gameObject.SetActive(false);
                Debug.Log($"[SceneCleanup] Set {name} to inactive");
            }
        }
    }
}
