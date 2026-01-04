using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// 日本語フォントをTextMeshProで使用するためのセットアップツール
/// </summary>
public class JapaneseFontSetup : EditorWindow
{
    [MenuItem("Tools/Setup Japanese Font")]
    public static void ShowWindow()
    {
        GetWindow<JapaneseFontSetup>("Japanese Font Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("日本語フォントセットアップ", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("このツールはMeiryoフォントからTMPフォントアセットを作成します。");
        GUILayout.Space(10);

        if (GUILayout.Button("1. Font Asset Creator を開く"))
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
        }

        GUILayout.Space(10);
        GUILayout.Label("手順:", EditorStyles.boldLabel);
        GUILayout.Label("1. Source Font File: Assets/Fonts/meiryo.ttc を選択");
        GUILayout.Label("2. Atlas Resolution: 4096 x 4096");
        GUILayout.Label("3. Character Set: Custom Characters");
        GUILayout.Label("4. Custom Character Listに下のテキストをコピペ");
        GUILayout.Label("5. Generate Font Atlas をクリック");
        GUILayout.Label("6. Save をクリック");

        GUILayout.Space(10);

        if (GUILayout.Button("2. 必要な日本語文字をクリップボードにコピー"))
        {
            string chars = GetRequiredCharacters();
            GUIUtility.systemCopyBuffer = chars;
            Debug.Log("日本語文字をクリップボードにコピーしました！");
            EditorUtility.DisplayDialog("コピー完了",
                "必要な文字をクリップボードにコピーしました。\n" +
                "Font Asset CreatorのCustom Character Listに貼り付けてください。", "OK");
        }

        GUILayout.Space(10);
        GUILayout.Label("コピーされる文字:", EditorStyles.boldLabel);

        string preview = GetRequiredCharacters();
        EditorGUILayout.TextArea(preview, GUILayout.Height(100));
    }

    static string GetRequiredCharacters()
    {
        // ゲームで使用される日本語文字 + 基本的なASCII
        string ascii = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        // ゲームで使用される日本語
        string gameText = @"
きりみポーズおさかなさばけたつづけるおわるリトライタイトルへ
スペースキーで発射！
さばいたきりみ
おさしみ級たたき級にぎり級あら煮級エサ級
見事なさばきっぷり新鮮なおさしみができました
なかなかの腕前おいしいたたきになりました
まずまずの出来にぎり寿司にしましょう
もう少しがんばろうあら煮にして食べよう
練習あるのみエサにするしかない
時間切れおさかなが逃げてしまった
ライフがなくなったまな板から落ちてしまった
♥
";

        // ひらがな全部
        string hiragana = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゃゅょっ";

        // カタカナ全部
        string katakana = "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポャュョッー";

        // 数字・記号
        string symbols = "０１２３４５６７８９：！？。、「」";

        // 重複を除去してソート
        string all = ascii + gameText + hiragana + katakana + symbols;
        var chars = new System.Collections.Generic.HashSet<char>();
        foreach (char c in all)
        {
            if (!char.IsWhiteSpace(c) || c == ' ')
            {
                chars.Add(c);
            }
        }

        var sorted = new System.Collections.Generic.List<char>(chars);
        sorted.Sort();
        return new string(sorted.ToArray());
    }
}
