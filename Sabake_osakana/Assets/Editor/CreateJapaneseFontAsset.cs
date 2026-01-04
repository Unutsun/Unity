using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Meiryoフォントから日本語TMPフォントアセットを自動作成
/// </summary>
public class CreateJapaneseFontAsset
{
    [MenuItem("Tools/Create Japanese TMP Font (Auto)")]
    public static void CreateFont()
    {
        // Meiryoフォントのパス
        string fontPath = "Assets/Fonts/meiryo.ttc";
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

        if (sourceFont == null)
        {
            // TTCファイルからフォントをロード
            Debug.Log("Loading font from: " + fontPath);

            // Fontをインポート
            AssetDatabase.ImportAsset(fontPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            // TTCファイルの場合、サブアセットとして複数フォントが含まれる
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fontPath);
            foreach (var asset in assets)
            {
                if (asset is Font font)
                {
                    sourceFont = font;
                    Debug.Log("Found font: " + font.name);
                    break;
                }
            }
        }

        if (sourceFont == null)
        {
            EditorUtility.DisplayDialog("エラー",
                "Meiryoフォントが見つかりません。\n" +
                "Assets/Fonts/meiryo.ttc を確認してください。", "OK");
            return;
        }

        // 出力先
        string outputPath = "Assets/Fonts/Meiryo-Japanese SDF.asset";

        // 既存のアセットがあれば削除
        if (File.Exists(outputPath))
        {
            AssetDatabase.DeleteAsset(outputPath);
        }

        // ゲームで使用する文字を取得
        string characters = GetGameCharacters();

        EditorUtility.DisplayDialog("Font Asset Creator を使用してください",
            "TMP Font Assetの自動生成には Font Asset Creator を使用する必要があります。\n\n" +
            "手順:\n" +
            "1. Window > TextMeshPro > Font Asset Creator を開く\n" +
            "2. Source Font: Assets/Fonts/meiryo.ttc\n" +
            "3. Atlas Resolution: 2048 x 2048\n" +
            "4. Character Set: Custom Characters\n" +
            "5. 「Copy Characters」ボタンで文字をコピー\n" +
            "6. Custom Character List に貼り付け\n" +
            "7. Generate Font Atlas → Save\n\n" +
            "必要な文字はクリップボードにコピーしました。", "OK");

        GUIUtility.systemCopyBuffer = characters;
        Debug.Log("Characters copied to clipboard: " + characters.Length + " characters");

        // Font Asset Creatorを開く
        EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
    }

    static string GetGameCharacters()
    {
        // ASCII
        string ascii = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        // ゲームテキスト
        string gameText = "きりみポーズおさかなさばけたつづけるおわるリトライタイトルへスペースキーで発射さばいたおさしみ級たたき級にぎり級あら煮級エサ級見事なさばきっぷり新鮮なおさしみができましたなかなかの腕前おいしいたたきになりましたまずまずの出来にぎり寿司にしましょうもう少しがんばろうあら煮にして食べよう練習あるのみエサにするしかない時間切れおさかなが逃げてしまったライフがなくなったまな板から落ちてしまった";

        // ひらがな・カタカナ
        string hiragana = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゃゅょっ";
        string katakana = "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポャュョッー";

        // 記号
        string symbols = "♥０１２３４５６７８９：！？。、「」";

        return ascii + gameText + hiragana + katakana + symbols;
    }
}
