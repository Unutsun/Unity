using System;
using UnityEngine;

/// <summary>
/// ゲーム全体のイベントを管理する静的クラス
/// 各コンポーネントはこのイベントを通じて疎結合に通信する
/// 「さばけ！おさかな」対応版
/// </summary>
public static class GameEvents
{
    // ゲーム状態
    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameOver;
    public static event Action OnGameClear;
    public static event Action OnGameRestart;
    public static event Action OnReturnToTitle;

    // ボール関連
    public static event Action OnBallLaunched;
    public static event Action OnBallLost;
    public static event Action OnBallReset;

    // ブロック関連
    public static event Action<int> OnBrickDestroyed; // きりみ数を渡す
    public static event Action OnAllBricksDestroyed;

    // きりみ・ライフ・タイマー関連
    public static event Action<int> OnKirimiChanged;   // 旧OnScoreChanged
    public static event Action<int> OnLivesChanged;
    public static event Action<float> OnTimeChanged;   // 残り時間
    public static event Action OnTimeUp;               // タイムアップ

    // イベント発火メソッド（デバッグログ付き）
    public static void TriggerGameStart()
    {
        Debug.Log("[GameEvents] TriggerGameStart");
        OnGameStart?.Invoke();
    }

    public static void TriggerGamePause()
    {
        Debug.Log("[GameEvents] TriggerGamePause");
        OnGamePause?.Invoke();
    }

    public static void TriggerGameResume()
    {
        Debug.Log("[GameEvents] TriggerGameResume");
        OnGameResume?.Invoke();
    }

    public static void TriggerGameOver()
    {
        Debug.Log("[GameEvents] TriggerGameOver");
        OnGameOver?.Invoke();
    }

    public static void TriggerGameClear()
    {
        Debug.Log("[GameEvents] TriggerGameClear");
        OnGameClear?.Invoke();
    }

    public static void TriggerGameRestart()
    {
        Debug.Log("[GameEvents] TriggerGameRestart");
        OnGameRestart?.Invoke();
    }

    public static void TriggerReturnToTitle()
    {
        Debug.Log("[GameEvents] TriggerReturnToTitle");
        OnReturnToTitle?.Invoke();
    }

    public static void TriggerBallLaunched()
    {
        Debug.Log("[GameEvents] TriggerBallLaunched");
        OnBallLaunched?.Invoke();
    }

    public static void TriggerBallLost()
    {
        Debug.Log("[GameEvents] TriggerBallLost");
        OnBallLost?.Invoke();
    }

    public static void TriggerBallReset()
    {
        Debug.Log("[GameEvents] TriggerBallReset");
        OnBallReset?.Invoke();
    }

    public static void TriggerBrickDestroyed(int kirimi)
    {
        Debug.Log($"[GameEvents] TriggerBrickDestroyed: +{kirimi} kirimi");
        OnBrickDestroyed?.Invoke(kirimi);
    }

    public static void TriggerAllBricksDestroyed()
    {
        Debug.Log("[GameEvents] TriggerAllBricksDestroyed");
        OnAllBricksDestroyed?.Invoke();
    }

    public static void TriggerKirimiChanged(int kirimi)
    {
        Debug.Log($"[GameEvents] TriggerKirimiChanged: {kirimi}");
        OnKirimiChanged?.Invoke(kirimi);
    }

    public static void TriggerLivesChanged(int lives)
    {
        Debug.Log($"[GameEvents] TriggerLivesChanged: {lives}");
        OnLivesChanged?.Invoke(lives);
    }

    public static void TriggerTimeChanged(float time)
    {
        // 毎フレーム呼ばれるのでログは出さない
        OnTimeChanged?.Invoke(time);
    }

    public static void TriggerTimeUp()
    {
        Debug.Log("[GameEvents] TriggerTimeUp");
        OnTimeUp?.Invoke();
    }

    // イベントリスナーをすべてクリア（シーン遷移時に呼ぶ）
    public static void ClearAllListeners()
    {
        Debug.Log("[GameEvents] ClearAllListeners");
        OnGameStart = null;
        OnGamePause = null;
        OnGameResume = null;
        OnGameOver = null;
        OnGameClear = null;
        OnGameRestart = null;
        OnReturnToTitle = null;
        OnBallLaunched = null;
        OnBallLost = null;
        OnBallReset = null;
        OnBrickDestroyed = null;
        OnAllBricksDestroyed = null;
        OnKirimiChanged = null;
        OnLivesChanged = null;
        OnTimeChanged = null;
        OnTimeUp = null;
    }
}
