using UnityEngine;

// 전투 적 인터페이스
public interface IBattleEnemy {
	// 현재 패턴 인덱스
	int PatternIndex { get; set; }
	// 다음 패턴 타입
	ENUM_CARD_TYPE NextPatternTypeEnum { get; set; }
	// 해당 적 오브젝트
	BattleEnemyObject thisEnemyObject { get; set; }
	// 타겟 상태
	BattleStatus target { get; set; }
	// 적 행동 초기화
	void InitializeEnemyAction(BattleEnemyObject enemyObject);
	// 적 행동 실행
	void ProceedEnemyAction();
	// 다음 행동 UI 갱신
	void UpdateEnemyActionUI();
	// 공격 사운드
	AudioClip AttackSound { get; set; }
	// 보조 공격 사운드
	AudioClip Attack2Sound { get; set; }
	// 피격 사운드
	AudioClip HitSound { get; set; }
	// 사망 사운드
	AudioClip DeathSound { get; set; }
}
