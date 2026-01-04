using UnityEngine;

/// <summary>
/// パドルの移動を制御するコンポーネント
/// キーボード、マウス、タッチ入力に対応
/// </summary>
public class PaddleController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Bounds")]
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;

    [Header("Size")]
    [SerializeField] private float paddleWidth = 2f;

    private float targetX;
    private float currentVelocity;
    private Camera mainCamera;
    private bool useMouseControl = false;

    public float Width => paddleWidth;

    private void Awake()
    {
        mainCamera = Camera.main;
        targetX = transform.position.x;

        // スプライトからパドル幅を取得
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            paddleWidth = spriteRenderer.bounds.size.x;
        }
    }

    private void Update()
    {
        HandleInput();
        MovePaddle();
    }

    /// <summary>
    /// 入力を処理
    /// </summary>
    private void HandleInput()
    {
        // キーボード入力
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            useMouseControl = false;
            targetX += horizontalInput * moveSpeed * Time.deltaTime;
        }

        // マウス/タッチ入力
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Vector3 inputPosition;

            if (Input.touchCount > 0)
            {
                inputPosition = Input.GetTouch(0).position;
            }
            else
            {
                inputPosition = Input.mousePosition;
            }

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(inputPosition);
            targetX = worldPosition.x;
            useMouseControl = true;
        }

        // マウス移動（クリックなし）
        if (!Input.GetMouseButton(0) && Input.GetAxis("Mouse X") != 0)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // マウスがゲームエリア内にある場合のみ追従
            if (mouseWorldPos.x >= minX - 1f && mouseWorldPos.x <= maxX + 1f)
            {
                targetX = mouseWorldPos.x;
                useMouseControl = true;
            }
        }

        // 境界内に制限
        float halfWidth = paddleWidth / 2f;
        targetX = Mathf.Clamp(targetX, minX + halfWidth, maxX - halfWidth);
    }

    /// <summary>
    /// パドルを移動
    /// </summary>
    private void MovePaddle()
    {
        float newX;

        if (useMouseControl)
        {
            // マウス/タッチ時はスムーズに追従
            newX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocity, smoothTime);
        }
        else
        {
            // キーボード時はダイレクトに移動
            newX = targetX;
        }

        // 境界チェック
        float halfWidth = paddleWidth / 2f;
        newX = Mathf.Clamp(newX, minX + halfWidth, maxX - halfWidth);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    /// <summary>
    /// パドルをリセット
    /// </summary>
    public void ResetPosition()
    {
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
        targetX = 0;
        currentVelocity = 0;
    }

    /// <summary>
    /// 移動境界を設定
    /// </summary>
    public void SetBounds(float min, float max)
    {
        minX = min;
        maxX = max;
    }

    /// <summary>
    /// 移動速度を設定
    /// </summary>
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private void OnDrawGizmosSelected()
    {
        // エディタで境界を可視化
        Gizmos.color = Color.yellow;
        Vector3 leftBound = new Vector3(minX, transform.position.y, 0);
        Vector3 rightBound = new Vector3(maxX, transform.position.y, 0);
        Gizmos.DrawLine(leftBound + Vector3.up, leftBound + Vector3.down);
        Gizmos.DrawLine(rightBound + Vector3.up, rightBound + Vector3.down);
    }
}
