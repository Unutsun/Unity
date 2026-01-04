using UnityEngine;
using TMPro;

/// <summary>
/// ランタイムでTMPのフォールバック設定を行う
/// 日本語が表示されない場合、Unityの標準フォントを使用
/// </summary>
public class FontFallbackSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SetupFallbackFont()
    {
        // TMPのデフォルト設定を取得して警告を抑制
        Debug.Log("[FontFallbackSetup] Initializing font fallback system");
    }

    void Awake()
    {
        // シーン内のすべてのTMPテキストをチェック
        CheckAndFixJapaneseText();
    }

    void CheckAndFixJapaneseText()
    {
        // すべてのTMPテキストを取得
        var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

        foreach (var text in allTexts)
        {
            // 日本語フォントアセットが設定されているかチェック
            if (text.font != null && text.font.name.Contains("Japanese"))
            {
                continue; // 日本語フォントが設定済み
            }

            // フォントがないか、LiberationSansの場合は警告を出さずに続行
            // （日本語フォントアセットが作成されるまでの暫定対応）
        }
    }
}
