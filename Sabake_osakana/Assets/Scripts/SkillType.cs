/// <summary>
/// スキルの種類（ローグライト風）
/// </summary>
public enum SkillType
{
    None = 0,

    // パドル系
    BigPaddle,          // パドルが大きくなる（1.5倍）

    // ナイフ系
    KnifeRebound,       // 落下中のナイフをパドルで押すと強く上に跳ね返る
    KnifeHoming,        // ナイフがブロックに向けて微妙に角度を変える
    ExtraKnife,         // サブ包丁の同時出現数+1

    // フィッシュ系
    SlowFish,           // 落ちてくる魚が遅くなる（0.7倍）

    // きりみ系
    GamingBoost,        // ゲーミングきりみの出現率アップ（5%→15%）
}
