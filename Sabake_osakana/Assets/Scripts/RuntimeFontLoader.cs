using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// ランタイムで日本語フォントを読み込んでTMPテキストに適用する
/// エディタでフォントアセットが作成されていない場合のフォールバック
/// </summary>
public class RuntimeFontLoader : MonoBehaviour
{
    private static RuntimeFontLoader instance;
    private TMP_FontAsset japaneseFontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // 永続的なGameObjectを作成
        GameObject go = new GameObject("RuntimeFontLoader");
        instance = go.AddComponent<RuntimeFontLoader>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        StartCoroutine(SetupFonts());
    }

    IEnumerator SetupFonts()
    {
        // フレームを待ってからフォント設定
        yield return null;

        // 日本語フォントアセットを検索
        japaneseFontAsset = Resources.Load<TMP_FontAsset>("Fonts/Meiryo-Japanese SDF");

        if (japaneseFontAsset == null)
        {
            // Assets/Fonts フォルダから検索
            japaneseFontAsset = Resources.Load<TMP_FontAsset>("Meiryo-Japanese SDF");
        }

        if (japaneseFontAsset != null)
        {
            Debug.Log("[RuntimeFontLoader] 日本語フォント発見: " + japaneseFontAsset.name);
            ApplyToAllTexts();
        }
        else
        {
            Debug.LogWarning("[RuntimeFontLoader] 日本語フォントアセットが見つかりません。" +
                           "Unityエディタで Tools > Setup Japanese Font (Full Process) を実行してください。");
        }
    }

    void ApplyToAllTexts()
    {
        // シーン内のすべてのTMPテキストに適用
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var text in allTexts)
        {
            if (text.font == null || !text.font.name.Contains("Japanese"))
            {
                text.font = japaneseFontAsset;
            }
        }

        Debug.Log($"[RuntimeFontLoader] {allTexts.Length}個のテキストに日本語フォントを適用");
    }

    // シーンロード時にも適用
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (japaneseFontAsset != null)
        {
            StartCoroutine(DelayedApply());
        }
    }

    IEnumerator DelayedApply()
    {
        yield return null;
        ApplyToAllTexts();
    }
}
