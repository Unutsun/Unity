using UnityEngine;

/// <summary>
/// スキルの情報を保持するクラス
/// </summary>
[System.Serializable]
public class SkillData
{
    public SkillType type;
    public string nameJP;
    public string description;
    public string icon;  // 絵文字アイコン

    public SkillData(SkillType type, string nameJP, string description, string icon)
    {
        this.type = type;
        this.nameJP = nameJP;
        this.description = description;
        this.icon = icon;
    }

    /// <summary>
    /// 全スキルデータを取得
    /// </summary>
    public static SkillData[] GetAllSkills()
    {
        return new SkillData[]
        {
            new SkillData(SkillType.BigPaddle, "でかパドル", "パドルが1.5倍大きくなる", "[大]"),
            new SkillData(SkillType.KnifeRebound, "跳ね返り", "落下ナイフがパドルで強く跳ね返る", "[跳]"),
            new SkillData(SkillType.KnifeHoming, "追尾", "ナイフがブロックに向かう", "[追]"),
            new SkillData(SkillType.ExtraKnife, "追加包丁", "サブ包丁が+1本", "[+]"),
            new SkillData(SkillType.SlowFish, "のろのろ魚", "魚の落下が遅くなる", "[遅]"),
            new SkillData(SkillType.GamingBoost, "虹きりみ", "ゲーミングきりみ出現率UP", "[虹]"),
        };
    }
}
