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
	// ex) 특정 진영 공격 카드 사용 시 합연산 피해량 증가시 cardPowerAddition[(ENUM_FACTION.EXAMPLE_FACTION_1, ENUM_PLAYER_CARD_TYPE.DAMAGE)] += value
	// 곱연산량의 경우 1을 증가시킬 때 마다 원래 값의 2배, 3배, 4배로 선형적으로 증가함.
	// 단, 카드 파워 타입과 다른 타입의 추가적용 수치를 바꿀 수는 없음 ex) 특정 진영 방어 카드 사용 시 5의 피해를 줌.
	// 특정 진영 공격 카드 사용시 적에게 출혈 버프를 주고 싶다면 cardBuffAddition[(ENUM_FACTION.EXAMPLE_FACTION_2, ENUM_PLAYER_CARD_TYPE.DAMAGE)].Add((ENUM_BUFF_INDEX.EXAMPLE_BUFF, count))
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

	// 버프 효과
	public bool isBuffFlag_001;
	public bool isBuffFlag_002;
	public bool isBuffFlag_002_2;
	public bool isBuffFlag_003;
	public bool isBuffFlag_004;
	public bool isBuffFlag_005;
	public bool isBuffFlag_006;
	public bool isBuffFlag_007;
	public bool isBuffFlag_008;
	public bool isBuffFlag_009;
	public bool isBuffFlag_010;
	public bool isBuffFlag_011;
	public bool isBuffFlag_012;
	public bool isBuffFlag_013;
	public bool isBuffFlag_014;
	public bool isBuffFlag_015;
	public bool isBuffFlag_016;
	public bool isBuffFlag_017;
	public bool isBuffFlag_018;

	// 저주 효과

	public bool isCurseFlag_001;
	public bool isCurseFlag_001_2;
	public bool isCurseFlag_002;
	public bool isCurseFlag_002_2;
	public bool isCurseFlag_003;
	public bool isCurseFlag_003_2;
	public bool isCurseFlag_004;
	public bool isCurseFlag_004_2;
	public bool isCurseFlag_005;
	public bool isCurseFlag_005_2;

	// 키워드 효과
	private bool isKeyword_001;
	public bool IsKeyword_001 {
		get => hasEquipment_Example_001 || isKeyword_001;
		set { isKeyword_001 = value; }
	}
	private bool isKeyword_002;
	public bool IsKeyword_002 {
		get => isKeyword_002;
		set { isKeyword_002 = value; }
	}
	private bool isKeyword_003;
	public bool IsKeyword_003 {
		get => isKeyword_003 || isCardEffect_003;
		set { isKeyword_003 = value; }
	}

	// 카드 효과
	public bool isCardEffect_001;
	public bool isCardEffect_002;
	public bool isCardEffect_003;
	public bool isCardEffect_004;

	// 스킬 효과
	public bool isSkillEffect_001;
	public bool isSkillEffect_002;

	// 장비 효과
	public bool hasEquipment_Example_001;
	public bool hasEquipment_Example_002;
	public bool hasEquipment_Example_003;
	public bool hasEquipment_Example_004;
	public bool hasEquipment_Example_005;
	public bool hasEquipment_Example_006;
	public bool hasEquipment_Example_007;
	public bool hasEquipment_Example_008;
	public bool hasEquipment_Example_009;
	public bool hasEquipment_Example_010;
	// ... 추가 장비 플래그들

	// Card Temp Value
	// 카드 사용 시 임시로 가지고 있어야 하는 값들로 다음 턴에도 가지고 있어야하는 값들은 여기에 저장함.
	public int card_Example_TempValue_001;
	public bool card_Example_TempValue_002;
	public bool card_Example_TempValue_003;
	public bool card_Example_Active;

	// 적 효과
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
