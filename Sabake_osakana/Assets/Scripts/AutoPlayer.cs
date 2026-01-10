using UnityEngine;

/// <summary>
/// AI自動プレイヤー
/// ボールを追従してパドルを操作
/// バッチモード実行時に自動でゲームをプレイ
/// </summary>
public class AutoPlayer : MonoBehaviour
{
    public static AutoPlayer Instance { get; private set; }

    [Header("AI Settings")]
    [Tooltip("AIの反応速度（0-1、1で完璧）")]
    public float reactionSpeed = 0.8f;

    [Tooltip("予測精度のランダム誤差")]
    public float predictionError = 0.5f;

    [Tooltip("自動発射の遅延（秒）")]
    public float launchDelay = 0.5f;

    [Header("Debug")]
    public bool enableAutoPlay = true;
    public bool showDebugInfo = false;

    private Transform ball;
    private Transform paddle;
    private Rigidbody2D ballRb;
    private Rigidbody2D paddleRb;
    private float targetX;
    private float launchTimer = 0f;
    private bool waitingToLaunch = false;
    private Camera mainCamera;
    private float minX, maxX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // バッチモードなら自動で有効化
        if (Application.isBatchMode)
        {
            enableAutoPlay = true;
            Debug.Log("[AutoPlayer] Batch mode detected, auto-play enabled");
        }

        FindGameObjects();
        SetupBounds();
    }

    void FindGameObjects()
    {
        var ballObj = GameObject.Find("Ball");
        if (ballObj != null)
        {
            ball = ballObj.transform;
            ballRb = ballObj.GetComponent<Rigidbody2D>();
        }

        var paddleObj = GameObject.Find("Paddle");
        if (paddleObj != null)
        {
            paddle = paddleObj.transform;
            paddleRb = paddleObj.GetComponent<Rigidbody2D>();
        }

        mainCamera = Camera.main;

        Debug.Log($"[AutoPlayer] Found - Ball: {ball != null}, Paddle: {paddle != null}");
    }

    void SetupBounds()
    {
        if (mainCamera == null) return;

        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;
        float halfWidth = camWidth / 2f;
        float paddleHalfWidth = paddle != null ? paddle.localScale.x / 2f : 1.5f;

        minX = -halfWidth + paddleHalfWidth;
        maxX = halfWidth - paddleHalfWidth;
    }

    void Update()
    {
        if (!enableAutoPlay) return;

        // オブジェクトが見つからなければ再検索
        if (ball == null || paddle == null)
        {
            FindGameObjects();
            return;
        }

        // ゲーム状態チェック
        if (GameState.Instance == null) return;

        var state = GameState.Instance.CurrentState;

        switch (state)
        {
            case GameStateType.Ready:
                HandleReadyState();
                break;
            case GameStateType.Playing:
                HandlePlayingState();
                break;
            case GameStateType.GameClear:
            case GameStateType.GameOver:
                HandleGameEndState();
                break;
        }
    }

    void HandleReadyState()
    {
        // 発射待ち
        if (!waitingToLaunch)
        {
            waitingToLaunch = true;
            launchTimer = launchDelay;
            Debug.Log("[AutoPlayer] Waiting to launch...");
        }

        launchTimer -= Time.deltaTime;
        if (launchTimer <= 0)
        {
            // スペースキー入力をシミュレート
            SimulateLaunch();
            waitingToLaunch = false;
        }
    }

    void SimulateLaunch()
    {
        // BallControllerのLaunchを直接呼び出す代わりに、
        // 実際のゲームと同じ方法でボールを発射
        var ballController = ball.GetComponent<BallController>();
        if (ballController != null)
        {
            // リフレクションでprivateメソッドを呼び出す
            var launchMethod = typeof(BallController).GetMethod("Launch",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (launchMethod != null)
            {
                launchMethod.Invoke(ballController, null);
                Debug.Log("[AutoPlayer] Ball launched via reflection");
            }
        }
    }

    void HandlePlayingState()
    {
        waitingToLaunch = false;

        if (ballRb == null || paddle == null) return;

        // ボールの位置と速度から着地点を予測
        Vector2 ballPos = ball.position;
        Vector2 ballVel = ballRb.linearVelocity;

        // ボールが下に向かっている場合のみ追従
        if (ballVel.y < 0)
        {
            float predictedX = PredictLandingX(ballPos, ballVel);

            // 予測誤差を追加（人間らしさ）
            predictedX += Random.Range(-predictionError, predictionError);

            // 範囲内にクランプ
            targetX = Mathf.Clamp(predictedX, minX, maxX);
        }
        else
        {
            // ボールが上に向かっている間は中央寄りに待機
            targetX = Mathf.Lerp(targetX, 0f, Time.deltaTime * 2f);
        }

        // パドルを移動
        MovePaddle();

        if (showDebugInfo)
        {
            Debug.Log($"[AutoPlayer] Ball: {ballPos}, Vel: {ballVel}, Target: {targetX}");
        }
    }

    float PredictLandingX(Vector2 ballPos, Vector2 ballVel)
    {
        if (paddle == null || Mathf.Abs(ballVel.y) < 0.1f) return ballPos.x;

        // パドルのY位置
        float paddleY = paddle.position.y + 0.5f; // パドル上端

        // ボールがパドルに到達するまでの時間
        float timeToReach = (ballPos.y - paddleY) / (-ballVel.y);
        if (timeToReach < 0) return ballPos.x;

        // 予測X位置
        float predictedX = ballPos.x + ballVel.x * timeToReach;

        // 壁での反射を考慮（簡易版）
        float leftWall = minX - 0.5f;
        float rightWall = maxX + 0.5f;

        while (predictedX < leftWall || predictedX > rightWall)
        {
            if (predictedX < leftWall)
            {
                predictedX = leftWall + (leftWall - predictedX);
            }
            else if (predictedX > rightWall)
            {
                predictedX = rightWall - (predictedX - rightWall);
            }
        }

        return predictedX;
    }

    void MovePaddle()
    {
        if (paddleRb == null) return;

        float currentX = paddle.position.x;
        float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * 10f * reactionSpeed);
        newX = Mathf.Clamp(newX, minX, maxX);

        paddleRb.MovePosition(new Vector2(newX, paddle.position.y));
    }

    void HandleGameEndState()
    {
        // ゲーム終了時の処理
        if (Application.isBatchMode)
        {
            Debug.Log("[AutoPlayer] Game ended in batch mode, quitting...");

            // ログを保存してから終了
            if (SimulationLogger.Instance != null)
            {
                SimulationLogger.Instance.SaveLog();
            }

            // 少し待ってから終了
            Invoke("QuitApplication", 1f);
        }
    }

    void QuitApplication()
    {
        Debug.Log("[AutoPlayer] Quitting application...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void OnGUI()
    {
        if (!showDebugInfo || !enableAutoPlay) return;

        GUI.Label(new Rect(10, 10, 300, 20), $"AutoPlayer: ON");
        GUI.Label(new Rect(10, 30, 300, 20), $"Target X: {targetX:F2}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Ball: {(ball != null ? ball.position.ToString() : "null")}");
    }
}
