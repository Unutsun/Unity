using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// マリオカート風カウントダウン演出
/// 3（黄）→ 2（オレンジ）→ 1（赤）→ さばけ！（赤）
/// 全て太めの白フチ付き
/// </summary>
public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance { get; private set; }

    [Header("Countdown Settings")]
    public float countInterval = 1.0f;      // 各数字の表示間隔
    public float goDisplayTime = 0.8f;      // 「さばけ！」の表示時間
    public float scaleAnimDuration = 0.2f;  // スケールアニメーション時間

    [Header("Colors")]
    public Color color3 = new Color(1f, 0.9f, 0.2f);      // 黄色
    public Color color2 = new Color(1f, 0.5f, 0f);        // オレンジ
    public Color color1 = new Color(1f, 0.2f, 0.2f);      // 赤
    public Color colorGo = new Color(1f, 0.2f, 0.2f);     // さばけ！も赤
    public Color outlineColor = Color.white;              // 白フチ

    [Header("UI References")]
    public TextMeshProUGUI countdownText;
    public Outline textOutline;

    private bool isCountingDown = false;

    void Awake()
    {
        Debug.Log("[CountdownManager] Awake: Initializing");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[CountdownManager] Awake: Singleton set");
        }
        else
        {
            Debug.LogWarning("[CountdownManager] Awake: Duplicate instance, destroying");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // カウントダウンUIを自動作成（なければ）
        if (countdownText == null)
        {
            CreateCountdownUI();
        }

        // 初期状態は非表示
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    void CreateCountdownUI()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("[CountdownManager] GameCanvas not found");
            return;
        }

        // カウントダウン用のGameObject作成
        GameObject countdownObj = new GameObject("CountdownText");
        countdownObj.transform.SetParent(canvas.transform, false);

        // RectTransform - 画面中央
        RectTransform rect = countdownObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400, 200);

        // TextMeshProUGUI
        countdownText = countdownObj.AddComponent<TextMeshProUGUI>();
        countdownText.text = "";
        countdownText.fontSize = 150;
        countdownText.fontStyle = FontStyles.Bold;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

        // Outline（白フチ）- TMPの場合はMaterialでアウトラインを設定
        // UI.Outlineも追加（フォールバック用）
        textOutline = countdownObj.AddComponent<Outline>();
        textOutline.effectColor = outlineColor;
        textOutline.effectDistance = new Vector2(4, -4);

        // 2つ目のOutlineで太くする
        Outline outline2 = countdownObj.AddComponent<Outline>();
        outline2.effectColor = outlineColor;
        outline2.effectDistance = new Vector2(-4, 4);

        // 3つ目のOutline
        Outline outline3 = countdownObj.AddComponent<Outline>();
        outline3.effectColor = outlineColor;
        outline3.effectDistance = new Vector2(4, 4);

        // 4つ目のOutline
        Outline outline4 = countdownObj.AddComponent<Outline>();
        outline4.effectColor = outlineColor;
        outline4.effectDistance = new Vector2(-4, -4);

        Debug.Log("[CountdownManager] Countdown UI created");
    }

    /// <summary>
    /// カウントダウンを開始する
    /// </summary>
    public void StartCountdown(System.Action onComplete = null)
    {
        if (isCountingDown) return;

        Debug.Log("[CountdownManager] Starting countdown");
        StartCoroutine(CountdownCoroutine(onComplete));
    }

    IEnumerator CountdownCoroutine(System.Action onComplete)
    {
        isCountingDown = true;

        if (countdownText == null)
        {
            Debug.LogError("[CountdownManager] countdownText is null!");
            isCountingDown = false;
            onComplete?.Invoke();
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        // 3
        yield return StartCoroutine(ShowCountdownNumber("3", color3));

        // 2
        yield return StartCoroutine(ShowCountdownNumber("2", color2));

        // 1
        yield return StartCoroutine(ShowCountdownNumber("1", color1));

        // さばけ！
        yield return StartCoroutine(ShowCountdownNumber("さばけ！", colorGo, true));

        // 非表示
        countdownText.gameObject.SetActive(false);

        isCountingDown = false;

        Debug.Log("[CountdownManager] Countdown complete");
        onComplete?.Invoke();
    }

    IEnumerator ShowCountdownNumber(string text, Color color, bool isGo = false)
    {
        countdownText.text = text;
        countdownText.color = color;

        // スケールアニメーション（大→通常）
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 1.5f;
        Vector3 endScale = Vector3.one;

        while (elapsed < scaleAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scaleAnimDuration;
            // イーズアウト
            t = 1f - (1f - t) * (1f - t);
            countdownText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        countdownText.transform.localScale = endScale;

        // 表示時間待機
        float waitTime = isGo ? goDisplayTime : countInterval - scaleAnimDuration;
        yield return new WaitForSecondsRealtime(waitTime);
    }

    /// <summary>
    /// カウントダウン中かどうか
    /// </summary>
    public bool IsCountingDown => isCountingDown;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
