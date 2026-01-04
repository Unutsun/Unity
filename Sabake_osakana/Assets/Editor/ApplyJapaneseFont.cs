using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// シーン内のすべてのTMPテキストに日本語フォントを適用
/// </summary>
public class ApplyJapaneseFont : Editor
{
    [MenuItem("Tools/Apply Japanese Font to All TMP Texts")]
    public static void ApplyFont()
    {
        // 日本語フォントアセットを検索
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset Japanese");
        TMP_FontAsset japaneseFont = null;

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            japaneseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }

        if (japaneseFont == null)
        {
            // MaruMonica-Japaneseを直接検索
            japaneseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MaruMonica-Japanese SDF.asset");
        }

        if (japaneseFont == null)
        {
            Debug.LogError("[ApplyJapaneseFont] 日本語フォントアセットが見つかりません。先に「Tools/Auto Generate Japanese Font Asset」を実行してください。");
            EditorUtility.DisplayDialog("エラー",
                "日本語フォントアセットが見つかりません。\n\n" +
                "先に「Tools/Auto Generate Japanese Font Asset」を実行してください。", "OK");
            return;
        }

        Debug.Log("[ApplyJapaneseFont] フォント発見: " + japaneseFont.name);

        // 現在のシーンのすべてのTMPテキストを取得
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int count = 0;
        foreach (var text in allTexts)
        {
            Undo.RecordObject(text, "Apply Japanese Font");
            text.font = japaneseFont;
            EditorUtility.SetDirty(text);
            count++;
            Debug.Log($"[ApplyJapaneseFont] フォント適用: {text.gameObject.name}");
        }

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[ApplyJapaneseFont] 完了: {count}個のテキストにフォントを適用しました");
        EditorUtility.DisplayDialog("完了",
            $"{count}個のTextMeshProUGUIに日本語フォントを適用しました。\n\n" +
            "シーンを保存してください（Ctrl+S）", "OK");
    }

    [MenuItem("Tools/Setup Japanese Font (Full Process)")]
    public static void FullSetup()
    {
        // 1. フォントアセット生成
        Debug.Log("[FullSetup] Step 1: フォントアセット生成開始");

        string fontPath = "Assets/Fonts/x12y16pxMaruMonica.ttf";

        // TTFファイルを直接読み込み
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

        if (sourceFont == null)
        {
            // LoadAllAssetsでサブアセットも試す
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(fontPath);
            foreach (var asset in allAssets)
            {
                if (asset is Font f)
                {
                    sourceFont = f;
                    break;
                }
            }
        }

        if (sourceFont == null)
        {
            Debug.LogError($"[FullSetup] MaruMonicaフォントが見つかりません: {fontPath}");
            Debug.LogError("[FullSetup] Assets/Fontsフォルダにx12y16pxMaruMonica.ttfが存在するか確認してください");
            EditorUtility.DisplayDialog("エラー",
                $"フォントが見つかりません:\n{fontPath}\n\nAssets/Fontsフォルダを確認してください", "OK");
            return;
        }

        Debug.Log($"[FullSetup] フォント読み込み成功: {sourceFont.name}");

        // フォントアセットを作成
        string outputPath = "Assets/Fonts/MaruMonica-Japanese SDF.asset";

        TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
        if (existingAsset != null)
        {
            AssetDatabase.DeleteAsset(outputPath);
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            48,  // Sampling Point Size (smaller for faster generation)
            5,   // Padding
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            512, // Atlas Width
            512  // Atlas Height
        );

        if (fontAsset == null)
        {
            Debug.LogError("[FullSetup] フォントアセット作成失敗");
            return;
        }

        fontAsset.name = "MaruMonica-Japanese SDF";

        // 保存
        AssetDatabase.CreateAsset(fontAsset, outputPath);

        // 必要な文字を追加
        string chars = GetGameCharacters();
        uint[] unicodes = new uint[chars.Length];
        for (int i = 0; i < chars.Length; i++)
        {
            unicodes[i] = chars[i];
        }

        fontAsset.TryAddCharacters(unicodes, out uint[] missing);

        // アトラスとマテリアルを保存
        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FullSetup] Step 1完了: フォントアセット作成");

        // 2. TMPテキストに適用
        Debug.Log("[FullSetup] Step 2: フォント適用開始");

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int count = 0;
        foreach (var text in allTexts)
        {
            Undo.RecordObject(text, "Apply Japanese Font");
            text.font = fontAsset;
            EditorUtility.SetDirty(text);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[FullSetup] 完了: {count}個のテキストに日本語フォントを適用しました");

        EditorUtility.DisplayDialog("セットアップ完了",
            $"日本語フォントセットアップが完了しました！\n\n" +
            $"・フォントアセット作成: {outputPath}\n" +
            $"・適用したテキスト数: {count}\n\n" +
            "シーンを保存してください（Ctrl+S）", "OK");
    }

    static string GetGameCharacters()
    {
        return " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
               "きりみポーズおさかなさばけたつづけるおわるリトライタイトルへスペースキーで発射さばいた" +
               "おさしみ級たたき級にぎり級あら煮級エサ級見事なさばきっぷり新鮮なおさしみができました" +
               "なかなかの腕前おいしいたたきになりましたまずまずの出来にぎり寿司にしましょう" +
               "もう少しがんばろうあら煮にして食べよう練習あるのみエサにするしかない" +
               "時間切れおさかなが逃げてしまったライフがなくなったまな板から落ちてしまった" +
               "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん" +
               "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゃゅょっ" +
               "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン" +
               "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポャュョッー" +
               "♥０１２３４５６７８９：！？。、「」";
    }
}
