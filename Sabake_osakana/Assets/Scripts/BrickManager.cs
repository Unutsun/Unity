using UnityEngine;

/// <summary>
/// ブロックの生成と管理
/// 「さばけ！おさかな」対応版 - 魚の形にブロックを配置
/// ステージデータから魚画像・グリッドサイズを読み込む
/// </summary>
public class BrickManager : MonoBehaviour
{
    [Header("Brick Settings")]
    public float spacing = 0.05f;
    public bool autoCenter = true;     // 自動中央配置
    public bool autoAdjustToImage = true; // 画像に合わせてブロックサイズを調整

    // startPositionはautoCenterがfalseの場合のみ使用
    public Vector2 startPosition = new Vector2(-4f, 2.5f);

    [Header("Fish Colors (Fallback)")]
    public Color fishBodyColor = new Color(1f, 0.6f, 0.6f);    // 薄赤（魚の身）
    public Color fishBellyColor = new Color(1f, 0.85f, 0.85f); // 白っぽい（腹）

    [Header("Stage Settings")]
    public int stageId = 1;  // 現在のステージID（Inspector設定可能）

    [Header("Power-Up Settings")]
    public int minPowerUps = 4;
    public int maxPowerUps = 10;

    // ステージデータから読み込む値
    private float brickWidth = 0.9f;
    private float brickHeight = 0.9f;
    private int gridCols = 17;
    private int gridRows = 13;
    private string fishImagePath = "Sprites/sakana_normal";

    private GameObject brickPrefab;
    private GameObject bricksParent;
    private GameObject boneBackground;
    private Texture2D fishTexture;
    private Material brickMaterial;
    private Vector2 calculatedStartPos;

    // 魚の形状パターン（動的に生成）
    private int[,] fishPattern;

    // ブロックをグリッド位置で追跡
    private GameObject[,] brickGrid;
    private GameObject powerUpsParent;

    void Awake()
    {
        Debug.Log($"[BrickManager] Awake: Initializing");
    }

    void OnEnable()
    {
        Debug.Log($"[BrickManager] OnEnable: Registering event listeners");
        GameEvents.OnGameRestart += RebuildBricks;
    }

    void OnDisable()
    {
        Debug.Log($"[BrickManager] OnDisable: Unregistering event listeners");
        GameEvents.OnGameRestart -= RebuildBricks;
    }

    void Start()
    {
        Debug.Log($"[BrickManager] Start: Creating bricks for stage {stageId}");
        LoadStageData();
        InitializePattern();
        LoadFishTexture();
        AdjustBrickSizeToImage();
        CreateBrickPrefab();
        CalculateStartPosition();
        CreateBoneBackground();
        CreateFishBricks();
        SpawnPowerUps();
    }

    /// <summary>
    /// ステージデータを読み込む
    /// </summary>
    void LoadStageData()
    {
        StageData stage = null;

        // StageDataManagerがあれば使用
        if (StageDataManager.Instance != null)
        {
            stage = StageDataManager.Instance.GetStage(stageId);
        }

        if (stage != null)
        {
            Debug.Log($"[BrickManager] LoadStageData: Loading stage {stage}");
            fishImagePath = stage.fishImagePath;
            gridCols = stage.gridCols;
            gridRows = stage.gridRows;
            brickWidth = stage.brickWidth;

            // GameStateに制限時間を設定
            if (GameState.Instance != null)
            {
                GameState.Instance.SetTimeLimit(stage.timeLimit);
            }
        }
        else
        {
            Debug.Log("[BrickManager] LoadStageData: Using default values");
            // デフォルト値を使用（既にフィールドで設定済み）
        }
    }

    /// <summary>
    /// パターン配列を初期化
    /// </summary>
    void InitializePattern()
    {
        fishPattern = new int[gridRows, gridCols];
        brickGrid = new GameObject[gridRows, gridCols];
        // 初期値は全て0（画像アルファから生成される）
        Debug.Log($"[BrickManager] InitializePattern: Created {gridCols}x{gridRows} pattern array");
    }

    /// <summary>
    /// 別のステージを読み込んでブロックを再構築
    /// </summary>
    public void LoadStage(int newStageId)
    {
        stageId = newStageId;
        Debug.Log($"[BrickManager] LoadStage: Switching to stage {stageId}");

        // 既存のブロックを破棄
        if (bricksParent != null)
        {
            Destroy(bricksParent);
        }
        if (boneBackground != null)
        {
            Destroy(boneBackground);
        }

        // 新しいステージデータで再構築
        LoadStageData();
        InitializePattern();
        LoadFishTexture();
        AdjustBrickSizeToImage();
        CalculateStartPosition();
        CreateBoneBackground();
        CreateFishBricks();
    }

