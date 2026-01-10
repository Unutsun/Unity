using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の色設定を一元管理
/// CSVから色を読み込み、各UIコンポーネントで参照
/// </summary>
public static class GameColors
{
    private static Dictionary<string, Color> colorCache = new Dictionary<string, Color>();
    private static bool isInitialized = false;

    // === デフォルト値（CSV読み込み前のフォールバック）===
    private static readonly Dictionary<string, Color> defaultColors = new Dictionary<string, Color>
    {
        // 背景色
        { "background.game", new Color(0.96f, 0.94f, 0.90f) },
        { "background.panel", new Color(0.96f, 0.94f, 0.90f) },
        { "background.bone", new Color(0.96f, 0.94f, 0.90f) },
        // テキスト色
        { "text.black", new Color(0.15f, 0.12f, 0.10f) },
        { "text.red", new Color(0.85f, 0.15f, 0.15f) },
        { "text.hud", new Color(0.15f, 0.12f, 0.10f) },
        { "text.white", Color.white },
        { "text.gray", new Color(0.5f, 0.5f, 0.5f) },
        // ボタン色
        { "button.background", new Color(0.96f, 0.94f, 0.90f) },
        { "button.border", new Color(0.25f, 0.20f, 0.15f) },
        { "button.ready_bg", Color.white },
        { "button.ready_border", new Color(0.3f, 0.3f, 0.3f) },
        // まないた/シークバー色
        { "ui.manaita", new Color(0.87f, 0.76f, 0.55f) },
        { "ui.seekbar_bg", new Color(0.7f, 0.6f, 0.4f) },
        { "ui.seekbar_fill", new Color(0.87f, 0.76f, 0.55f) },
        // ブロック色
        { "fish.body", new Color(1f, 0.6f, 0.6f) },
        { "fish.belly", new Color(1f, 0.85f, 0.85f) },
    };

    /// <summary>
    /// CSVから色データを読み込む
    /// </summary>
    public static void Initialize()
    {
        if (isInitialized) return;

        TextAsset csvAsset = Resources.Load<TextAsset>("ColorData");
        if (csvAsset != null)
        {
            ParseCSV(csvAsset.text);
            Debug.Log("[GameColors] Loaded colors from CSV");
        }
        else
        {
            Debug.LogWarning("[GameColors] ColorData.csv not found, using defaults");
        }
        isInitialized = true;
    }

    private static void ParseCSV(string csvText)
    {
        string[] lines = csvText.Split('\n');
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

            string[] parts = trimmed.Split(',');
            if (parts.Length < 6) continue;

            string category = parts[0].Trim();
            string key = parts[1].Trim();

            if (category == "category") continue; // ヘッダー行スキップ

            if (float.TryParse(parts[2], out float r) &&
                float.TryParse(parts[3], out float g) &&
                float.TryParse(parts[4], out float b) &&
                float.TryParse(parts[5], out float a))
            {
                string colorKey = $"{category}.{key}";
                colorCache[colorKey] = new Color(r, g, b, a);
            }
        }
    }

    /// <summary>
    /// 色を取得（キー形式: "category.key"）
    /// </summary>
    public static Color GetColor(string category, string key)
    {
        if (!isInitialized) Initialize();

        string colorKey = $"{category}.{key}";
        if (colorCache.TryGetValue(colorKey, out Color color))
            return color;
        if (defaultColors.TryGetValue(colorKey, out Color defaultColor))
            return defaultColor;
        return Color.magenta; // 見つからない場合は目立つ色
    }

    // === 互換性のためのプロパティ（既存コードから参照可能）===

    // 背景色
    public static Color Background => GetColor("background", "game");
    public static Color PanelBackground => GetColor("background", "panel");
    public static Color BoneBackground => GetColor("background", "bone");

    // テキスト色
    public static Color TextBlack => GetColor("text", "black");
    public static Color TextRed => GetColor("text", "red");
    public static Color TextHUD => GetColor("text", "hud");
    public static Color TextWhite => GetColor("text", "white");
    public static Color HeartEmpty => GetColor("text", "gray");

    // ボタン色
    public static Color ButtonBackground => GetColor("button", "background");
    public static Color ButtonBorder => GetColor("button", "border");
    public static Color ReadyButtonBackground => GetColor("button", "ready_bg");
    public static Color ReadyButtonBorder => GetColor("button", "ready_border");

    // まないた/シークバー色
    public static Color Manaita => GetColor("ui", "manaita");
    public static Color SeekbarBackground => GetColor("ui", "seekbar_bg");
    public static Color SeekbarFill => GetColor("ui", "seekbar_fill");

    // ブロック色
    public static Color FishBody => GetColor("fish", "body");
    public static Color FishBelly => GetColor("fish", "belly");

    /// <summary>
    /// 色を再読み込み（デバッグ用）
    /// </summary>
    public static void Reload()
    {
        isInitialized = false;
        colorCache.Clear();
        Initialize();
    }
}
