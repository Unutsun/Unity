using UnityEngine;

/// <summary>
/// きりみのスポーン管理
/// ブロック破壊時やパワーアップ取得時にきりみをスポーン
/// </summary>
public class KirimiSpawner : MonoBehaviour
{
    public static KirimiSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public int spawnCount = 3;           // 1回にスポーンする数
    public float gamingChance = 0.05f;   // ゲーミングきりみの確率（5%）

    [Header("Launch Settings")]
    public float launchSpeedMin = 3f;
    public float launchSpeedMax = 5f;
    public float spreadAngle = 60f;      // 扇状に広がる角度

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[KirimiSpawner] Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定位置にきりみをスポーン
    /// </summary>
    public void SpawnKirimi(Vector3 position, int count = -1)
    {
        if (count < 0) count = spawnCount;

        float baseAngle = 90f;  // 上方向を基準
        float angleStep = count > 1 ? spreadAngle / (count - 1) : 0f;
        float startAngle = baseAngle - spreadAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            // ゲーミングきりみ判定（スキル効果: GamingBoost）
            float actualGamingChance = SkillManager.Instance != null
                ? SkillManager.Instance.GamingKirimiChance
                : gamingChance;
            bool isGaming = Random.value < actualGamingChance;

            // スポーン
            GameObject kirimiObj = new GameObject(isGaming ? "GamingKirimi" : "FallingKirimi");
            kirimiObj.transform.position = position;

            FallingKirimi kirimi = kirimiObj.AddComponent<FallingKirimi>();
            kirimi.isGaming = isGaming;

            // 発射角度計算（扇状に広がる）
            float angle = count > 1 ? startAngle + angleStep * i : baseAngle;
            // ランダムな揺らぎを追加
            angle += Random.Range(-10f, 10f);

            float speed = Random.Range(launchSpeedMin, launchSpeedMax);
            Vector2 velocity = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * speed,
                Mathf.Sin(angle * Mathf.Deg2Rad) * speed
            );

            kirimi.Launch(velocity);

            if (isGaming)
            {
                Debug.Log($"[KirimiSpawner] Gaming kirimi spawned at {position}!");
            }
        }

        Debug.Log($"[KirimiSpawner] Spawned {count} kirimi at {position}");
    }

    /// <summary>
    /// ブロック破壊イベント用（位置情報付き）
    /// </summary>
    public void SpawnAtBrickPosition(Vector3 brickPosition)
    {
        SpawnKirimi(brickPosition, spawnCount);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
