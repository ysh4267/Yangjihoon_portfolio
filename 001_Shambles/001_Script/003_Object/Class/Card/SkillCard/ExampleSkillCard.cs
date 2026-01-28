using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 예제 스킬 카드 클래스
/// 데미지, 힐, 실드, 버프 인터페이스를 모두 구현한 템플릿
/// </summary>
public class ExampleSkillCard : IBattleSkill, IBattleCardDamage, IBattleCardHeal, IBattleCardShield, IBattleCardBuff {
	#region 기본 속성
	// 타겟팅 필요 여부
	public bool IsTargeting => false;
	// 현재 사용 가능 횟수
	public int CurrentCount { set; get; }
	// 최대 사용 가능 횟수
	public int UsableCount => 2;
	// 스킬 데이터 객체
	public ICard ThisCard { get; set; }
	// 스킬 소유자 상태
	public IBattleStatus OwnerStatus { get; set; }
	// 스킬 대상 상태
	public BattleStatus TargetStatus { get; set; }
	// 사용 가능 상태
	public bool IsUsable { get => true; set { } }
	#endregion

	#region 피해 (IBattleCardDamage)
	// 계산된 현재 피해량
	public BattleDamage CurrentDamage => DamageCalc(10);

	// 최종 피해량 계산
	public BattleDamage DamageCalc(int value) {
		BattleDamage battleDamage = new BattleDamage(value, ENUM_DAMAGE_TYPE.NORMAL, ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK, OwnerStatus);
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

	#region 스킬 동작
	// 스킬 초기화
	public void InitializeCardAction(ICard card, IBattleStatus cardOwnerStatus = null) {
		ThisCard = card;
		OwnerStatus = cardOwnerStatus;
	}

	// 스킬 사용 시 동작
	public void ProceedCardAction(IBattleStatus cardTargetStatus = null) {
		TargetStatus = BattleManager.GetInstance().GetBattleStatus(cardTargetStatus);

		// 피해 적용 예시
		TargetStatus.Damage(this, CurrentDamage);

		// 회복 적용 예시
		OwnerStatus.GainHP(this, HealCalc(5));

		// 방어 적용 예시
		OwnerStatus.GainShield(this, ShieldCalc(3));

		// 버프 적용 예시
		OwnerStatus.GainBuff(this, ENUM_BUFF_INDEX.SHARPNESS, CountCalc(1));

		// SFX 예시
		SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_hp_heal);
		// VFX 예시
		BattleEffectManager.PlayUIEffect(ENUM_BATTLE_VFX_UI.HEAL);
	}

	// 스킬 사용 종료 후 처리
	public void FinalizeCardAction() {

	}
	#endregion
}
