using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// シミュレーションログをファイルに出力
/// バッチモード実行時に全イベントを記録
/// </summary>
public class SimulationLogger : MonoBehaviour
{
    public static SimulationLogger Instance { get; private set; }

    private StringBuilder logBuffer;
    private string logFilePath;
    private float sessionStartTime;
    private int frameCount = 0;
    private List<string> eventLog = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLogger();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeLogger()
    {
        sessionStartTime = Time.realtimeSinceStartup;
        logBuffer = new StringBuilder();

        // ログディレクトリを作成
        string logDir = Path.Combine(Application.dataPath, "..", "Logs");
        Directory.CreateDirectory(logDir);

        // ログファイル名（タイムスタンプ付き）
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logFilePath = Path.Combine(logDir, $"simulation_{timestamp}.json");

        Log("SESSION_START", new {
            unityVersion = Application.unityVersion,
            platform = Application.platform.ToString(),
            isBatchMode = Application.isBatchMode,
            timestamp = System.DateTime.Now.ToString("o")
        });

        Debug.Log($"[SimulationLogger] Initialized: {logFilePath}");
    }

    void OnEnable()
    {
        // ゲームイベントを購読
        GameEvents.OnBallLaunched += () => Log("BALL_LAUNCHED", null);
        GameEvents.OnBallLost += () => Log("BALL_LOST", new { lives = GameState.Instance?.Lives ?? 0 });
        GameEvents.OnBallReset += () => Log("BALL_RESET", null);
        GameEvents.OnBrickDestroyed += (points) => Log("BRICK_DESTROYED", new { points, totalKirimi = GameState.Instance?.Kirimi ?? 0 });
        GameEvents.OnGameClear += () => Log("GAME_CLEAR", GetGameSummary());
        GameEvents.OnGameOver += () => Log("GAME_OVER", GetGameSummary());
        GameEvents.OnKirimiChanged += (kirimi) => Log("KIRIMI_CHANGED", new { kirimi });
        GameEvents.OnLivesChanged += (lives) => Log("LIVES_CHANGED", new { lives });
    }

    void Update()
    {
        frameCount++;

        // 30フレームごとにステータスログ
        if (frameCount % 30 == 0 && GameState.Instance != null)
        {
            var state = GameState.Instance.CurrentState;
            if (state == GameStateType.Playing)
            {
                LogStatus();
            }
        }
    }

    void LogStatus()
    {
        var ball = FindFirstObjectByType<BallController>();
        var paddle = FindFirstObjectByType<PaddleController>();

        Log("STATUS", new {
            frame = frameCount,
            gameTime = Time.time,
            state = GameState.Instance?.CurrentState.ToString(),
            remainingTime = GameState.Instance?.RemainingTime ?? 0,
            kirimi = GameState.Instance?.Kirimi ?? 0,
            lives = GameState.Instance?.Lives ?? 0,
            destroyedBricks = GameState.Instance?.DestroyedBricks ?? 0,
            totalBricks = GameState.Instance?.TotalBricks ?? 0,
            ballPos = ball != null ? $"{ball.transform.position.x:F2},{ball.transform.position.y:F2}" : "null",
            paddlePos = paddle != null ? $"{paddle.transform.position.x:F2}" : "null"
        });
    }

    object GetGameSummary()
    {
        return new {
            finalKirimi = GameState.Instance?.Kirimi ?? 0,
            finalLives = GameState.Instance?.Lives ?? 0,
            destroyedBricks = GameState.Instance?.DestroyedBricks ?? 0,
            totalBricks = GameState.Instance?.TotalBricks ?? 0,
            clearPercentage = GameState.Instance?.GetClearPercentage() ?? 0,
            rank = GameState.Instance?.GetRank() ?? "unknown",
            remainingTime = GameState.Instance?.RemainingTime ?? 0,
            totalFrames = frameCount,
            sessionDuration = Time.realtimeSinceStartup - sessionStartTime
        };
    }

    public void Log(string eventType, object data)
    {
        float timestamp = Time.realtimeSinceStartup - sessionStartTime;
        string dataJson = data != null ? JsonUtility.ToJson(data) : "{}";

        // JsonUtilityは匿名型をサポートしないので手動で構築
        string json = $"{{\"t\":{timestamp:F3},\"event\":\"{eventType}\",\"data\":{SerializeData(data)}}}";
        eventLog.Add(json);

        // デバッグ出力
        Debug.Log($"[SimLog] {eventType}: {SerializeData(data)}");
    }

    string SerializeData(object data)
    {
        if (data == null) return "{}";

        // リフレクションで匿名型をシリアライズ
        var type = data.GetType();
        var props = type.GetProperties();
        var sb = new StringBuilder("{");
        bool first = true;

        foreach (var prop in props)
        {
            if (!first) sb.Append(",");
            first = false;

            var value = prop.GetValue(data);
            string valueStr;
            if (value is string s)
                valueStr = $"\"{s}\"";
            else if (value is float f)
                valueStr = f.ToString("F2");
            else if (value is double d)
                valueStr = d.ToString("F2");
            else
                valueStr = value?.ToString() ?? "null";

            sb.Append($"\"{prop.Name}\":{valueStr}");
        }
        sb.Append("}");
        return sb.ToString();
    }

    void OnApplicationQuit()
    {
        SaveLog();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SaveLog();
        }
    }

    public void SaveLog()
    {
        if (eventLog.Count == 0) return;

        try
        {
            Log("SESSION_END", new {
                totalEvents = eventLog.Count,
                sessionDuration = Time.realtimeSinceStartup - sessionStartTime
            });

            string fullLog = "[\n" + string.Join(",\n", eventLog) + "\n]";
            File.WriteAllText(logFilePath, fullLog);
            Debug.Log($"[SimulationLogger] Log saved: {logFilePath} ({eventLog.Count} events)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SimulationLogger] Failed to save log: {e.Message}");
        }
    }
}
