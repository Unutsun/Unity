using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI配置データ（1要素分）
/// </summary>
[System.Serializable]
public class UILayoutData
{
    public string element;
    public float anchorX;
    public float anchorY;
    public float pivotX;
    public float pivotY;
    public float posX;
    public float posY;
    public float width;
    public float height;
    public float fontSize;
    public string extra;

    public Vector2 AnchorMin => new Vector2(anchorX, anchorY);
    public Vector2 AnchorMax => new Vector2(anchorX, anchorY);
    public Vector2 Pivot => new Vector2(pivotX, pivotY);
    public Vector2 Position => new Vector2(posX, posY);
    public Vector2 Size => new Vector2(width, height);
}

/// <summary>
/// UI配置データ管理（疎結合設計）
/// CSVからレイアウトデータを読み込み、各UIコンポーネントに提供
/// </summary>
public class UILayoutManager : MonoBehaviour
{
    public static UILayoutManager Instance { get; private set; }

    private Dictionary<string, UILayoutData> layoutData = new Dictionary<string, UILayoutData>();
    private bool isLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadLayoutData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSVからレイアウトデータを読み込み
    /// </summary>
    void LoadLayoutData()
    {
        TextAsset csv = Resources.Load<TextAsset>("UILayoutData");
        if (csv == null)
        {
            Debug.LogWarning("[UILayoutManager] UILayoutData.csv not found!");
            return;
        }

        string[] lines = csv.text.Split('\n');
        bool isHeader = true;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // 空行・コメント行をスキップ
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            // ヘッダー行をスキップ
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            string[] values = trimmedLine.Split(',');
            if (values.Length < 10) continue;

            UILayoutData data = new UILayoutData
            {
                element = values[0].Trim(),
                anchorX = ParseFloat(values[1]),
                anchorY = ParseFloat(values[2]),
                pivotX = ParseFloat(values[3]),
                pivotY = ParseFloat(values[4]),
                posX = ParseFloat(values[5]),
                posY = ParseFloat(values[6]),
                width = ParseFloat(values[7]),
                height = ParseFloat(values[8]),
                fontSize = ParseFloat(values[9]),
                extra = values.Length > 10 ? values[10].Trim() : ""
            };

            layoutData[data.element] = data;
        }

        isLoaded = true;
        Debug.Log($"[UILayoutManager] Loaded {layoutData.Count} layout entries");
    }

    float ParseFloat(string value)
    {
        if (float.TryParse(value.Trim(), out float result))
            return result;
        return 0f;
    }

    /// <summary>
    /// 要素名からレイアウトデータを取得
    /// </summary>
    public UILayoutData GetLayout(string elementName)
    {
        if (layoutData.TryGetValue(elementName, out UILayoutData data))
            return data;

        Debug.LogWarning($"[UILayoutManager] Layout not found: {elementName}");
        return null;
    }

    /// <summary>
    /// RectTransformにレイアウトを適用
    /// </summary>
    public bool ApplyLayout(RectTransform rt, string elementName)
    {
        UILayoutData data = GetLayout(elementName);
        if (data == null || rt == null) return false;

        rt.anchorMin = data.AnchorMin;
        rt.anchorMax = data.AnchorMax;
        rt.pivot = data.Pivot;
        rt.anchoredPosition = data.Position;
        rt.sizeDelta = data.Size;

        return true;
    }

    /// <summary>
    /// レイアウトデータが読み込み済みかどうか
    /// </summary>
    public bool IsLoaded => isLoaded;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
