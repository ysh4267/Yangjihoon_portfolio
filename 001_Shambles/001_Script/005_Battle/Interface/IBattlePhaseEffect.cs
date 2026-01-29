/// <summary>
/// 특정 페이즈에 행동을 실행하기 위한 옵저버 패턴 구독 인터페이스
/// BattlePhaseManager에 구독하여 특정 페이즈 진입 시 콜백을 받음
/// 버프, 장비 등 페이즈 기반 효과에서 구현
/// </summary>
public interface IBattlePhaseEffect {
    void OnEffectPhase(ENUM_BATTLE_PHASE_ACTION action);  // 특정 페이즈 진입 시 호출되는 콜백 메서드
}
