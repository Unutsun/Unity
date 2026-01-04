using UnityEngine;
using System.Collections.Generic;

public class OsechiBox : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 4;
    public int gridHeight = 4;
    public float cellSize = 1f;

    [Header("Visual")]
    public Color gridColor = new Color(0.8f, 0.6f, 0.4f, 1f);
    public Color occupiedColor = new Color(1f, 0.5f, 0.5f, 0.5f);
    public Color validDropColor = new Color(0.5f, 1f, 0.5f, 0.5f);
    public Color invalidDropColor = new Color(1f, 0.3f, 0.3f, 0.5f);

    [Header("State")]
    public bool isTargetBox = false;

    private Okazu[,] grid;
    private List<Okazu> placedOkazu = new List<Okazu>();

    private bool showingPreview = false;
    private int previewX, previewY;
    private OkazuData previewData;
    private bool previewValid;

    private void Awake()
    {
        grid = new Okazu[gridWidth, gridHeight];
    }

    public Vector3 GetWorldPosition(int gridX, int gridY)
    {
        float offsetX = (gridWidth - 1) * cellSize * 0.5f;
        float offsetY = (gridHeight - 1) * cellSize * 0.5f;
        return transform.position + new Vector3(
            gridX * cellSize - offsetX,
            -gridY * cellSize + offsetY,
            0
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        float offsetX = (gridWidth - 1) * cellSize * 0.5f;
        float offsetY = (gridHeight - 1) * cellSize * 0.5f;

        int gx = Mathf.RoundToInt((localPos.x + offsetX) / cellSize);
        int gy = Mathf.RoundToInt((-localPos.y + offsetY) / cellSize);

        return new Vector2Int(gx, gy);
    }

    public bool CanPlace(OkazuData data, int gridX, int gridY, Okazu ignoreOkazu = null)
    {
        if (data == null) return false;

        var shape = data.GetShape();
        int w = data.Width;
        int h = data.Height;

        for (int dy = 0; dy < h; dy++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                if (!data.GetCell(dx, dy)) continue;

                int gx = gridX + dx;
                int gy = gridY + dy;

                if (gx < 0 || gx >= gridWidth || gy < 0 || gy >= gridHeight)
                    return false;

                if (grid[gx, gy] != null && grid[gx, gy] != ignoreOkazu)
                    return false;
            }
        }
        return true;
    }

    public bool Place(Okazu okazu, int gridX, int gridY)
    {
        if (!CanPlace(okazu.Data, gridX, gridY, okazu))
            return false;

        Remove(okazu);

        var data = okazu.Data;
        int w = data.Width;
        int h = data.Height;

        for (int dy = 0; dy < h; dy++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                if (!data.GetCell(dx, dy)) continue;
                grid[gridX + dx, gridY + dy] = okazu;
            }
        }

        okazu.GridPosition = new Vector2Int(gridX, gridY);
        okazu.CurrentBox = this;
        okazu.transform.position = GetWorldPosition(gridX, gridY);

        if (!placedOkazu.Contains(okazu))
            placedOkazu.Add(okazu);

        return true;
    }

    public void Remove(Okazu okazu)
    {
        if (okazu.CurrentBox != this) return;

        var data = okazu.Data;
        int gx = okazu.GridPosition.x;
        int gy = okazu.GridPosition.y;

        for (int dy = 0; dy < data.Height; dy++)
        {
            for (int dx = 0; dx < data.Width; dx++)
            {
                if (!data.GetCell(dx, dy)) continue;
                int x = gx + dx;
                int y = gy + dy;
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    if (grid[x, y] == okazu)
                        grid[x, y] = null;
                }
            }
        }

        placedOkazu.Remove(okazu);
        okazu.CurrentBox = null;
    }

    public void ShowPreview(OkazuData data, int gridX, int gridY, Okazu ignoreOkazu = null)
    {
        showingPreview = true;
        previewX = gridX;
        previewY = gridY;
        previewData = data;
        previewValid = CanPlace(data, gridX, gridY, ignoreOkazu);
    }

    public void HidePreview()
    {
        showingPreview = false;
        previewData = null;
    }

    public int PlacedCount => placedOkazu.Count;
    public List<Okazu> GetPlacedOkazu() => new List<Okazu>(placedOkazu);

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 pos = GetWorldPosition(x, y);
                Gizmos.DrawWireCube(pos, Vector3.one * cellSize * 0.95f);

                if (Application.isPlaying && grid != null && grid[x, y] != null)
                {
                    Gizmos.color = occupiedColor;
                    Gizmos.DrawCube(pos, Vector3.one * cellSize * 0.9f);
                    Gizmos.color = gridColor;
                }
            }
        }

        if (showingPreview && previewData != null)
        {
            Gizmos.color = previewValid ? validDropColor : invalidDropColor;
            for (int dy = 0; dy < previewData.Height; dy++)
            {
                for (int dx = 0; dx < previewData.Width; dx++)
                {
                    if (!previewData.GetCell(dx, dy)) continue;
                    Vector3 pos = GetWorldPosition(previewX + dx, previewY + dy);
                    Gizmos.DrawCube(pos, Vector3.one * cellSize * 0.85f);
                }
            }
        }
    }
}
