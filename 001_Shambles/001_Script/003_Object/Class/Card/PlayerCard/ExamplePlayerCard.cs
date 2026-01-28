using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 예제 플레이어 카드 클래스
/// 데미지, 힐, 실드, 버프 인터페이스를 모두 구현한 템플릿
/// </summary>
public class ExamplePlayerCard : IBattlePlayerCard, IBattleCardDamage, IBattleCardHeal, IBattleCardShield, IBattleCardBuff {
	#region 기본 속성
	// 타겟팅 필요 여부
	public bool IsTargeting => true;
	// 사용 가능 상태
	public bool IsUsable { get; set; }
	// 카드 세력
	public ENUM_FACTION FactionEnum { get; set; }
	// 카드 데이터 객체
	public ICard ThisCard { get; set; }
	// 카드 소유자 상태
	public IBattleStatus OwnerStatus { get; set; }
	// 카드 대상 상태
	public BattleStatus TargetStatus { get; set; }
	// 카드 코스트
	public BattleCost Cost { get; set; }
	// 카드 수치 데이터
	public BattleCardValues battleCardValues { get; set; }
	// 카드 사용 후 목적지
	public ENUM_CARD_DESTINATION cardDestination { get; set; }
	// 연계 장비 목록
	public List<Equipment> Equipments { get; set; }
	#endregion

	#region 피해 (IBattleCardDamage)
	// 계산된 현재 피해량
	public BattleDamage CurrentDamage => DamageCalc(battleCardValues.damageValues[(int)ENUM_CARD_ARRAY_INDEX.MAIN]);

	// 최종 피해량 계산
	public BattleDamage DamageCalc(int value) {
		BattleDamage battleDamage = new BattleDamage(value, ENUM_DAMAGE_TYPE.NORMAL, ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK, OwnerStatus, FactionEnum);
		battleDamage = BattleCardStaticMethod.BasicDamageCalc(this, battleDamage);
		return battleDamage;
	}
	#endregion

	#region 회복 (IBattleCardHeal)
	// 최종 회복량 계산
	public int HealCalc(int value) {
		return value;
	}
	#endregion

	#region 방어 (IBattleCardShield)
	// 최종 방어력 계산
	public int ShieldCalc(int value) {
		return value;
	}
	#endregion

	#region 버프 (IBattleCardBuff)
	// 버프 카운트 계산
	public int CountCalc(int value) {
		return value;
	}
	#endregion

	#region 코스트 계산
	// 최종 코스트 계산
	public int CostCalc() {
		return BattleCardStaticMethod.BasicCostCalc(this, Cost);
	}
	#endregion

	#region 카드 동작
	// 카드 초기화
	public void InitializeCardAction(ICard card, IBattleStatus cardOwnerStatus = null) {
		ThisCard = card;
		OwnerStatus = cardOwnerStatus;
	}

	// 카드 사용 시 동작
	public void ProceedCardAction(IBattleStatus cardTargetStatus = null) {
		TargetStatus = BattleManager.GetInstance().GetBattleStatus(cardTargetStatus);
		OwnerStatus.LoseAP(CostCalc());

		// 피해 적용 예시
		TargetStatus.Damage(this, CurrentDamage);

		// 회복 적용 예시
		OwnerStatus.GainHP(this, HealCalc(5));

		// 방어 적용 예시
		OwnerStatus.GainShield(this, ShieldCalc(3));

		// 버프 적용 예시
		TargetStatus.GainBuff(this, ENUM_BUFF_INDEX.INJURY, CountCalc(1));

		// SFX 예시
		SoundManager.GetInstance().PlayEffectSound(ENUM_BATTLE_EFFECT_SOUND.SFX_attack_hit);
		// VFX
		BattleEffectManager.PlayEffect(ENUM_BATTLE_VFX.VFX_attack_hit, cardTargetStatus.ObjectTransform);
	}

	// 카드 사용 종료 후 처리
	public void FinalizeCardAction() {

	}
	#endregion
}
