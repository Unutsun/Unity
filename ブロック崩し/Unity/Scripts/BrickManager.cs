using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ブロックの生成と管理を行うコンポーネント
/// </summary>
public class BrickManager : MonoBehaviour
{
    [Header("Brick Prefab")]
    [SerializeField] private GameObject brickPrefab;

    [Header("Layout Settings")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 8;
    [SerializeField] private float brickWidth = 1.5f;
    [SerializeField] private float brickHeight = 0.5f;
    [SerializeField] private float paddingX = 0.1f;
    [SerializeField] private float paddingY = 0.1f;
    [SerializeField] private Vector2 startOffset = new Vector2(0, 3f);

    [Header("Colors")]
    [SerializeField] private Color[] rowColors = new Color[]
    {
        new Color(1f, 0.42f, 0.42f),     // #FF6B6B - Red
        new Color(0.31f, 0.8f, 0.77f),   // #4ECDC4 - Teal
        new Color(0.27f, 0.72f, 0.82f),  // #45B7D1 - Blue
        new Color(1f, 0.63f, 0.48f),     // #FFA07A - Orange
        new Color(0.6f, 0.85f, 0.78f)    // #98D8C8 - Green
    };

    [Header("Scoring")]
    [SerializeField] private int scorePerBrick = 10;

    private List<BrickController> bricks = new List<BrickController>();
    private int activeBrickCount;

    // Events
    public System.Action<int> OnBrickDestroyed; // score
    public System.Action OnAllBricksDestroyed;

    public int TotalBricks => rows * columns;
    public int ActiveBricks => activeBrickCount;

    private void Start()
    {
        GenerateBricks();
    }

    /// <summary>
    /// ブロックを生成
    /// </summary>
    public void GenerateBricks()
    {
        ClearBricks();

        float totalWidth = columns * (brickWidth + paddingX) - paddingX;
        float startX = -totalWidth / 2f + brickWidth / 2f;

        for (int row = 0; row < rows; row++)
        {
            Color rowColor = rowColors[row % rowColors.Length];

            for (int col = 0; col < columns; col++)
            {
                float x = startX + col * (brickWidth + paddingX) + startOffset.x;
                float y = startOffset.y - row * (brickHeight + paddingY);

                GameObject brickObj = Instantiate(brickPrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                brickObj.name = $"Brick_{row}_{col}";
                brickObj.tag = "Brick";

                // スケール設定
                brickObj.transform.localScale = new Vector3(brickWidth, brickHeight, 1f);

                // BrickController の設定
                BrickController brick = brickObj.GetComponent<BrickController>();
                if (brick == null)
                {
                    brick = brickObj.AddComponent<BrickController>();
                }

                brick.Initialize(rowColor, 1, scorePerBrick);
                brick.OnDestroyed += HandleBrickDestroyed;

                bricks.Add(brick);
            }
        }

        activeBrickCount = bricks.Count;
        Debug.Log($"Generated {activeBrickCount} bricks");
    }

    /// <summary>
    /// ブロックをクリア
    /// </summary>
    public void ClearBricks()
    {
        foreach (var brick in bricks)
        {
            if (brick != null)
            {
                brick.OnDestroyed -= HandleBrickDestroyed;
                Destroy(brick.gameObject);
            }
        }
        bricks.Clear();
        activeBrickCount = 0;
    }

    /// <summary>
    /// ブロックをリセット
    /// </summary>
    public void ResetBricks()
    {
        foreach (var brick in bricks)
        {
            if (brick != null)
            {
                brick.ResetBrick();
            }
        }
        activeBrickCount = bricks.Count;
    }

    /// <summary>
    /// ブロック破壊時のハンドラ
    /// </summary>
    private void HandleBrickDestroyed(int score)
    {
        activeBrickCount--;
        OnBrickDestroyed?.Invoke(score);

        if (activeBrickCount <= 0)
        {
            OnAllBricksDestroyed?.Invoke();
        }
    }

    /// <summary>
    /// 残りのアクティブなブロック数を取得
    /// </summary>
    public int GetActiveBrickCount()
    {
        int count = 0;
        foreach (var brick in bricks)
        {
            if (brick != null && brick.IsActive)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// レイアウト設定を変更
    /// </summary>
    public void SetLayout(int newRows, int newCols)
    {
        rows = newRows;
        columns = newCols;
    }

    private void OnDrawGizmosSelected()
    {
        // エディタでブロック配置をプレビュー
        Gizmos.color = Color.cyan;
        float totalWidth = columns * (brickWidth + paddingX) - paddingX;
        float totalHeight = rows * (brickHeight + paddingY) - paddingY;

        Vector3 center = new Vector3(startOffset.x, startOffset.y - totalHeight / 2f + brickHeight / 2f, 0);
        Gizmos.DrawWireCube(center, new Vector3(totalWidth, totalHeight, 0.1f));
    }
}
