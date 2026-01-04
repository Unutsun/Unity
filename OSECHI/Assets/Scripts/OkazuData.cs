using UnityEngine;

[CreateAssetMenu(fileName = "NewOkazu", menuName = "OSECHI/Okazu Data")]
public class OkazuData : ScriptableObject
{
    [Header("Basic Info")]
    public string okazuName;
    public Sprite sprite;
    public Color color = Color.white;

    [Header("Shape (row by row, comma separated)")]
    [Tooltip("Example: '1,1,1' for horizontal 3-block")]
    public string[] shapeRows;

    private bool[,] _shape;
    private int _width;
    private int _height;

    public int Width => _width;
    public int Height => _height;

    public void ParseShape()
    {
        if (shapeRows == null || shapeRows.Length == 0)
        {
            _shape = new bool[1, 1] { { true } };
            _width = 1;
            _height = 1;
            return;
        }

        _height = shapeRows.Length;
        _width = 0;

        foreach (var row in shapeRows)
        {
            var cells = row.Split(',');
            if (cells.Length > _width) _width = cells.Length;
        }

        _shape = new bool[_height, _width];

        for (int y = 0; y < _height; y++)
        {
            var cells = shapeRows[y].Split(',');
            for (int x = 0; x < cells.Length; x++)
            {
                _shape[y, x] = cells[x].Trim() == "1";
            }
        }
    }

    public bool GetCell(int x, int y)
    {
        if (_shape == null) ParseShape();
        if (x < 0 || x >= _width || y < 0 || y >= _height) return false;
        return _shape[y, x];
    }

    public bool[,] GetShape()
    {
        if (_shape == null) ParseShape();
        return _shape;
    }

    private void OnValidate()
    {
        ParseShape();
    }
}
