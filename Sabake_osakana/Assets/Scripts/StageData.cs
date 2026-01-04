using UnityEngine;

/// <summary>
/// ステージデータクラス
/// CSVから読み込んだステージ設定を保持
/// </summary>
[System.Serializable]
public class StageData
{
    public int stageId;           // ステージID
    public string stageName;      // ステージ名（アジ、サバ等）
    public string fishImagePath;  // 魚画像パス（Resources相対、拡張子なし）
    public int gridCols;          // グリッド列数
    public int gridRows;          // グリッド行数
    public float brickWidth;      // ブロック幅
    public int timeLimit;         // 制限時間（秒）
    public float bonusMultiplier; // スコア倍率

    public StageData()
    {
        stageId = 1;
        stageName = "デフォルト";
        fishImagePath = "Sprites/sakana_normal";
        gridCols = 17;
        gridRows = 13;
        brickWidth = 0.9f;
        timeLimit = 90;
        bonusMultiplier = 1.0f;
    }

    public override string ToString()
    {
        return $"Stage{stageId}: {stageName} ({fishImagePath}, {gridCols}x{gridRows}, {timeLimit}s)";
    }
}

/// <summary>
/// ステージデータ管理マネージャー
/// CSVからステージデータを読み込み、提供する
/// </summary>
public class StageDataManager : MonoBehaviour
{
    public static StageDataManager Instance { get; private set; }

    [Header("Settings")]
    public string csvFileName = "StageData";  // Resources内のCSVファイル名（拡張子なし）

    private StageData[] stages;
    private int currentStageIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStageData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadStageData()
    {
        Debug.Log($"[StageDataManager] LoadStageData: Loading from {csvFileName}");

        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        if (csvFile == null)
        {
            Debug.LogWarning($"[StageDataManager] LoadStageData: {csvFileName}.csv not found, using defaults");
            CreateDefaultStages();
            return;
        }

        ParseCSV(csvFile.text);
        Debug.Log($"[StageDataManager] LoadStageData: Loaded {stages.Length} stages");
    }

    void ParseCSV(string csvText)
    {
        string[] lines = csvText.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogWarning("[StageDataManager] ParseCSV: CSV has no data rows");
            CreateDefaultStages();
            return;
        }

        // ヘッダー行をスキップしてデータ行を処理
        var stageList = new System.Collections.Generic.List<StageData>();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 8)
            {
                Debug.LogWarning($"[StageDataManager] ParseCSV: Line {i} has insufficient columns: {line}");
                continue;
            }

            try
            {
                StageData stage = new StageData
                {
                    stageId = int.Parse(values[0].Trim()),
                    stageName = values[1].Trim(),
                    fishImagePath = values[2].Trim(),
                    gridCols = int.Parse(values[3].Trim()),
                    gridRows = int.Parse(values[4].Trim()),
                    brickWidth = float.Parse(values[5].Trim()),
                    timeLimit = int.Parse(values[6].Trim()),
                    bonusMultiplier = float.Parse(values[7].Trim())
                };
                stageList.Add(stage);
                Debug.Log($"[StageDataManager] ParseCSV: Loaded {stage}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[StageDataManager] ParseCSV: Error parsing line {i}: {e.Message}");
            }
        }

        stages = stageList.ToArray();

        if (stages.Length == 0)
        {
            CreateDefaultStages();
        }
    }

    void CreateDefaultStages()
    {
        Debug.Log("[StageDataManager] CreateDefaultStages: Creating default stage");
        stages = new StageData[]
        {
            new StageData()  // デフォルト値を使用
        };
    }

    /// <summary>
    /// 指定IDのステージデータを取得
    /// </summary>
    public StageData GetStage(int stageId)
    {
        if (stages == null || stages.Length == 0)
        {
            Debug.LogWarning("[StageDataManager] GetStage: No stages loaded");
            return new StageData();
        }

        foreach (var stage in stages)
        {
            if (stage.stageId == stageId)
                return stage;
        }

        Debug.LogWarning($"[StageDataManager] GetStage: Stage {stageId} not found, returning first stage");
        return stages[0];
    }

    /// <summary>
    /// 現在のステージデータを取得
    /// </summary>
    public StageData GetCurrentStage()
    {
        if (stages == null || stages.Length == 0)
            return new StageData();

        return stages[Mathf.Clamp(currentStageIndex, 0, stages.Length - 1)];
    }

    /// <summary>
    /// 現在のステージIDを設定
    /// </summary>
    public void SetCurrentStage(int stageId)
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].stageId == stageId)
            {
                currentStageIndex = i;
                Debug.Log($"[StageDataManager] SetCurrentStage: Set to stage {stageId} ({stages[i].stageName})");
                return;
            }
        }
        Debug.LogWarning($"[StageDataManager] SetCurrentStage: Stage {stageId} not found");
    }

    /// <summary>
    /// 次のステージに進む
    /// </summary>
    public bool NextStage()
    {
        if (currentStageIndex < stages.Length - 1)
        {
            currentStageIndex++;
            Debug.Log($"[StageDataManager] NextStage: Advanced to stage {stages[currentStageIndex].stageId}");
            return true;
        }
        Debug.Log("[StageDataManager] NextStage: Already at last stage");
        return false;
    }

    /// <summary>
    /// 全ステージ数を取得
    /// </summary>
    public int GetStageCount()
    {
        return stages?.Length ?? 0;
    }

    /// <summary>
    /// 全ステージデータを取得
    /// </summary>
    public StageData[] GetAllStages()
    {
        return stages;
    }
}
