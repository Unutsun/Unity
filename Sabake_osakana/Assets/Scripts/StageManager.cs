using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージ進行管理（疎結合設計）
/// - 現在のステージ番号を管理
/// - 次のステージへの遷移
/// - ステージ設定（将来的にCSV化可能）
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage Settings")]
    [Tooltip("最大ステージ数")]
    public int maxStages = 3;

    [Tooltip("ゲームシーン名（ステージ番号は自動付与されない、同じシーンを使用）")]
    public string gameSceneName = "SampleScene";

    [Tooltip("タイトルシーン名")]
    public string titleSceneName = "TitleScene";

    // 現在のステージ（1から開始）
    private int currentStage = 1;

    public int CurrentStage => currentStage;
    public int MaxStages => maxStages;
    public bool IsLastStage => currentStage >= maxStages;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[StageManager] Initialized. Current stage: {currentStage}/{maxStages}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ゲーム開始時にステージをリセット
    /// </summary>
    public void ResetToFirstStage()
    {
        currentStage = 1;
        Debug.Log($"[StageManager] Reset to stage {currentStage}");
    }

    /// <summary>
    /// 次のステージへ進む
    /// </summary>
    public void GoToNextStage()
    {
        if (IsLastStage)
        {
            Debug.Log("[StageManager] Already at last stage, going to title");
            GoToTitle();
            return;
        }

        currentStage++;
        Debug.Log($"[StageManager] Advancing to stage {currentStage}/{maxStages}");

        // 同じシーンをリロード（ステージデータは将来的に変更可能）
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// 現在のステージをリトライ
    /// </summary>
    public void RetryCurrentStage()
    {
        Debug.Log($"[StageManager] Retrying stage {currentStage}");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// タイトルへ戻る
    /// </summary>
    public void GoToTitle()
    {
        currentStage = 1;  // リセット
        Debug.Log("[StageManager] Going to title, stage reset to 1");
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// ステージ表示用文字列を取得
    /// </summary>
    public string GetStageDisplayText()
    {
        return $"ステージ {currentStage}";
    }

    /// <summary>
    /// 進行状況の表示用文字列を取得
    /// </summary>
    public string GetProgressText()
    {
        return $"{currentStage} / {maxStages}";
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
