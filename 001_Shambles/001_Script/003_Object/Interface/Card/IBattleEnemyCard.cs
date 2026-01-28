using UnityEngine;
// 적 카드 인터페이스
public interface IBattleEnemyCard : IBattleFactor {
	// 카드 소유자 상태
	IBattleStatus OwnerStatus { get; set; }
	// 카드 대상 상태
	BattleStatus TargetStatus { get; set; }
	// 카드 초기화
	void InitializeCardAction(IBattleStatus cardOwnerStatus);
	// 카드 사용 시 발동 (BattleStatus 대상)
	void ProceedCardAction(BattleStatus cardTargetStatus, int value);
	// 카드 사용 시 발동 (IBattleStatus 대상)
	void ProceedCardAction(IBattleStatus cardTargetStatus, int value);
}
