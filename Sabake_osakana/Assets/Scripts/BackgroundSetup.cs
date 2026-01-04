using UnityEngine;

/// <summary>
/// 背景色とゲーム全体の見た目を設定
/// 色はGameColorsから取得（疎結合）
/// </summary>
public class BackgroundSetup : MonoBehaviour
{
    void Awake()
    {
        SetupBackground();
    }

    void Start()
    {
        // Awakeで設定できなかった場合の保険
        SetupBackground();
    }

    void SetupBackground()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // 常にGameColorsの背景色を使用
            cam.backgroundColor = GameColors.Background;
            cam.clearFlags = CameraClearFlags.SolidColor;
            Debug.Log($"[BackgroundSetup] Camera background set to: {GameColors.Background}");
        }
    }
}
