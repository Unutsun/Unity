using UnityEngine;

/// <summary>
/// ゲーム全体の色設定を一元管理
/// 仕様書の画面イメージに基づいた色定義
/// </summary>
public static class GameColors
{
    // === 背景色 ===
    /// <summary>ゲーム背景（クリーム/ベージュ - 和紙風）</summary>
    public static readonly Color Background = new Color(0.96f, 0.94f, 0.90f);  // #F5F0E6

    // === パネル背景色 ===
    /// <summary>リザルト/ポーズパネルの背景（和紙風クリーム）</summary>
    public static readonly Color PanelBackground = new Color(0.96f, 0.94f, 0.90f);

    // === テキスト色 ===
    /// <summary>通常テキスト（黒）- HUDラベル等</summary>
    public static readonly Color TextBlack = new Color(0.15f, 0.12f, 0.10f);

    /// <summary>ハート/ランク強調（赤系）</summary>
    public static readonly Color TextRed = new Color(0.85f, 0.15f, 0.15f);

    /// <summary>HUDテキスト（黒）- 仕様書に合わせて黒に変更</summary>
    public static readonly Color TextHUD = new Color(0.15f, 0.12f, 0.10f);

    /// <summary>失ったライフ（グレー）</summary>
    public static readonly Color HeartEmpty = new Color(0.5f, 0.5f, 0.5f);

    // === ボタン色 ===
    /// <summary>ボタン背景（クリーム）</summary>
    public static readonly Color ButtonBackground = new Color(0.96f, 0.94f, 0.90f);

    /// <summary>ボタン枠線（手描き風ダークブラウン）</summary>
    public static readonly Color ButtonBorder = new Color(0.25f, 0.20f, 0.15f);

    // === ブロック色（フォールバック用）===
    /// <summary>魚の身（薄赤）</summary>
    public static readonly Color FishBody = new Color(1f, 0.6f, 0.6f);

    /// <summary>魚の腹（白っぽい）</summary>
    public static readonly Color FishBelly = new Color(1f, 0.85f, 0.85f);

    /// <summary>骨の背景（クリーム - 背景と同色）</summary>
    public static readonly Color BoneBackground = new Color(0.96f, 0.94f, 0.90f);

    // === 互換性のため ===
    public static readonly Color TextWhite = Color.white;
}
