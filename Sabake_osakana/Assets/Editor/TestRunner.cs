using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

/// <summary>
/// バッチモード実行用エントリーポイント
/// Usage: Unity.exe -batchmode -projectPath "D:\Unity\Sabake_osakana" -executeMethod TestRunner.BuildAndRun
/// </summary>
public static class TestRunner
{
    /// <summary>
    /// ビルドして実行
    /// </summary>
    public static void BuildAndRun()
    {
        Debug.Log("[TestRunner] Building standalone player...");

        string buildPath = "Builds/SimulationBuild/SabakeOsakana.exe";

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[TestRunner] Build succeeded: {buildPath}");
            Debug.Log("[TestRunner] Run the executable to start simulation");
        }
        else
        {
            Debug.LogError($"[TestRunner] Build failed: {report.summary.result}");
        }

        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    /// <summary>
    /// シミュレーション実行のメインエントリーポイント（Editor Play Mode用）
    /// </summary>
    public static void RunSimulation()
    {
        Debug.Log("[TestRunner] Starting simulation...");

        // シミュレーション設定
        int maxDuration = GetArgInt("-duration", 120); // デフォルト120秒
        float aiReactionSpeed = GetArgFloat("-aiSpeed", 0.8f);

        Debug.Log($"[TestRunner] Config: duration={maxDuration}s, aiSpeed={aiReactionSpeed}");

        // PlayerSettingsを調整（ヘッドレス実行用）
        PlayerSettings.runInBackground = true;

        // シーンをロードしてシミュレーション開始
        EditorApplication.EnterPlaymode();

        // プレイモード開始後のコールバック設定
        EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SetupSimulation(aiReactionSpeed, maxDuration);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("[TestRunner] Simulation ended, exiting...");
                EditorApplication.Exit(0);
            }
        };
    }

    static void SetupSimulation(float aiSpeed, int maxDuration)
    {
        Debug.Log("[TestRunner] Setting up simulation objects...");

        // SimulationLoggerを作成
        if (Object.FindFirstObjectByType<SimulationLogger>() == null)
        {
            var loggerObj = new GameObject("SimulationLogger");
            loggerObj.AddComponent<SimulationLogger>();
            Debug.Log("[TestRunner] SimulationLogger created");
        }

        // AutoPlayerを作成
        var autoPlayer = Object.FindFirstObjectByType<AutoPlayer>();
        if (autoPlayer == null)
        {
            var playerObj = new GameObject("AutoPlayer");
            autoPlayer = playerObj.AddComponent<AutoPlayer>();
            Debug.Log("[TestRunner] AutoPlayer created");
        }

        autoPlayer.enableAutoPlay = true;
        autoPlayer.reactionSpeed = aiSpeed;
        autoPlayer.showDebugInfo = false;

        // タイムアウト設定
        var timeoutObj = new GameObject("SimulationTimeout");
        var timeout = timeoutObj.AddComponent<SimulationTimeout>();
        timeout.maxDuration = maxDuration;
    }

    static int GetArgInt(string argName, int defaultValue)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName && int.TryParse(args[i + 1], out int value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    static float GetArgFloat(string argName, float defaultValue)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName && float.TryParse(args[i + 1], out float value))
            {
                return value;
            }
        }
        return defaultValue;
    }
}

/// <summary>
/// シミュレーションのタイムアウト管理
/// </summary>
public class SimulationTimeout : MonoBehaviour
{
    public int maxDuration = 120;
    private float startTime;

    void Start()
    {
        startTime = Time.realtimeSinceStartup;
        Debug.Log($"[SimulationTimeout] Started, will timeout in {maxDuration}s");
    }

    void Update()
    {
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed >= maxDuration)
        {
            Debug.Log($"[SimulationTimeout] Timeout reached ({maxDuration}s), ending simulation...");

            // ログを保存
            if (SimulationLogger.Instance != null)
            {
                SimulationLogger.Instance.SaveLog();
            }

            // 少し待ってから終了
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
