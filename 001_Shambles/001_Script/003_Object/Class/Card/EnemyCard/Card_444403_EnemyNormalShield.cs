using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_444403_EnemyNormalShield : IBattleEnemyCard, IBattleEnemyCardShield {
	// 카드 소유자 상태
	public IBattleStatus OwnerStatus { get; set; }
	// 카드 대상 상태
	public BattleStatus TargetStatus { get; set; }

	// 최종 방어력 계산
	public int ShieldCalc(int shield) {
		return BattleCardStaticMethod.BasicEnemyShieldCalc(OwnerStatus.DynamicValues, shield);
	}

	// 카드 초기화
	public void InitializeCardAction(IBattleStatus cardOwnerStatus) {
		OwnerStatus = cardOwnerStatus;
	}

	// 카드 사용 시 대상에게 방어력 부여
	public void ProceedCardAction(BattleStatus cardTargetStatus, int shield) {
		TargetStatus = cardTargetStatus;
		TargetStatus.GainShield(this, ShieldCalc(shield));
	}

	// 단일 대상용 오버로드
	public void ProceedCardAction(IBattleStatus cardTargetStatus, int shield) {
		ProceedCardAction(BattleManager.GetInstance().GetBattleStatus(cardTargetStatus), shield);
	}
}
