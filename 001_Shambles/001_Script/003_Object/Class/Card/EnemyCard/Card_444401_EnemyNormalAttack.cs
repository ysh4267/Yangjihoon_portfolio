using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_444401_EnemyNormalAttack : IBattleEnemyCard, IBattleEnemyCardDamage {
	// 카드 대상 상태
	public BattleStatus TargetStatus { get; set; }
	// 카드 소유자 상태
	public IBattleStatus OwnerStatus { get; set; }

	// 최종 피해량 계산
	public BattleDamage DamageCalc(int damage) {
		BattleDamage battleDamage = new BattleDamage(damage, ENUM_DAMAGE_TYPE.NORMAL, ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK, OwnerStatus);
		return BattleCardStaticMethod.BasicEnemyDamageCalc(OwnerStatus.DynamicValues, battleDamage);
	}

	// 카드 초기화
	public void InitializeCardAction(IBattleStatus cardOwnerStatus) {
		OwnerStatus = cardOwnerStatus;
	}

	// 카드 사용 시 대상에게 피해 적용 및 효과 재생
	public void ProceedCardAction(BattleStatus cardTargetStatus, int damage) {
		TargetStatus = cardTargetStatus;
		BattleManager.GetInstance().battlePhaseManager.ProceedPhase(OwnerStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.ATTACK);
		TargetStatus.Damage(this, BattleCardStaticMethod.BasicEnemyDamageAccuracyCalc(OwnerStatus.DynamicValues, DamageCalc(damage)));
		OwnerStatus.PlayEffect(ENUM_BATTLE_VFX.ENEMY_CREATURE_ATTACK);
	}

	// 단일 대상용 오버로드
	public void ProceedCardAction(IBattleStatus cardTargetStatus, int damage) {
		ProceedCardAction(BattleManager.GetInstance().GetBattleStatus(cardTargetStatus), damage);
	}
}
