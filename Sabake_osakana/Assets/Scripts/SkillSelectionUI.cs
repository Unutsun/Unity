using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// スキル選択UI
/// 3つのスキルからランダムに1つ選ぶローグライト風選択画面
/// </summary>
public class SkillSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Transform buttonContainer;
    public GameObject skillButtonPrefab;

    [Header("Style")]
    public Color buttonColor = new Color(0.2f, 0.2f, 0.3f, 0.95f);
    public Color hoverColor = new Color(0.3f, 0.3f, 0.5f, 1f);

    private SkillData[] currentChoices;
    private Button[] skillButtons;

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        CreateUI();
    }

    void OnEnable()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillSelectionRequired += ShowSkillSelection;
        }
    }

    void OnDisable()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillSelectionRequired -= ShowSkillSelection;
        }
    }

    void CreateUI()
    {
        // パネルがなければ作成
        if (panel == null)
        {
            // Canvas取得または作成
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("SkillCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // パネル作成
            panel = new GameObject("SkillSelectionPanel");
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 背景
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);

            // タイトル
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.sizeDelta = new Vector2(600, 80);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "スキル獲得！";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.yellow;

            // ボタンコンテナ
            GameObject container = new GameObject("ButtonContainer");
            container.transform.SetParent(panel.transform, false);
            buttonContainer = container.transform;
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, 0.2f);
            containerRect.anchorMax = new Vector2(0.9f, 0.75f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(20, 20, 20, 20);

            panel.SetActive(false);
        }
    }

    void ShowSkillSelection(SkillData[] choices)
    {
        currentChoices = choices;

        // UIが破棄されていたら再作成
        if (panel == null || buttonContainer == null)
        {
            Debug.Log("[SkillSelectionUI] UI was destroyed, recreating...");
            CreateUI();
        }

        // それでもnullなら作成失敗
        if (buttonContainer == null)
        {
            Debug.LogError("[SkillSelectionUI] Failed to create UI");
            return;
        }

        // 既存のボタンを削除（安全なイテレーション）
        var childrenToDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in buttonContainer)
        {
            if (child != null)
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }
        foreach (var child in childrenToDestroy)
        {
            Destroy(child);
        }

        skillButtons = new Button[choices.Length];

        // 各スキルのボタンを作成
        for (int i = 0; i < choices.Length; i++)
        {
            CreateSkillButton(choices[i], i);
        }

        panel.SetActive(true);
        Debug.Log($"[SkillSelectionUI] Showing {choices.Length} skill choices");
    }

    void CreateSkillButton(SkillData skill, int index)
    {
        // ボタンオブジェクト
        GameObject btnObj = new GameObject($"SkillButton_{index}");
        btnObj.transform.SetParent(buttonContainer, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(250, 300);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = buttonColor;

        Button btn = btnObj.AddComponent<Button>();
        skillButtons[index] = btn;

        // ホバー効果
        ColorBlock colors = btn.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = new Color(0.4f, 0.4f, 0.6f, 1f);
        btn.colors = colors;

        // アイコン
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.65f);
        iconRect.anchorMax = new Vector2(0.5f, 0.95f);
        iconRect.sizeDelta = new Vector2(80, 80);
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = skill.icon;
        iconText.fontSize = 48;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = Color.white;

        // スキル名
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(btnObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.4f);
        nameRect.anchorMax = new Vector2(1f, 0.6f);
        nameRect.offsetMin = new Vector2(10, 0);
        nameRect.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = skill.nameJP;
        nameText.fontSize = 28;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.fontStyle = FontStyles.Bold;

        // 説明
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(btnObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 0.1f);
        descRect.anchorMax = new Vector2(1f, 0.35f);
        descRect.offsetMin = new Vector2(10, 0);
        descRect.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = skill.description;
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = new Color(0.8f, 0.8f, 0.8f);

        // クリックイベント
        int skillIndex = index;  // クロージャ用にコピー
        btn.onClick.AddListener(() => OnSkillSelected(skillIndex));
    }

    void OnSkillSelected(int index)
    {
        if (currentChoices == null || index >= currentChoices.Length) return;

        SkillData selected = currentChoices[index];
        Debug.Log($"[SkillSelectionUI] Selected: {selected.nameJP}");

        // パネルを閉じる
        panel.SetActive(false);

        // SkillManagerに通知
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.SelectSkill(selected.type);
        }
    }
}
