// 피해 카드 인터페이스
public interface IBattleCardDamage {
	// 카드를 타겟에 드래그 했을 때 예상 피해량 표시용
	BattleDamage CurrentDamage { get; }
	// 최종 피해량 계산
	BattleDamage DamageCalc(int value);
}
