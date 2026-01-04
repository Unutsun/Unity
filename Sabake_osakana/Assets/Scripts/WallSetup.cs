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
    public float wallThickness = 1f;

    private float lastScreenWidth;
    private float lastScreenHeight;

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

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float halfWidth = camWidth / 2f;
        float halfHeight = camHeight / 2f;

        Debug.Log($"[WallSetup] Camera size: {camWidth}x{camHeight}, half: {halfWidth}x{halfHeight}");

        // 左壁
        if (leftWall != null)
        {
            leftWall.position = new Vector3(-halfWidth - wallThickness / 2f, 0, 0);
            leftWall.localScale = new Vector3(wallThickness, camHeight + wallThickness * 2, 1);
            SetupCollider(leftWall.gameObject);
            Debug.Log($"[WallSetup] LeftWall: pos={leftWall.position}, scale={leftWall.localScale}");
        }

        // 右壁
        if (rightWall != null)
        {
            rightWall.position = new Vector3(halfWidth + wallThickness / 2f, 0, 0);
            rightWall.localScale = new Vector3(wallThickness, camHeight + wallThickness * 2, 1);
            SetupCollider(rightWall.gameObject);
            Debug.Log($"[WallSetup] RightWall: pos={rightWall.position}, scale={rightWall.localScale}");
        }

        // 上壁
        if (topWall != null)
        {
            topWall.position = new Vector3(0, halfHeight + wallThickness / 2f, 0);
            topWall.localScale = new Vector3(camWidth + wallThickness * 2, wallThickness, 1);
            SetupCollider(topWall.gameObject);
            Debug.Log($"[WallSetup] TopWall: pos={topWall.position}, scale={topWall.localScale}");
        }

        // 下壁（DeathZone）
        if (bottomWall != null)
        {
            bottomWall.position = new Vector3(0, -halfHeight - wallThickness / 2f, 0);
            bottomWall.localScale = new Vector3(camWidth + wallThickness * 2, wallThickness, 1);
            SetupTrigger(bottomWall.gameObject);
            Debug.Log($"[WallSetup] BottomWall: pos={bottomWall.position}, scale={bottomWall.localScale}");
        }
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
