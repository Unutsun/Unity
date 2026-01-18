using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// スキル管理（シングルトン）
/// ローグライト風のスキルシステム
/// リザルト画面で「つづける」を押すとスキル選択画面が表示される
/// </summary>
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("Settings")]
    public int skillChoiceCount = 3;          // 選択肢の数（6種類から3つ）

    [Header("Debug")]
    [SerializeField] private List<SkillType> activeSkills = new List<SkillType>();

    // スキル効果の値
    public float PaddleSizeMultiplier => HasSkill(SkillType.BigPaddle) ? 1.5f : 1f;
    public bool HasKnifeRebound => HasSkill(SkillType.KnifeRebound);
    public bool HasKnifeHoming => HasSkill(SkillType.KnifeHoming);
    public int ExtraKnifeCount => HasSkill(SkillType.ExtraKnife) ? 1 : 0;
    public float FishSpeedMultiplier => HasSkill(SkillType.SlowFish) ? 0.7f : 1f;
    public float GamingKirimiChance => HasSkill(SkillType.GamingBoost) ? 0.15f : 0.05f;

    // イベント
    public event System.Action<SkillData[]> OnSkillSelectionRequired;
    public event System.Action<SkillType> OnSkillAcquired;
    public event System.Action OnSkillSelectionComplete;  // スキル選択完了時（次のステージへ進む）

    private SkillData[] allSkills;
    private bool isWaitingForSelection = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            allSkills = SkillData.GetAllSkills();
            Debug.Log("[SkillManager] Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        GameEvents.OnReturnToTitle += ResetSkills;
    }

    void OnDisable()
    {
        GameEvents.OnReturnToTitle -= ResetSkills;
    }

    /// <summary>
    /// スキル選択画面を表示（リザルト画面のつづけるボタンから呼ばれる）
    /// </summary>
    public void ShowSkillSelection()
    {
        if (isWaitingForSelection)
        {
            Debug.LogWarning("[SkillManager] Already waiting for selection");
            return;
        }

        // まだ獲得していないスキルからランダムに選ぶ
        var availableSkills = allSkills
            .Where(s => !activeSkills.Contains(s.type))
            .ToList();

        if (availableSkills.Count == 0)
        {
            Debug.Log("[SkillManager] All skills already acquired! Proceeding to next stage.");
            OnSkillSelectionComplete?.Invoke();
            return;
        }

        // ランダムに3つ選ぶ（被りなし）
        var choices = new List<SkillData>();
        var shuffled = availableSkills.OrderBy(x => Random.value).ToList();
        int count = Mathf.Min(skillChoiceCount, shuffled.Count);

        for (int i = 0; i < count; i++)
        {
            choices.Add(shuffled[i]);
        }

        isWaitingForSelection = true;
        Debug.Log($"[SkillManager] Skill selection triggered! {count} choices from {availableSkills.Count} available");

        // UIに通知（Time.timeScaleは既に0のはず）
        OnSkillSelectionRequired?.Invoke(choices.ToArray());
    }

    /// <summary>
    /// スキルを選択（UIから呼ばれる）
    /// </summary>
    public void SelectSkill(SkillType skill)
    {
        if (!isWaitingForSelection) return;

        activeSkills.Add(skill);
        isWaitingForSelection = false;

        Debug.Log($"[SkillManager] Skill acquired: {skill}");

        // イベント発火
        OnSkillAcquired?.Invoke(skill);

        // スキル効果を即座に適用
        ApplySkillEffect(skill);

        // 選択完了→次のステージへ
        OnSkillSelectionComplete?.Invoke();
    }

    /// <summary>
    /// スキル効果を適用
    /// </summary>
    void ApplySkillEffect(SkillType skill)
    {
        switch (skill)
        {
            case SkillType.BigPaddle:
                Debug.Log("[SkillManager] BigPaddle effect will be applied");
                break;
            case SkillType.ExtraKnife:
                Debug.Log("[SkillManager] ExtraKnife effect will be applied");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// スキルを持っているかチェック
    /// </summary>
    public bool HasSkill(SkillType skill)
    {
        return activeSkills.Contains(skill);
    }

    /// <summary>
    /// アクティブなスキル一覧を取得
    /// </summary>
    public List<SkillType> GetActiveSkills()
    {
        return new List<SkillType>(activeSkills);
    }

    /// <summary>
    /// 獲得スキル数を取得
    /// </summary>
    public int GetSkillCount()
    {
        return activeSkills.Count;
    }

    /// <summary>
    /// スキルをリセット（タイトルに戻る時）
    /// </summary>
    void ResetSkills()
    {
        activeSkills.Clear();
        isWaitingForSelection = false;
        Debug.Log("[SkillManager] Skills reset");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
