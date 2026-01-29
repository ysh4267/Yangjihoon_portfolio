using UnityEngine;

/// <summary>
/// 전투 개체의 읽기 전용 속성을 정의하는 인터페이스
/// 플레이어와 적 모두 이 인터페이스를 구현하여 공통된 속성에 접근 가능
/// BattleStatus 클래스에서 구현됨
/// </summary>
public interface IBattleStatusAttributes {
    ENUM_BATTLE_PHASE_TARGET TargetEnum { get; }     // 페이즈 시스템에서 대상을 식별하는 Enum 값
    int[] Status { get; }                             // 기본 스탯 배열 (힘, 민첩, 지능 등)
    int MaxHp { get; }                                // 최대 체력
    int CurrentHp { get; }                            // 현재 체력
    int MaxAp { get; }                                // 최대 행동력
    int CurrentAp { get; }                            // 현재 행동력
    int CurrentShield { get; }                        // 현재 방어도
    BattleDynamicValues DynamicValues { get; }        // 전투 중 동적으로 변하는 상태 값
    CharacterBuffCounter BuffCounter { get; }         // 버프/디버프 관리자
    Transform ObjectTransform { get; }                // 상위 오브젝트 Transform
}
