using UnityEngine;

/// <summary>
/// カメラに合わせて壁を自動配置
/// </summary>
public class WallSetup : MonoBehaviour
{
    [Header("Wall References")]
    public Transform leftWall;
    public Transform rightWall;
    public Transform topWall;
    public Transform bottomWall;

    [Header("Settings")]
    public float wallThickness = 0.15f;  // 薄い縦線
    public float leftMargin = 6.7f;      // 左側のマージン（きりみテキストの右に余裕を持たせる）
    public float rightMargin = 6.7f;     // 右側のマージン（左右対称でフィールド中央配置）
    public bool showVisibleWalls = true; // 壁を可視化

    private float lastScreenWidth;
    private float lastScreenHeight;

    // ボーナスタイム用（全ブロック破壊後）
    private bool isBonusTime = false;
    private float hue = 0f;
    private SpriteRenderer leftWallRenderer;
    private SpriteRenderer rightWallRenderer;

    void OnEnable()
    {
        GameEvents.OnAllBricksDestroyed += StartBonusTime;
    }

    void OnDisable()
    {
        GameEvents.OnAllBricksDestroyed -= StartBonusTime;
    }

    void StartBonusTime()
    {
        isBonusTime = true;
        Debug.Log("[WallSetup] BONUS TIME! Walls will glow rainbow!");
    }

    void Start()
    {
        FindWalls();
        SetupWalls();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    void Update()
    {
        // 画面サイズ変更を検知して壁を再配置
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            Debug.Log($"[WallSetup] Screen resized: {lastScreenWidth}x{lastScreenHeight} -> {Screen.width}x{Screen.height}");
            SetupWalls();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        // ボーナスタイム中は壁が虹色に光る
        if (isBonusTime)
        {
            hue += Time.deltaTime * 0.5f;  // ゆっくり色が変わる
            if (hue > 1f) hue -= 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 0.8f, 1f);

            if (leftWallRenderer != null) leftWallRenderer.color = rainbowColor;
            if (rightWallRenderer != null) rightWallRenderer.color = rainbowColor;
        }
    }

    void FindWalls()
    {
        if (leftWall == null)
            leftWall = GameObject.Find("LeftWall")?.transform;
        if (rightWall == null)
            rightWall = GameObject.Find("RightWall")?.transform;
        if (topWall == null)
            topWall = GameObject.Find("TopWall")?.transform;
        if (bottomWall == null)
            bottomWall = GameObject.Find("BottomWall")?.transform;

        Debug.Log($"[WallSetup] FindWalls: L={leftWall != null}, R={rightWall != null}, T={topWall != null}, B={bottomWall != null}");
    }

    void SetupWalls()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[WallSetup] Main camera not found!");
            return;
        }

        // デバッグ：実際の設定値を出力
        Debug.Log($"[WallSetup] Settings: wallThickness={wallThickness}, leftMargin={leftMargin}, rightMargin={rightMargin}");

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float halfWidth = camWidth / 2f;
        float halfHeight = camHeight / 2f;

        // マージンを適用してフィールドを狭める
        float fieldLeft = -halfWidth + leftMargin;
        float fieldRight = halfWidth - rightMargin;
        float fieldWidth = fieldRight - fieldLeft;

        Debug.Log($"[WallSetup] Camera size: {camWidth}x{camHeight}, Field: {fieldLeft} to {fieldRight} (width={fieldWidth})");

        // 左壁（縦線）
        if (leftWall != null)
        {
            leftWall.position = new Vector3(fieldLeft - wallThickness / 2f, 0, 0);
            leftWall.localScale = new Vector3(wallThickness, camHeight + wallThickness * 2, 1);
            SetupCollider(leftWall.gameObject);
            SetupWallVisual(leftWall.gameObject, true);
            Debug.Log($"[WallSetup] LeftWall: pos={leftWall.position}, scale={leftWall.localScale}");
        }

        // 右壁（縦線）
        if (rightWall != null)
        {
            rightWall.position = new Vector3(fieldRight + wallThickness / 2f, 0, 0);
            rightWall.localScale = new Vector3(wallThickness, camHeight + wallThickness * 2, 1);
            SetupCollider(rightWall.gameObject);
            SetupWallVisual(rightWall.gameObject, false);
            Debug.Log($"[WallSetup] RightWall: pos={rightWall.position}, scale={rightWall.localScale}");
        }

        // 上壁
        if (topWall != null)
        {
            topWall.position = new Vector3((fieldLeft + fieldRight) / 2f, halfHeight + wallThickness / 2f, 0);
            topWall.localScale = new Vector3(fieldWidth + wallThickness * 2, wallThickness, 1);
            SetupCollider(topWall.gameObject);
            Debug.Log($"[WallSetup] TopWall: pos={topWall.position}, scale={topWall.localScale}");
        }

        // 下壁（DeathZone）
        if (bottomWall != null)
        {
            bottomWall.position = new Vector3((fieldLeft + fieldRight) / 2f, -halfHeight - wallThickness / 2f, 0);
            bottomWall.localScale = new Vector3(fieldWidth + wallThickness * 2, wallThickness, 1);
            SetupTrigger(bottomWall.gameObject);
            Debug.Log($"[WallSetup] BottomWall: pos={bottomWall.position}, scale={bottomWall.localScale}");
        }
    }

    void SetupWallVisual(GameObject wall, bool isLeft)
    {
        if (!showVisibleWalls) return;

        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = wall.AddComponent<SpriteRenderer>();
            // 1x1の白いテクスチャを作成
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
        // まないた色の縦線
        sr.color = GameColors.Manaita;
        sr.sortingOrder = 10;

        // レンダラー参照を保存（フィーバー用）
        if (isLeft)
            leftWallRenderer = sr;
        else
            rightWallRenderer = sr;
    }

    void SetupCollider(GameObject wall)
    {
        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = wall.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = false;

        // Physics Material
        PhysicsMaterial2D mat = new PhysicsMaterial2D("WallMat");
        mat.bounciness = 1f;
        mat.friction = 0f;
        col.sharedMaterial = mat;
    }

    void SetupTrigger(GameObject wall)
    {
        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = wall.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;  // 下壁はトリガー（ボール落下検知）
    }
}
