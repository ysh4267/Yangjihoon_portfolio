using UnityEngine;

public interface IBattleStatusAttributes {
    ENUM_BATTLE_PHASE_TARGET TargetEnum { get; }
    int[] Status { get; }
    int MaxHp { get; }
    int CurrentHp { get; }
    int MaxAp { get; }
    int CurrentAp { get; }
    int CurrentShield { get; }
    BattleDynamicValues DynamicValues { get; }
    CharacterBuffCounter BuffCounter { get; }
    Transform ObjectTransform { get; }
}
