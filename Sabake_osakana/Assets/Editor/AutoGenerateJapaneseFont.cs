using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using TMPro;
using TMPro.EditorUtilities;
using System.Collections.Generic;

/// <summary>
/// 日本語TMPフォントアセットを自動生成するエディタスクリプト
/// </summary>
public class AutoGenerateJapaneseFont : Editor
{
    [MenuItem("Tools/Auto Generate Japanese Font Asset")]
    public static void GenerateJapaneseFont()
    {
        // フォントパス (MaruMonica ピクセルフォント)
        string fontPath = "Assets/Fonts/x12y16pxMaruMonica.ttf";

        // フォントをロード
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

        if (sourceFont == null)
        {
            // TTCの場合、サブアセットから取得
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
            Debug.LogError("[AutoGenerateJapaneseFont] フォントが見つかりません: " + fontPath);
            EditorUtility.DisplayDialog("エラー", "フォントが見つかりません: " + fontPath, "OK");
            return;
        }

        Debug.Log("[AutoGenerateJapaneseFont] フォント読み込み成功: " + sourceFont.name);

        // ゲームで使用する文字
        string characters = GetGameCharacters();
        Debug.Log("[AutoGenerateJapaneseFont] 文字数: " + characters.Length);

        // TMP_FontAssetを生成
        string outputPath = "Assets/Fonts/MaruMonica-Japanese SDF.asset";

        // 既存アセットの確認
        TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
        if (existingAsset != null)
        {
            AssetDatabase.DeleteAsset(outputPath);
            Debug.Log("[AutoGenerateJapaneseFont] 既存アセットを削除");
        }

        // TMPフォントアセットを作成
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,  // Sampling Point Size
            9,   // Padding
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            1024, // Atlas Width
            1024  // Atlas Height
        );

        if (fontAsset == null)
        {
            Debug.LogError("[AutoGenerateJapaneseFont] フォントアセットの作成に失敗");
            return;
        }

        fontAsset.name = "MaruMonica-Japanese SDF";

        // アセットとして保存
        AssetDatabase.CreateAsset(fontAsset, outputPath);

        // 文字を追加
        uint[] unicodes = new uint[characters.Length];
        for (int i = 0; i < characters.Length; i++)
        {
            unicodes[i] = characters[i];
        }

        fontAsset.TryAddCharacters(unicodes, out uint[] missing);

        if (missing != null && missing.Length > 0)
        {
            Debug.LogWarning($"[AutoGenerateJapaneseFont] {missing.Length}文字が追加できませんでした");
        }

        // アトラステクスチャも保存
        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        // マテリアルも保存
        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AutoGenerateJapaneseFont] フォントアセット作成完了: {outputPath}");
        EditorUtility.DisplayDialog("成功",
            $"日本語フォントアセットを作成しました！\n{outputPath}\n\n" +
            "次に、シーン内のTMPテキストにこのフォントを適用してください。",
            "OK");

        // 作成したアセットを選択
        Selection.activeObject = fontAsset;
        EditorGUIUtility.PingObject(fontAsset);
    }

    static string GetGameCharacters()
    {
        // ASCII基本文字
        string ascii = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        // ゲーム内テキスト
        string gameText = "きりみポーズおさかなさばけたつづけるおわるリトライタイトルへスペースキーで発射さばいたおさしみ級たたき級にぎり級あら煮級エサ級見事なさばきっぷり新鮮なおさしみができましたなかなかの腕前おいしいたたきになりましたまずまずの出来にぎり寿司にしましょうもう少しがんばろうあら煮にして食べよう練習あるのみエサにするしかない時間切れおさかなが逃げてしまったライフがなくなったまな板から落ちてしまった";

        // ひらがな
        string hiragana = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゃゅょっ";

        // カタカナ
        string katakana = "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポャュョッー";

        // 記号
        string symbols = "♥０１２３４５６７８９：！？。、「」";

        // 重複除去
        HashSet<char> charSet = new HashSet<char>();
        foreach (char c in ascii + gameText + hiragana + katakana + symbols)
        {
            if (!char.IsWhiteSpace(c) || c == ' ')
            {
                charSet.Add(c);
            }
        }

        char[] sorted = new char[charSet.Count];
        charSet.CopyTo(sorted);
        System.Array.Sort(sorted);
        return new string(sorted);
    }
}
