using UnityEngine;

/// <summary>
/// コンボシステム（桜井理論：リスクとリターン）
/// 壁に当たらずにブロックを連続破壊するとコンボ倍率UP
/// </summary>
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [Header("Combo Settings")]
    public float combo2Multiplier = 1.5f;   // 2コンボ
    public float combo3Multiplier = 2.0f;   // 3コンボ
    public float combo5Multiplier = 3.0f;   // 5コンボ以上
    public float comboDisplayTime = 1.5f;   // コンボ表示時間

    [Header("Debug")]
    [SerializeField] private int currentCombo = 0;
    [SerializeField] private float currentMultiplier = 1f;

    // イベント
    public event System.Action<int, float> OnComboChanged;  // (コンボ数, 倍率)
    public event System.Action OnComboReset;

    private float lastBrickTime = 0f;
    private bool hasHitWallSinceLastBrick = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ComboManager] Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        GameEvents.OnBrickDestroyed += OnBrickDestroyed;
        GameEvents.OnBallLost += ResetCombo;
        GameEvents.OnReturnToTitle += ResetCombo;
        GameEvents.OnGameRestart += ResetCombo;
    }

    void OnDisable()
    {
        GameEvents.OnBrickDestroyed -= OnBrickDestroyed;
        GameEvents.OnBallLost -= ResetCombo;
        GameEvents.OnReturnToTitle -= ResetCombo;
        GameEvents.OnGameRestart -= ResetCombo;
    }

    /// <summary>
    /// 壁に当たった時に呼び出す（BallControllerから）
    /// </summary>
    public void OnWallHit()
    {
        hasHitWallSinceLastBrick = true;
    }

    /// <summary>
    /// ブロック破壊時
    /// </summary>
    void OnBrickDestroyed(int points)
    {
        if (hasHitWallSinceLastBrick)
        {
            // 壁に当たった後のブロック破壊 → コンボリセット、新規開始
            if (currentCombo > 1)
            {
                Debug.Log($"[ComboManager] Combo reset (hit wall). Previous: {currentCombo}");
                OnComboReset?.Invoke();
            }
            currentCombo = 1;
            hasHitWallSinceLastBrick = false;
        }
        else
        {
            // 壁に当たらずブロック破壊 → コンボ継続！
            currentCombo++;
        }

        lastBrickTime = Time.time;
        UpdateMultiplier();

        if (currentCombo >= 2)
        {
            Debug.Log($"[ComboManager] Combo: {currentCombo}x, Multiplier: {currentMultiplier}");
            OnComboChanged?.Invoke(currentCombo, currentMultiplier);
        }
    }

    void UpdateMultiplier()
    {
        if (currentCombo >= 5)
        {
            currentMultiplier = combo5Multiplier;
        }
        else if (currentCombo >= 3)
        {
            currentMultiplier = combo3Multiplier;
        }
        else if (currentCombo >= 2)
        {
            currentMultiplier = combo2Multiplier;
        }
        else
        {
            currentMultiplier = 1f;
        }
    }

    /// <summary>
    /// 現在のコンボ倍率を取得
    /// </summary>
    public float GetCurrentMultiplier()
    {
        return currentMultiplier;
    }

    /// <summary>
    /// 現在のコンボ数を取得
    /// </summary>
    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    /// <summary>
    /// コンボをリセット
    /// </summary>
    public void ResetCombo()
    {
        if (currentCombo > 1)
        {
            Debug.Log($"[ComboManager] Combo ended: {currentCombo}");
            OnComboReset?.Invoke();
        }
        currentCombo = 0;
        currentMultiplier = 1f;
        hasHitWallSinceLastBrick = false;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
