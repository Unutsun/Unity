using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

/// <summary>
/// 角丸ボタン用スプライトを生成・適用
/// </summary>
public class RoundedButtonSetup : Editor
{
    [MenuItem("Tools/Create Rounded Button Sprite")]
    public static void CreateRoundedSprite()
    {
        int width = 64;
        int height = 64;
        int radius = 16; // 角丸の半径

        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                pixels[y * width + x] = inside ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // PNGとして保存
        string path = "Assets/Resources/RoundedButton.png";
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngData);
        AssetDatabase.Refresh();

        // Import設定
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.spriteBorder = new Vector4(radius, radius, radius, radius); // 9-slice
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        Debug.Log($"[RoundedButtonSetup] Created rounded sprite at {path}");
        EditorUtility.DisplayDialog("完了", $"角丸スプライトを作成しました:\n{path}", "OK");
    }

    static bool IsInsideRoundedRect(int x, int y, int w, int h, int r)
    {
        // 4つの角をチェック
        // 左下
        if (x < r && y < r)
            return IsInsideCircle(x, y, r, r, r);
        // 右下
        if (x >= w - r && y < r)
            return IsInsideCircle(x, y, w - r - 1, r, r);
        // 左上
        if (x < r && y >= h - r)
            return IsInsideCircle(x, y, r, h - r - 1, r);
        // 右上
        if (x >= w - r && y >= h - r)
            return IsInsideCircle(x, y, w - r - 1, h - r - 1, r);

        return true; // 角以外は矩形内
    }

    static bool IsInsideCircle(int x, int y, int cx, int cy, int r)
    {
        float dx = x - cx;
        float dy = y - cy;
        return (dx * dx + dy * dy) <= (r * r);
    }

    [MenuItem("Tools/Apply Rounded Sprite to Result Buttons")]
    public static void ApplyToResultButtons()
    {
        Sprite roundedSprite = Resources.Load<Sprite>("RoundedButton");
        if (roundedSprite == null)
        {
            EditorUtility.DisplayDialog("エラー", "RoundedButton.pngが見つかりません。\n先に'Create Rounded Button Sprite'を実行してください。", "OK");
            return;
        }

        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null)
        {
            Debug.LogError("GameCanvas not found");
            return;
        }

        string[] buttonPaths = {
            "ResultPanel/ContinueButton",
            "ResultPanel/EndButton",
            "GameOverPanel/RetryButton",
            "GameOverPanel/TitleButton",
            "PausePanel/ResumeButton",
            "PausePanel/PauseTitleButton"
        };

        int count = 0;
        foreach (string path in buttonPaths)
        {
            Transform t = canvas.transform.Find(path);
            if (t != null)
            {
                Image img = t.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = roundedSprite;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.8f, 0.8f, 0.8f); // BONE色
                    count++;
                }
            }
        }

        // ResultPanelにも適用
        Transform resultPanel = canvas.transform.Find("ResultPanel");
        if (resultPanel != null)
        {
            Image panelImg = resultPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.sprite = roundedSprite;
                panelImg.type = Image.Type.Sliced;
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"[RoundedButtonSetup] Applied rounded sprite to {count} buttons");
        EditorUtility.DisplayDialog("完了", $"{count}個のボタンに角丸スプライトを適用しました。\nシーンを保存してください。", "OK");
    }
}
