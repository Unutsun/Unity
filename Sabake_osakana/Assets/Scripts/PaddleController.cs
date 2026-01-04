using UnityEngine;

/// <summary>
/// パドルの操作を制御
/// Rigidbody2Dを使用して物理的な衝突を正しく処理
/// </summary>
public class PaddleController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    public float minX = -7f;
    public float maxX = 7f;

    [Header("Initial Position")]
    public float initialYOffset = -0.8f;  // 画面下端からのオフセット（正の値で上方向）

    private Camera mainCamera;
    private Rigidbody2D rb;
    private float targetX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        // Kinematicに設定して物理演算の影響を受けないようにする
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        mainCamera = Camera.main;
        SetupInitialPosition();
        targetX = transform.position.x;
        SetupSprite();
        SetupCollider();
        SetupMovementBounds();
    }

    void SetupInitialPosition()
    {
        if (mainCamera == null) return;

        // 画面下端からオフセットを加えた位置に配置
        float bottomY = -mainCamera.orthographicSize;
        float paddleY = bottomY + Mathf.Abs(initialYOffset) + 0.5f;  // 0.5はパドル高さの半分
        transform.position = new Vector3(0, paddleY, 0);

        Debug.Log($"[PaddleController] Initial position set: Y={paddleY} (bottom={bottomY}, offset={initialYOffset})");
    }

    void SetupMovementBounds()
    {
        if (mainCamera == null) return;

        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;
        float halfWidth = camWidth / 2f;

        // パドルの半分の幅を考慮して可動域を設定
        float paddleHalfWidth = transform.localScale.x / 2f;
        minX = -halfWidth + paddleHalfWidth;
        maxX = halfWidth - paddleHalfWidth;

        Debug.Log($"[PaddleController] Movement bounds: minX={minX}, maxX={maxX}, camWidth={camWidth}");
    }

    void SetupCollider()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }

        // パドルのサイズを設定（localScaleに合わせる）
        Vector3 scale = transform.localScale;
        if (scale.x < 0.1f || scale.y < 0.1f)
        {
            // デフォルトサイズ: 幅3.0、高さ0.5（大きめに調整）
            transform.localScale = new Vector3(3.0f, 0.5f, 1f);
            scale = transform.localScale;
        }

        // コライダーサイズはlocalScaleを考慮して1x1に設定
        // (localScaleがコライダーサイズに適用されるため)
        col.size = new Vector2(1f, 1f);

        // Physics Material 2D を設定
        PhysicsMaterial2D mat = new PhysicsMaterial2D("PaddleMat");
        mat.bounciness = 1f;
        mat.friction = 0f;
        col.sharedMaterial = mat;

        Debug.Log($"[PaddleController] Collider setup: scale={transform.localScale}, colSize={col.size}, bounds={col.bounds}");
    }

    void SetupSprite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            sr.color = new Color(0.4f, 0.49f, 0.92f); // 青いパドル
        }
    }

    void Update()
    {
        // ゲーム状態チェック
        if (GameState.Instance != null)
        {
            var state = GameState.Instance.CurrentState;
            if (state == GameStateType.GameOver || state == GameStateType.GameClear)
            {
                return;
            }
        }

        HandleInput();
    }

    void FixedUpdate()
    {
        // FixedUpdateでRigidbody2Dの位置を更新
        Vector2 newPos = new Vector2(targetX, rb.position.y);
        rb.MovePosition(newPos);
    }

    void HandleInput()
    {
        // マウス/タッチ入力
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Vector3 inputPos = Input.touchCount > 0
                ? (Vector3)Input.GetTouch(0).position
                : Input.mousePosition;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPos);
            targetX = Mathf.Clamp(worldPos.x, minX, maxX);
            return;
        }

        // キーボード入力
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
        }

        if (moveInput != 0f)
        {
            targetX += moveInput * speed * Time.deltaTime;
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }
    }
}