    void AdjustBrickSizeToImage()
    {
        if (!autoAdjustToImage || fishTexture == null) return;

        // 画像のアスペクト比
        float imageAspect = (float)fishTexture.width / fishTexture.height;

        // 画像全体が収まるようにブロックサイズを計算
        brickHeight = (gridCols * brickWidth) / (gridRows * imageAspect);

        Debug.Log($"[BrickManager] AdjustBrickSizeToImage: imageAspect={imageAspect:F2}, brickSize=({brickWidth}x{brickHeight:F2})");

        // 画像のアルファ値に基づいてパターンを再生成
        GeneratePatternFromImage();
    }

    void GeneratePatternFromImage()
    {
        if (fishTexture == null) return;

        float pixelWidth = (float)fishTexture.width / gridCols;
        float pixelHeight = (float)fishTexture.height / gridRows;

        // 各セルの中央付近のアルファ値をサンプリング
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                // 複数ポイントをサンプリングして平均アルファを計算
                float avgAlpha = SampleAverageAlpha(col, row, pixelWidth, pixelHeight);

                // アルファが一定以上ならブロックを配置
                fishPattern[row, col] = (avgAlpha > 0.3f) ? 1 : 0;
            }
        }

        Debug.Log($"[BrickManager] GeneratePatternFromImage: Pattern regenerated from image alpha");
    }

    float SampleAverageAlpha(int col, int row, float pixelWidth, float pixelHeight)
    {
        float totalAlpha = 0f;
        int sampleCount = 0;

        // セル内の複数ポイントをサンプリング
        for (float dx = 0.2f; dx <= 0.8f; dx += 0.3f)
        {
            for (float dy = 0.2f; dy <= 0.8f; dy += 0.3f)
            {
                int px = Mathf.RoundToInt((col + dx) * pixelWidth);
                int py = Mathf.RoundToInt((gridRows - 1 - row + dy) * pixelHeight);

                px = Mathf.Clamp(px, 0, fishTexture.width - 1);
                py = Mathf.Clamp(py, 0, fishTexture.height - 1);

                Color pixel = fishTexture.GetPixel(px, py);
                totalAlpha += pixel.a;
                sampleCount++;
            }
        }

        return totalAlpha / sampleCount;
    }

    void CalculateStartPosition()
    {
        float totalWidth = gridCols * (brickWidth + spacing) - spacing;
        float totalHeight = gridRows * (brickHeight + spacing) - spacing;

        if (autoCenter)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float centerX = -totalWidth / 2f;
                float centerY = totalHeight / 2f + 0.5f;
                calculatedStartPos = new Vector2(centerX, centerY);
                Debug.Log($"[BrickManager] CalculateStartPosition: fishSize=({totalWidth:F1}x{totalHeight:F1}), startPos={calculatedStartPos}");
                return;
            }
        }
        calculatedStartPos = startPosition;
    }

    void CreateBrickPrefab()
    {
        if (brickPrefab != null) return;  // 既に作成済みなら再作成しない

        Debug.Log($"[BrickManager] CreateBrickPrefab: Creating prefab");
        brickPrefab = new GameObject("BrickPrefab");
        brickPrefab.SetActive(false);

        SpriteRenderer sr = brickPrefab.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.sortingOrder = 10;

        BoxCollider2D col = brickPrefab.AddComponent<BoxCollider2D>();

        PhysicsMaterial2D mat = new PhysicsMaterial2D("BrickMat");
        mat.bounciness = 1f;
        mat.friction = 0f;
        col.sharedMaterial = mat;

        brickPrefab.AddComponent<BrickController>();
        DontDestroyOnLoad(brickPrefab);
    }

    Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    void CreateBoneBackground()
    {
        Debug.Log($"[BrickManager] CreateBoneBackground: Creating bone background");

        if (boneBackground != null)
        {
            Destroy(boneBackground);
        }

        boneBackground = new GameObject("BoneBackground");

        SpriteRenderer sr = boneBackground.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 0;

        float totalWidth = gridCols * (brickWidth + spacing) - spacing;
        float totalHeight = gridRows * (brickHeight + spacing) - spacing;

        // BONE画像をResourcesから読み込む
        Sprite boneSprite = Resources.Load<Sprite>("Sprites/bone_image");
        if (boneSprite != null)
        {
            sr.sprite = boneSprite;
            sr.color = Color.white;

            // 画像のアスペクト比を保持してフィット
            float imageAspect = (float)boneSprite.texture.width / boneSprite.texture.height;
            float targetAspect = totalWidth / totalHeight;

            float scaleX, scaleY;
            if (imageAspect > targetAspect)
            {
                // 画像が横長 → 幅に合わせる
                scaleX = totalWidth;
                scaleY = totalWidth / imageAspect;
            }
            else
            {
                // 画像が縦長 → 高さに合わせる
                scaleY = totalHeight;
                scaleX = totalHeight * imageAspect;
            }

            // ピクセル単位からワールド単位へ変換
            float pixelsPerUnit = boneSprite.pixelsPerUnit;
            boneBackground.transform.localScale = new Vector3(
                scaleX / (boneSprite.texture.width / pixelsPerUnit),
                scaleY / (boneSprite.texture.height / pixelsPerUnit),
                1
            );

            Debug.Log($"[BrickManager] Bone image loaded: {boneSprite.texture.width}x{boneSprite.texture.height}");
        }
        else
        {
            // フォールバック：従来のBONEテキスト表示
            sr.sprite = CreateSquareSprite();
            sr.color = GameColors.BoneBackground;
            boneBackground.transform.localScale = new Vector3(totalWidth * 0.9f, totalHeight * 0.9f, 1);

            GameObject boneText = new GameObject("BoneText");
            boneText.transform.parent = boneBackground.transform;
            boneText.transform.localPosition = Vector3.zero;

            var textMesh = boneText.AddComponent<TextMesh>();
            textMesh.text = "BONE";
            textMesh.fontSize = 48;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;
            textMesh.characterSize = 0.15f;

            Debug.LogWarning("[BrickManager] Bone image not found, using fallback text");
        }

        boneBackground.transform.position = new Vector3(
            calculatedStartPos.x + totalWidth / 2f,
            calculatedStartPos.y - totalHeight / 2f,
            1f
        );
    }

    void LoadFishTexture()
    {
        Debug.Log($"[BrickManager] LoadFishTexture: Loading {fishImagePath}");

        fishTexture = Resources.Load<Texture2D>(fishImagePath);
        if (fishTexture == null)
        {
            Debug.LogWarning($"[BrickManager] LoadFishTexture: {fishImagePath} not found in Resources/");
            return;
        }

        Shader shader = Shader.Find("Custom/BrickClipShader");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        brickMaterial = new Material(shader);
        brickMaterial.mainTexture = fishTexture;

        Debug.Log($"[BrickManager] LoadFishTexture: Texture loaded ({fishTexture.width}x{fishTexture.height})");
    }

    void CreateFishBricks()
    {
        Debug.Log($"[BrickManager] CreateFishBricks: Creating fish-shaped bricks");

        if (bricksParent != null)
        {
            Destroy(bricksParent);
        }

        bricksParent = new GameObject("FishBricks");
        int totalBricks = 0;

        Debug.Log($"[BrickManager] CreateFishBricks: Using startPos={calculatedStartPos}, brickSize=({brickWidth}x{brickHeight})");

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                if (fishPattern[row, col] == 1)
                {
                    float x = calculatedStartPos.x + col * (brickWidth + spacing);
                    float y = calculatedStartPos.y - row * (brickHeight + spacing);

                    GameObject brick = Instantiate(brickPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    brick.SetActive(true);
                    brick.name = $"Brick_{row}_{col}";
                    brick.transform.parent = bricksParent.transform;
                    brick.transform.localScale = new Vector3(brickWidth, brickHeight, 1);

                    // グリッドに登録
                    brickGrid[row, col] = brick;

                    SpriteRenderer sr = brick.GetComponent<SpriteRenderer>();
                    if (sr != null && fishTexture != null)
                    {
                        float pixelWidth = (float)fishTexture.width / gridCols;
                        float pixelHeight = (float)fishTexture.height / gridRows;
                        float pixelX = col * pixelWidth;
                        float pixelY = (gridRows - 1 - row) * pixelHeight;

                        Rect spriteRect = new Rect(pixelX, pixelY, pixelWidth, pixelHeight);
                        Vector2 pivot = new Vector2(0.5f, 0.5f);
                        float pixelsPerUnit = pixelWidth / brickWidth;
                        Sprite brickSprite = Sprite.Create(fishTexture, spriteRect, pivot, pixelsPerUnit);

                        sr.sprite = brickSprite;
                        sr.color = Color.white;
                        brick.transform.localScale = Vector3.one;
                    }
                    else if (sr != null)
                    {
                        // フォールバック：画像がない場合は色表示
                        bool isBelly = (row >= gridRows * 0.4f && row <= gridRows * 0.6f) &&
                                       (col >= gridCols * 0.5f && col <= gridCols * 0.8f);
                        sr.color = isBelly ? fishBellyColor : fishBodyColor;
                    }

                    totalBricks++;
                }
            }
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.SetTotalBricks(totalBricks);
        }

        Debug.Log($"[BrickManager] CreateFishBricks: Created {totalBricks} bricks in fish shape");
    }

    void RebuildBricks()
    {
        Debug.Log($"[BrickManager] RebuildBricks: Rebuilding all bricks");

        // パワーアップも削除
        if (powerUpsParent != null)
        {
            Destroy(powerUpsParent);
        }

        LoadStageData();
        InitializePattern();
        LoadFishTexture();
        AdjustBrickSizeToImage();
        CalculateStartPosition();
        CreateBoneBackground();
        CreateFishBricks();
        SpawnPowerUps();
    }

    /// <summary>
    /// ランダムな空き位置にパワーアップアイテムを配置
    /// ブロックの端から2マス以上離れた位置にのみ配置
    /// </summary>
    void SpawnPowerUps()
    {
        if (powerUpsParent != null)
        {
            Destroy(powerUpsParent);
        }
        powerUpsParent = new GameObject("PowerUps");

        // 空きマス（ブロックがない＆ブロックから2マス以上離れている位置）を収集
        System.Collections.Generic.List<Vector2Int> emptyPositions = new System.Collections.Generic.List<Vector2Int>();

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                // ブロックがない位置のみ
                if (fishPattern[row, col] == 0)
                {
                    // 周囲2マス以内にブロックがあるかチェック
                    if (!HasBrickNearby(row, col, 2))
                    {
                        emptyPositions.Add(new Vector2Int(row, col));
                    }
                }
            }
        }

        // ランダムに4〜10個配置
        int numPowerUps = Random.Range(minPowerUps, maxPowerUps + 1);
        numPowerUps = Mathf.Min(numPowerUps, emptyPositions.Count);

        // シャッフルして先頭から選択
        for (int i = emptyPositions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = emptyPositions[i];
            emptyPositions[i] = emptyPositions[j];
            emptyPositions[j] = temp;
        }

        for (int i = 0; i < numPowerUps; i++)
        {
            Vector2Int pos = emptyPositions[i];
            int row = pos.x;
            int col = pos.y;

            float x = calculatedStartPos.x + col * (brickWidth + spacing);
            float y = calculatedStartPos.y - row * (brickHeight + spacing);

            GameObject powerUp = new GameObject($"PowerUp_{row}_{col}");
            powerUp.transform.position = new Vector3(x, y, 0);
            powerUp.transform.localScale = new Vector3(brickWidth * 0.8f, brickHeight * 0.8f, 1);
            powerUp.transform.parent = powerUpsParent.transform;

            PowerUpItem item = powerUp.AddComponent<PowerUpItem>();
            item.gridRow = row;
            item.gridCol = col;
            item.brickManager = this;
        }

        Debug.Log($"[BrickManager] SpawnPowerUps: Spawned {numPowerUps} power-ups");
    }

    /// <summary>
    /// 指定位置の周囲4マス（上下左右）のブロックを破壊
    /// </summary>
    public void DestroyBricksAround(int row, int col)
    {
        int destroyed = 0;

        // 上下左右の4方向
        int[,] directions = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

        for (int d = 0; d < 4; d++)
        {
            int newRow = row + directions[d, 0];
            int newCol = col + directions[d, 1];

            // 範囲チェック
            if (newRow >= 0 && newRow < gridRows && newCol >= 0 && newCol < gridCols)
            {
                GameObject brick = brickGrid[newRow, newCol];
                if (brick != null)
                {
                    // ブロックを破壊
                    Destroy(brick);
                    brickGrid[newRow, newCol] = null;
                    destroyed++;

                    // スコア更新
                    if (GameState.Instance != null)
                    {
                        GameState.Instance.AddKirimi(1);
                        GameState.Instance.AddDestroyedBricks(1);
                    }

                    // イベント発火
                    GameEvents.TriggerBrickDestroyed(1);
                }
            }
        }

        Debug.Log($"[BrickManager] DestroyBricksAround: Destroyed {destroyed} bricks around ({row}, {col})");
    }

    /// <summary>
    /// 指定位置の周囲にブロックがあるかチェック
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="col">列</param>
    /// <param name="distance">チェックする距離（マス数）</param>
    /// <returns>周囲にブロックがあればtrue</returns>
    bool HasBrickNearby(int row, int col, int distance)
    {
        for (int dr = -distance; dr <= distance; dr++)
        {
            for (int dc = -distance; dc <= distance; dc++)
            {
                int checkRow = row + dr;
                int checkCol = col + dc;

                // 範囲外はスキップ
                if (checkRow < 0 || checkRow >= gridRows || checkCol < 0 || checkCol >= gridCols)
                    continue;

                // 自分自身はスキップ
                if (dr == 0 && dc == 0)
                    continue;

                // ブロックがあればtrue
                if (fishPattern[checkRow, checkCol] == 1)
                    return true;
            }
        }
        return false;
    }
}
