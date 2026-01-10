using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// タイトル画面の管理（CSV対応版）
/// TextDataManagerからテキストを取得
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public Button startButton;
    public TextMeshProUGUI startButtonText;

    private TextDataManager textManager => TextDataManager.Instance;

    void Awake()
    {
        // TextDataManagerが存在しなければ作成
        if (TextDataManager.Instance == null)
        {
            GameObject textMgrObj = new GameObject("TextDataManager");
            textMgrObj.AddComponent<TextDataManager>();
        }

        // StageManagerが存在しなければ作成
        if (StageManager.Instance == null)
        {
            GameObject stageMgrObj = new GameObject("StageManager");
            stageMgrObj.AddComponent<StageManager>();
        }
        else
        {
            // タイトルに戻ってきたらステージをリセット
            StageManager.Instance.ResetToFirstStage();
        }
    }

    void Start()
    {
        Debug.Log($"[TitleManager] Start: Initializing...");

        AutoSetupUIReferences();
        SetupTexts();
        SetupButton();
    }

    void AutoSetupUIReferences()
    {
        // TitleCanvasを検索
        GameObject canvas = GameObject.Find("TitleCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("[TitleManager] TitleCanvas not found");
            return;
        }

        // タイトルテキスト
        if (titleText == null)
            titleText = FindUIElement<TextMeshProUGUI>(canvas, "TitleText");

        // スタートボタン
        if (startButton == null)
            startButton = FindUIElement<Button>(canvas, "StartButton");

        // ボタンテキスト
        if (startButton != null && startButtonText == null)
            startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    T FindUIElement<T>(GameObject parent, string name) where T : Component
    {
        Transform found = parent.transform.Find(name);
        if (found != null)
        {
            T comp = found.GetComponent<T>();
            if (comp != null) return comp;
        }

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

    void SetupTexts()
    {
        if (textManager == null)
        {
            Debug.LogWarning("[TitleManager] TextDataManager not available yet");
            return;
        }

        if (titleText != null)
            titleText.text = textManager.GetText("title", "default", "title_main", "さばけ！おさかな");

        if (startButtonText != null)
            startButtonText.text = textManager.GetText("title", "default", "button_start", "さばく！");
    }

    void SetupButton()
    {
        if (startButton == null)
        {
            // 自動検索
            startButton = GetComponentInChildren<Button>();
            if (startButton == null)
            {
                GameObject btnObj = GameObject.Find("StartButton");
                if (btnObj != null)
                    startButton = btnObj.GetComponent<Button>();
            }
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
            Debug.Log($"[TitleManager] Button listener added");
        }
        else
        {
            Debug.LogError("[TitleManager] No start button found!");
        }
    }

    void OnStartButtonClicked()
    {
        Debug.Log("[TitleManager] Start button clicked! Loading SampleScene...");
        SceneManager.LoadScene("SampleScene");
    }

    void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
    }
}
