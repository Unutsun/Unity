using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Okazu : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private OkazuData data;

    [Header("State")]
    public Vector2Int GridPosition;
    public OsechiBox CurrentBox;

    private SpriteRenderer spriteRenderer;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private OsechiBox originalBox;
    private Vector2Int originalGridPos;

    private Camera mainCamera;
    private OsechiBox[] allBoxes;

    public OkazuData Data => data;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        allBoxes = FindObjectsByType<OsechiBox>(FindObjectsSortMode.None);
        UpdateVisual();
    }

    public void Initialize(OkazuData okazuData)
    {
        data = okazuData;
        data.ParseShape();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (data == null) return;

        if (data.sprite != null)
            spriteRenderer.sprite = data.sprite;
        spriteRenderer.color = data.color;

        float scale = CurrentBox != null ? CurrentBox.cellSize : 1f;
        transform.localScale = new Vector3(data.Width * scale, data.Height * scale, 1f);
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsCleared)
            return;

        isDragging = true;
        originalPosition = transform.position;
        originalBox = CurrentBox;
        originalGridPos = GridPosition;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        dragOffset = transform.position - mouseWorld;

        spriteRenderer.sortingOrder = 100;

        if (CurrentBox != null)
            CurrentBox.Remove(this);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        transform.position = mouseWorld + dragOffset;

        foreach (var box in allBoxes)
        {
            Vector2Int gridPos = box.WorldToGrid(transform.position);
            box.ShowPreview(data, gridPos.x, gridPos.y, this);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        spriteRenderer.sortingOrder = 0;

        foreach (var box in allBoxes)
            box.HidePreview();

        bool placed = false;
        foreach (var box in allBoxes)
        {
            Vector2Int gridPos = box.WorldToGrid(transform.position);
            if (box.CanPlace(data, gridPos.x, gridPos.y, this))
            {
                box.Place(this, gridPos.x, gridPos.y);
                placed = true;
                break;
            }
        }

        if (!placed)
        {
            if (originalBox != null && originalBox.CanPlace(data, originalGridPos.x, originalGridPos.y, this))
                originalBox.Place(this, originalGridPos.x, originalGridPos.y);
            else
                transform.position = originalPosition;
        }

        UpdateVisual();

        if (GameManager.Instance != null)
            GameManager.Instance.CheckClear();
    }
}
