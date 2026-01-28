using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 발생하는 이벤트 델리게이트를 관리하는 클래스
/// 턴 시작, 카드 사용, 피해/회복/버프 적용 등의 이벤트 구독 제공
/// </summary>
public class BattleEventManager : MonoBehaviour {
	// 시전자의 영향을 받아 수치가 바뀌는 이벤트
	public Action OnTurnStart;
	public Action<Skill> OnUseSkill = null;
	// public Func<Card, Card> OnUseCardStandBy = null;
	public Action<Card> OnUseCard = null;
	public Action<Card> OnDrawCard = null;
	public Action<Card> OnCardOnHand = null;
	public Func<IBattlePlayerCard, int> CostSet = null;
	public Func<IBattlePlayerCard, int> CostAddition = null;
	public Func<IBattlePlayerCard, int> DamageAddition = null; // "추가 피해", 카드 종류에 영향을 받으며 수치가 카드에 표시됨. + 또는 - 형태로 증감시킴.
	public Func<IBattlePlayerCard, int> HealAddition = null;
	public Func<IBattlePlayerCard, int> ShieldAddition = null;
	// 시전대상의 영향을 받아 수치가 바뀌는 이벤트
	public Func<IBattleStatus, IBattleFactor, int> ExtraDamageOnTargetDamaged = null; // "추가 피해". 피해를 입는 대상에 영향을 받으며 수치가 카드에 표시되지 않음.
	public Action<IBattleStatus, IBattleFactor, int> OnTargetDamaged = null; // "직접 피해". 버프, 디버프, 장비 같은 효과가 아닌 카드(적의 경우 행동)와 스킬로 인한 피해에만 발동.
	public Action<IBattleStatus, IBattleFactor, int> OnTargetHpDamaged = null; // "직접 피해"와 연계하여 체력에 피해를 입으면 발동.
	public Action<IBattleStatus, IBattleFactor, int> OnTargetGainShield = null;
	public Action<IBattleStatus, IBattleFactor, int> OnTargetShieldDamaged = null;
	public Action<IBattleStatus, IBattleFactor> OnTargetGainHp = null;
	public Action<IBattleStatus, IBattleFactor> OnTargetGainAp = null;
	public Action<IBattleStatus, IBattleFactor, Buff, int> OnTargetGainBuff = null;
	public Action<IBattleStatus, Buff> OnTargetLoseBuff = null;
	public Action<BattleEnemyObject> OnEnemyDead = null;
}
