using UnityEngine;
using UnityEditor;

/// <summary>
/// シミュレーション関連のエディタメニュー
/// </summary>
public static class SimulationMenu
{
    private const string AUTO_PLAY_KEY = "Sabake_AutoPlay_Enabled";

    [MenuItem("Sabake/AIで自動プレイ ON", false, 100)]
    public static void EnableAutoPlay()
    {
        EditorPrefs.SetBool(AUTO_PLAY_KEY, true);
        Debug.Log("[SimulationMenu] 自動プレイを有効化しました。次のプレイからAIが操作します。");
    }

    [MenuItem("Sabake/AIで自動プレイ OFF", false, 101)]
    public static void DisableAutoPlay()
    {
        EditorPrefs.SetBool(AUTO_PLAY_KEY, false);
        Debug.Log("[SimulationMenu] 自動プレイを無効化しました。手動操作に戻ります。");
    }

    [MenuItem("Sabake/ログフォルダを開く", false, 200)]
    public static void OpenLogsFolder()
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "..", "Logs");
        System.IO.Directory.CreateDirectory(logDir);
        EditorUtility.RevealInFinder(logDir);
    }

    [MenuItem("Sabake/最新ログを表示", false, 201)]
    public static void ShowLatestLog()
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "..", "Logs");
        if (!System.IO.Directory.Exists(logDir))
        {
            Debug.Log("[SimulationMenu] ログフォルダがありません。プレイ後に生成されます。");
            return;
        }

        var files = System.IO.Directory.GetFiles(logDir, "simulation_*.json");
        if (files.Length == 0)
        {
            Debug.Log("[SimulationMenu] シミュレーションログがありません。プレイ後に生成されます。");
            return;
        }

        // 最新のファイルを取得
        System.Array.Sort(files);
        string latestFile = files[files.Length - 1];

        string content = System.IO.File.ReadAllText(latestFile);
        Debug.Log($"[SimulationMenu] 最新ログ: {latestFile}\n{content}");
    }

    /// <summary>
    /// プレイモード開始時に自動プレイ設定を適用
    /// </summary>
    [InitializeOnLoadMethod]
    private static void SetupPlayModeCallback()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            bool autoPlayEnabled = EditorPrefs.GetBool(AUTO_PLAY_KEY, false);

            if (autoPlayEnabled)
            {
                // AutoPlayerを探して有効化
                var autoPlayer = Object.FindFirstObjectByType<AutoPlayer>();
                if (autoPlayer != null)
                {
                    autoPlayer.enableAutoPlay = true;
                    autoPlayer.showDebugInfo = true;
                    Debug.Log("[SimulationMenu] AutoPlayer有効化完了");
                }
            }
        }
    }
}
