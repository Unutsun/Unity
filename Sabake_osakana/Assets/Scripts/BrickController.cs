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
            // 落下きりみをスポーン（スコアは落下きりみ取得時に加算）
            if (KirimiSpawner.Instance != null)
            {
                KirimiSpawner.Instance.SpawnKirimi(transform.position);
            }

            // ブロック破壊カウント用（スコアは0、カウントのみ）
            GameEvents.TriggerBrickDestroyed(0);

            Debug.Log($"Brick destroyed! Kirimi spawned at {transform.position}");
            Destroy(gameObject);
        }
    }
}
