using UnityEngine;

/// <summary>
/// ボール判定ユーティリティ（タグ不要・コンポーネントベース）
/// </summary>
public static class BallHelper
{
    /// <summary>
    /// GameObjectがボール（メインまたはサブ）かどうか判定
    /// </summary>
    public static bool IsBall(GameObject obj)
    {
        if (obj == null) return false;

        // メインボール（BallController）
        if (obj.GetComponent<BallController>() != null) return true;

        // サブボール（SubBall）
        if (obj.GetComponent<SubBall>() != null) return true;

        // 名前でフォールバック（念のため）
        if (obj.name == "Ball") return true;

        return false;
    }

    /// <summary>
    /// Collision2Dからボール判定
    /// </summary>
    public static bool IsBall(Collision2D collision)
    {
        return collision != null && IsBall(collision.gameObject);
    }

    /// <summary>
    /// Collider2Dからボール判定
    /// </summary>
    public static bool IsBall(Collider2D collider)
    {
        return collider != null && IsBall(collider.gameObject);
    }

    /// <summary>
    /// GameObjectがメインボールかどうか判定
    /// </summary>
    public static bool IsMainBall(GameObject obj)
    {
        if (obj == null) return false;
        return obj.GetComponent<BallController>() != null || obj.name == "Ball";
    }

    /// <summary>
    /// GameObjectがサブボールかどうか判定
    /// </summary>
    public static bool IsSubBall(GameObject obj)
    {
        if (obj == null) return false;
        return obj.GetComponent<SubBall>() != null;
    }
}
