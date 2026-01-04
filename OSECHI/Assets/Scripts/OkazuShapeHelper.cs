using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class OkazuShapeHelper
{
    public static readonly string[][] Shapes = new string[][]
    {
        new string[] { "1,1,1" },           // Kamaboko
        new string[] { "1,1", "1,1" },      // Datemaki
        new string[] { "1" },               // Kuromame
        new string[] { "1,1", "1,1" },      // Kurikinton
        new string[] { "1", "1", "1" },     // Kazunoko
        new string[] { "1,1,1", "0,0,1" },  // Ebi
        new string[] { "1", "1" },          // Tazukuri
        new string[] { "1,1" },             // Kobumaki
        new string[] { "1,1,1", "0,1,0" },  // Nishime
        new string[] { "1,1" }              // Renkon
    };

    public static readonly string[] Names = new string[]
    {
        "Kamaboko", "Datemaki", "Kuromame", "Kurikinton", "Kazunoko",
        "Ebi", "Tazukuri", "Kobumaki", "Nishime", "Renkon"
    };

    public static readonly Color[] Colors = new Color[]
    {
        new Color(1f, 0.8f, 0.8f),      // Kamaboko - pink
        new Color(1f, 0.9f, 0.5f),      // Datemaki - yellow
        new Color(0.2f, 0.2f, 0.2f),    // Kuromame - black
        new Color(1f, 0.8f, 0.3f),      // Kurikinton - gold
        new Color(1f, 1f, 0.7f),        // Kazunoko - light yellow
        new Color(1f, 0.5f, 0.3f),      // Ebi - orange-red
        new Color(0.6f, 0.5f, 0.3f),    // Tazukuri - brown
        new Color(0.3f, 0.4f, 0.2f),    // Kobumaki - dark green
        new Color(0.7f, 0.5f, 0.3f),    // Nishime - brown
        new Color(0.9f, 0.9f, 0.8f)     // Renkon - white
    };
}

#if UNITY_EDITOR
public class OkazuDataCreator : EditorWindow
{
    [MenuItem("OSECHI/Create Sample Okazu Data")]
    public static void CreateSampleData()
    {
        string folder = "Assets/ScriptableObjects/OkazuTypes";

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "OkazuTypes");

        for (int i = 0; i < OkazuShapeHelper.Names.Length; i++)
        {
            var data = ScriptableObject.CreateInstance<OkazuData>();
            data.okazuName = OkazuShapeHelper.Names[i];
            data.shapeRows = OkazuShapeHelper.Shapes[i];
            data.color = OkazuShapeHelper.Colors[i];

            string path = $"{folder}/{OkazuShapeHelper.Names[i]}.asset";
            AssetDatabase.CreateAsset(data, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created sample okazu data!");
    }

    [MenuItem("OSECHI/Setup Game Scene")]
    public static void SetupGameScene()
    {
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();

        var targetObj = new GameObject("TargetBox");
        var targetBox = targetObj.AddComponent<OsechiBox>();
        targetBox.isTargetBox = true;
        targetBox.gridWidth = 4;
        targetBox.gridHeight = 4;
        targetBox.transform.position = new Vector3(0, 0, 0);
        gm.targetBox = targetBox;

        var leftObj = new GameObject("LeftBox");
        var leftBox = leftObj.AddComponent<OsechiBox>();
        leftBox.gridWidth = 4;
        leftBox.gridHeight = 4;
        leftBox.transform.position = new Vector3(-6, 0, 0);

        var rightObj = new GameObject("RightBox");
        var rightBox = rightObj.AddComponent<OsechiBox>();
        rightBox.gridWidth = 4;
        rightBox.gridHeight = 4;
        rightBox.transform.position = new Vector3(6, 0, 0);

        gm.sourceBoxes = new OsechiBox[] { leftBox, rightBox };

        var okazuParent = new GameObject("OkazuParent");
        gm.okazuParent = okazuParent.transform;

        Debug.Log("Game scene setup complete!");
    }
}
#endif
