using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 전투 중 동적으로 변화하는 상태값들을 관리하는 클래스
/// 버프/디버프 효과, 키워드 상태, 장비 플래그, 스탯 보정치 등을 포함
/// </summary>
public class BattleDynamicValues {
	public IBattleStatus ownerStatus;
	public IBattleStatus lastDamagedByTarget;

	// 카드 사용 시 추가 적용되는 수치들 또는 버프를 줌. BattleCardStaticMethods 클래스의 메서드들과 연계됨.
	// ex) 산마지카 공격 카드 사용 시 합연산 피해량 증가시 cardPowerAddition[(ENUM_FACTION.SAN_MAGIKA, ENUM_PLAYER_CARD_TYPE.DAMAGE)] += value
	// 곱연산량의 경우 1을 증가시킬 때 마다 원래 값의 2배, 3배, 4배로 선형적으로 증가함.
	// 단, 카드 파워 타입과 다른 타입의 추가적용 수치를 바꿀 수는 없음 ex) 임페리얼 방어 카드 사용 시 5의 피해를 줌.
	// 로아 공격 카드 사용시 적에게 출혈 버프를 주고 싶다면 cardBuffAddition[(ENUM_FACTION.ROA, ENUM_PLAYER_CARD_TYPE.DAMAGE)].Add((ENUM_BUFF_INDEX.BLEEDING, count))
	public StatArray<int> statusAddition = new StatArray<int>();
	public StatArray<float> statusMultiply = new StatArray<float>();
	public StatArray<int> statusFinal = new StatArray<int>();

	public class StatArray<T> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable {
		private T[] _stat;
		// 이벤트 정의
		public event Action OnStatChanged;

		public StatArray() {
			_stat = new T[Enum.GetValues(typeof(ENUM_STATUS)).Length];
		}

		public int Length {
			get {
				return _stat.Length;
			}
		}

		public T this[int index] {
			get { return _stat[index]; }
			set {
				_stat[index] = value;

				// 이벤트 발생
				OnStatChanged?.Invoke();
			}
		}
	}

	public int damageAddition;
	public float damageMultiply;
	public Func<BattleDamage, BattleDamage> damageOverride;
	public int cardCostAddition;
	public float cardCostMultiply;

	// Buff Effect
	public bool isPenetrating;
	public bool isHarden;
	public bool isHarden_2;
	public bool isHiding;
	public bool isBloodSucking;
	public bool isDefenseDestruction;
	public bool isInvincible;
	public bool isBlind;
	public bool isCorrosion;
	public bool isCorruption;
	public bool isFrenzy;
	public bool isStunned;
	public bool isMarked;
	public bool isDisarmed;
	public bool isStopped;
	public bool isStunImmunity;
	public bool isRepairing;
	public bool isCaptured;
	public bool isSuppressed;
	public bool isHacked;
	public bool isRampaged;

	//Curse Effect

	public bool isOverburdened;
	public bool isOverburdened2;
	public bool isFragileBody;
	public bool isFragileBody2;
	public bool isToxicWeakness;
	public bool isToxicWeakness2;
	public bool isSlowHands;
	public bool isSlowHands2;
	public bool isCloudedMind;
	public bool isCloudedMind2;

	//Keyword Effect
	private bool isPureBlood;
	public bool IsPureBlood {
		get => hasEquipment_433 || isPureBlood;
		set { isPureBlood = value; }
	}
	private bool isOverwhelming;
	public bool IsOverwhelming {
		get => isOverwhelming;
		set { isOverwhelming = value; }
	}
	private bool isLoneliness;
	public bool IsLoneliness {
		get => isLoneliness || isLonelyBattle;
		set { isLoneliness = value; }
	}

	//Card Effect
	public bool isTargetFixed;
	public bool isGreatBuff;
	public bool isLonelyBattle;
	public bool isWeaknessDetection;

	//Skill Effect
	public bool isDoubleUp;
	public bool isMechanicsGrip;

	//Equipment Effect
	public bool hasEquipment_8;
	//public bool hasEquipment_18;
	//public bool hasEquipment_19;
	public bool hasEquipment_20;
	public bool hasEquipment_29;
	public bool hasEquipment_31;
	public bool hasEquipment_32;
	public bool hasEquipment_33;
	public bool hasEquipment_110;
	public bool hasEquipment_122;
	public bool hasEquipment_205;
	public bool hasEquipment_218;
	public bool hasEquipment_222;
	public bool hasEquipment_223;
	public bool hasEquipment_227;
	public bool hasEquipment_230;
	public bool hasEquipment_303;
	public bool hasEquipment_310;
	public bool hasEquipment_309;
	public bool hasEquipment_311;
	public bool hasEquipment_313;
	public bool hasEquipment_315;
	public bool hasEquipment_316;
	public bool hasEquipment_318;
	public bool hasEquipment_326;
	public bool hasEquipment_327;
	public bool hasEquipment_330;
	public bool hasEquipment_340;
	public bool hasEquipment_341;
	public bool hasEquipment_344;
	public bool hasEquipment_345;
	public bool hasEquipment_349;
	public bool hasEquipment_350;
	public bool hasEquipment_351;
	public bool hasEquipment_433;
	public bool hasEquipment_424;
	public bool hasEquipment_512;
	public bool hasEquipment_515;
	public bool hasEquipment_521;
	public bool hasEquipment_533;
	public bool hasEquipment_534;

	// Card Temp Value
	// 카드 사용 시 임시로 가지고 있어야 하는 값들로 다음 턴에도 가지고 있어야하는 값들은 여기에 저장함.
	//Deprecated
	public int card_033_MemoryRewind_MapAP;
	public bool card_033_MemoryRewind_IsLoneliness;
	public bool card_033_MemoryRewind_TurnStartDraw;
	//Renew
	public bool card_033;

	// Enemy Effect
	public bool isImmortal;
	public bool isResting;
	public bool noPlayDie;

	public BattleDynamicValues(IBattleStatus ownerStatus) {
		this.ownerStatus = ownerStatus;

		for (int i = 0; i < Enum.GetValues(typeof(ENUM_STATUS)).Length; i++) {
			statusMultiply[i] = 0;
			statusAddition[i] = 0;
			statusFinal[i] = 0;
		}
		statusMultiply.OnStatChanged += () => BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ownerStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.STAT_CHANGED);
		statusAddition.OnStatChanged += () => BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ownerStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.STAT_CHANGED);
		damageAddition = 0;
		damageMultiply = 0;
		cardCostAddition = 0;
		cardCostMultiply = 0;
	}

	public float GetCaculatedStatus(ENUM_STATUS status, bool exceptFinal = false) {
		float m_Status = ownerStatus.Status[(int)status];
		m_Status += statusAddition[(int)status];
		m_Status *= 1f + statusMultiply[(int)status];
		return exceptFinal ? m_Status : Mathf.Max(m_Status, statusFinal[(int)status]);
	}
}
