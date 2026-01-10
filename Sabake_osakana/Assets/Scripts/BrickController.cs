using UnityEngine;

/// <summary>
/// 個々のブロックの動作を制御
/// イベント経由でスコアを通知
/// </summary>
public class BrickController : MonoBehaviour
{
    [Header("Brick Settings")]
    public int scoreValue = 1; // 1ブロック = 1きりみ

    void OnCollisionEnter2D(Collision2D collision)
    {
        // BallHelperで統一判定（メインボール・サブボール両対応）
        if (BallHelper.IsBall(collision))
        {
            // イベント経由でスコア加算を通知
            GameEvents.TriggerBrickDestroyed(scoreValue);

            Debug.Log($"Brick destroyed! +{scoreValue} points");
            Destroy(gameObject);
        }
    }
}
