using UnityEngine;

/// <summary>
/// シーン間でゲームデータを共有するシングルトン
/// DontDestroyOnLoadで永続化
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("Result Data")]
    public bool isGameClear;          // true=クリア, false=ゲームオーバー
    public bool isTimeOut;            // ゲームオーバー理由: true=時間切れ, false=ライフ切れ
    public int finalKirimi;           // 最終きりみ数
    public int totalBricks;           // 総ブロック数
    public int destroyedBricks;       // 破壊したブロック数
    public float remainingTime;       // 残り時間
    public int remainingLives;        // 残りライフ

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameData] Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// クリア時のデータを保存
    /// </summary>
    public void SaveClearData(int kirimi, int total, int destroyed, float time, int lives)
    {
        isGameClear = true;
        isTimeOut = false;
        finalKirimi = kirimi;
        totalBricks = total;
        destroyedBricks = destroyed;
        remainingTime = time;
        remainingLives = lives;
        Debug.Log($"[GameData] SaveClearData: kirimi={kirimi}, destroyed={destroyed}/{total}");
    }

    /// <summary>
    /// ゲームオーバー時のデータを保存
    /// </summary>
    public void SaveGameOverData(int kirimi, int total, int destroyed, float time, int lives, bool timeout)
    {
        isGameClear = false;
        isTimeOut = timeout;
        finalKirimi = kirimi;
        totalBricks = total;
        destroyedBricks = destroyed;
        remainingTime = time;
        remainingLives = lives;
        Debug.Log($"[GameData] SaveGameOverData: kirimi={kirimi}, timeout={timeout}");
    }

    /// <summary>
    /// クリア率を計算 (0-100)
    /// </summary>
    public float GetClearPercentage()
    {
        if (totalBricks <= 0) return 0f;
        return (float)destroyedBricks / totalBricks * 100f;
    }

    /// <summary>
    /// ランクキーを取得
    /// </summary>
    public string GetRankKey()
    {
        float pct = GetClearPercentage();
        if (pct >= 100f) return "osashimi";
        if (pct >= 80f) return "tataki";
        if (pct >= 60f) return "nigiri";
        if (pct >= 40f) return "arani";
        return "esa";
    }

    /// <summary>
    /// データをリセット
    /// </summary>
    public void Reset()
    {
        isGameClear = false;
        isTimeOut = false;
        finalKirimi = 0;
        totalBricks = 0;
        destroyedBricks = 0;
        remainingTime = 0;
        remainingLives = 0;
    }
}
