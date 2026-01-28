using UnityEngine;

/// <summary>
/// 예제 버프 클래스
/// 능동형(Active)과 수동형(Passive) 버프 인터페이스를 모두 구현한 템플릿
/// </summary>
public class ExampleBuff : IBattleBuff, IBattleBuffActive, IBattleBuffPassive {
	#region 기본 속성
	// 버프 데이터 객체
	public Buff BuffObject { get; set; }
	// 버프/디버프 유형
	public ENUM_BUFF_TYPE EffectType => ENUM_BUFF_TYPE.BUFF;
	// 카운터 감소 방식 (턴 기반)
	public ENUM_BUFF_COUNTER_TYPE CounterType => ENUM_BUFF_COUNTER_TYPE.COUNT_BY_TURN;
	// 버프 지속 카운트
	public int ContinuousCount { get; set; }
	// 버프 고유 이름
	public string BuffName => "ExampleBuff";
	// 버프 대상 상태
	public IBattleStatus BuffTargetStatus { get; set; }
	#endregion

	#region 버프 동작
	// 버프 초기화
	public void InitializeBuff(IBattleStatus _targetStatus, Buff _buffObject, int _continuousCount, params int[] _params) {
		BuffTargetStatus = _targetStatus;
		BuffObject = _buffObject;
		ContinuousCount = _continuousCount;
	}

	// 버프 활성화 시 동작
	public void ActivateBuffEffect() {
		if (!BuffTargetStatus.BuffCounter.BuffExist(BuffName)) {
			// 페이즈 이펙트 등록 예시 (능동형 버프)
			BattleManager.GetInstance().battlePhaseManager.AddPhaseEffect(this, BuffTargetStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.TURN_START);
		}
		BuffTargetStatus.BuffCounter.AddBuff(BuffObject);

		// 이펙트 재생 예시
		if (BuffTargetStatus.TargetEnum == ENUM_BATTLE_PHASE_TARGET.PLAYER) {
			BattleEffectManager.PlayUIEffect(ENUM_BATTLE_VFX_UI.Buff_Line_Red);
		}
		else {
			BattleEffectManager.PlayEffect(ENUM_BATTLE_VFX.Buff_Red, BuffTargetStatus.ObjectTransform);
		}
	}

	// 버프 종료 시 동작
	public void EndBuffEffect() {
		// 페이즈 이펙트 해제 예시 (능동형 버프)
		BattleManager.GetInstance().battlePhaseManager.RemovePhaseEffectRequest(this, BuffTargetStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.TURN_START);
	}

	// 카운트 감소 및 만료 시 버프 제거
	public void SubtractCount() {
		if (--ContinuousCount <= 0)
			BuffTargetStatus.BuffCounter.RemoveBuff(BuffObject);
	}
	#endregion

	#region 능동형 버프 (IBattleBuffActive)
	// 특정 페이즈에서 발동되는 효과
	public void OnEffectPhase(ENUM_BATTLE_PHASE_ACTION action) {
		// 턴 시작 시 체력 회복 예시
		if (action == ENUM_BATTLE_PHASE_ACTION.TURN_START) {
			BuffTargetStatus.GainHP(this, 3);
		}
	}
	#endregion
}
