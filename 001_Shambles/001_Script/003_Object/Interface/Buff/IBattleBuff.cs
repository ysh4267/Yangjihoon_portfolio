using System.Collections.Generic;

// 전투 버프 기본 인터페이스
public interface IBattleBuff : IBattleFactor {
	// 버프 데이터 객체
	Buff BuffObject { get; set; }
	// 버프 대상 상태
	IBattleStatus BuffTargetStatus { get; set; }
	// 버프/디버프 유형
	ENUM_BUFF_TYPE EffectType { get; }
	// 카운터 감소 방식
	ENUM_BUFF_COUNTER_TYPE CounterType { get; }
	// 버프 지속 카운트
	int ContinuousCount { get; set; }
	// 버프 고유 이름
	string BuffName { get; }
	// 버프 초기화
	void InitializeBuff(IBattleStatus _targetStatus, Buff _buffObject, int _continuousCount, params int[] _params);
	// 버프 활성화 시 발동
	void ActivateBuffEffect();
	// 카운트 감소
	void SubtractCount();
	// 버프 종료 시 발동 (BuffCounter에서만 호출)
	void EndBuffEffect();
}