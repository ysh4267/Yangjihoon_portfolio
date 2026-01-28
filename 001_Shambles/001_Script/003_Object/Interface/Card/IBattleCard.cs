// 전투 카드 기본 인터페이스
public interface IBattleCard : IBattleFactor {
	// 카드 데이터 객체
	ICard ThisCard { get; set; }
	// 카드 소유자 상태
	IBattleStatus OwnerStatus { get; set; }
	// 카드 대상 상태
	BattleStatus TargetStatus { get; set; }
	// 타겟팅 필요 여부
	bool IsTargeting { get; }
	// 카드 초기화
	void InitializeCardAction(ICard card, IBattleStatus cardOwnerStatus = null);
	// 카드 사용 시 발동
	void ProceedCardAction(IBattleStatus cardTargetStatus = null);
	// 카드를 사용하거나 버린 후 발동
	void FinalizeCardAction();
}
