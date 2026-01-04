using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CSVからテキストデータを読み込み・管理するシングルトン
/// 疎結合設計: UIコンポーネントはこのクラス経由でテキストを取得
/// </summary>
public class TextDataManager : MonoBehaviour
{
    public static TextDataManager Instance { get; private set; }

    // キー: "scene:state:key" -> テキスト
    private Dictionary<string, string> textData = new Dictionary<string, string>();

    // CSVファイル名（Resourcesフォルダ内）
    private const string CSV_FILE = "TextData";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTextData();
            Debug.Log($"[TextDataManager] Initialized with {textData.Count} entries");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSVファイルを読み込んでDictionaryに格納
    /// </summary>
    void LoadTextData()
    {
        textData.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>(CSV_FILE);
        if (csvFile == null)
        {
            Debug.LogError($"[TextDataManager] CSV file not found: Resources/{CSV_FILE}.csv");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // ヘッダースキップ
        {
            string line = lines[i].Trim();

            // コメント行・空行スキップ
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            // CSV解析（カンマ区切り、最後のフィールドにカンマを含む可能性を考慮）
            string[] parts = ParseCSVLine(line);

            if (parts.Length >= 4)
            {
                string scene = parts[0].Trim();
                string state = parts[1].Trim();
                string key = parts[2].Trim();
                string text = parts[3].Trim();

                string fullKey = $"{scene}:{state}:{key}";
                textData[fullKey] = text;
            }
        }

        Debug.Log($"[TextDataManager] Loaded {textData.Count} text entries");
    }

    /// <summary>
    /// CSV行をパース（カンマ区切り、4フィールド以上対応）
    /// </summary>
    string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        int fieldCount = 0;
        int start = 0;

        for (int i = 0; i < line.Length && fieldCount < 3; i++)
        {
            if (line[i] == ',')
            {
                result.Add(line.Substring(start, i - start));
                start = i + 1;
                fieldCount++;
            }
        }

        // 残りは全てテキストフィールド
        if (start < line.Length)
        {
            result.Add(line.Substring(start));
        }

        return result.ToArray();
    }

    /// <summary>
    /// テキストを取得
    /// </summary>
    /// <param name="scene">シーン名 (title, game)</param>
    /// <param name="state">状態 (hud, clear, gameover, pause, rank, flavor)</param>
    /// <param name="key">キー名</param>
    /// <param name="defaultText">見つからない場合のデフォルト</param>
    public string GetText(string scene, string state, string key, string defaultText = "???")
    {
        string fullKey = $"{scene}:{state}:{key}";

        if (textData.TryGetValue(fullKey, out string text))
        {
            return text;
        }

        Debug.LogWarning($"[TextDataManager] Text not found: {fullKey}");
        return defaultText;
    }

    /// <summary>
    /// フォーマット付きテキストを取得
    /// </summary>
    public string GetFormattedText(string scene, string state, string key, params object[] args)
    {
        string format = GetText(scene, state, key);
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }

    /// <summary>
    /// ランク名を取得
    /// </summary>
    public string GetRankName(string rankKey)
    {
        return GetText("game", "rank", rankKey, rankKey);
    }

    /// <summary>
    /// ランクのフレーバーテキストを取得
    /// </summary>
    public string GetRankFlavor(string rankKey)
    {
        return GetText("game", "flavor", rankKey, "");
    }

    /// <summary>
    /// クリア率からランクキーを取得（旧：互換性用）
    /// </summary>
    public string GetRankKey(float clearPercentage)
    {
        return GetRankKeyByKirimi((int)clearPercentage); // 暫定で同じ処理
    }

    /// <summary>
    /// きりみ数からランクキーを取得
    /// 100以上: 舩盛り, 80以上: おさしみ, 50以上: さく切り, 20以上: あら汁, それ以下: エサ
    /// </summary>
    public string GetRankKeyByKirimi(int kirimi)
    {
        if (kirimi >= 100) return "funamori";
        if (kirimi >= 80) return "osashimi";
        if (kirimi >= 50) return "sakugiri";
        if (kirimi >= 20) return "arajiru";
        return "esa";
    }

    /// <summary>
    /// CSVをリロード（エディタ用）
    /// </summary>
    public void ReloadData()
    {
        LoadTextData();
    }
}
