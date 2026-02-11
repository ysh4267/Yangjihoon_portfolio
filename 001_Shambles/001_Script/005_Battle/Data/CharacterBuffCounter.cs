using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// 캐릭터의 버프/디버프 상태를 관리하는 클래스, 버프 추가/제거/조회/면역 판정 등을 처리
public class CharacterBuffCounter : IBattlePhaseEffect {
	// 버프가 적용될 대상의 전투 상태 참조
	private IBattleStatus targetStatus;
	// 버프 아이콘 UI 리스트
	BattleBuffIconList iconList;
	// 전체 버프 목록
	private LinkedList<Buff> allBuffList = new LinkedList<Buff>();
	// 전체 버프 목록을 외부에 반환
	public LinkedList<Buff> GetAllBuffList() {
		return allBuffList;
	}
	// 턴 기반 카운트 버프 목록
	private LinkedList<Buff> countByTurnBuffList = new LinkedList<Buff>();
	// 카드 효과로 부여된 버프 목록
	private LinkedList<Buff> cardBuffList = new LinkedList<Buff>();
	// 현재 적용 중인 전체 버프 수
	public int numberOfAllBuffs => allBuffList.Count;
	// 현재 적용 중인 버프 수
	public int numberOfBuffs => GetNumberOfBuffs(ENUM_BUFF_TYPE.BUFF);
	// 현재 적용 중인 디버프 수
	public int numberOfDebuffs => GetNumberOfBuffs(ENUM_BUFF_TYPE.DEBUFF);
	// 버프 UI 갱신 시 적용되는 지연 시간
	const float updateUIDelay = 0.3f; // sec

	// 아이콘 리스트와 대상 상태를 초기화하고 페이즈 이펙트로 등록
	public CharacterBuffCounter(BattleBuffIconList iconList, IBattleStatus targetStatus, ENUM_BATTLE_PHASE_ACTION action) {
		this.iconList = iconList;
		this.targetStatus = targetStatus;
		iconList.Initialize(this);
		BattleManager.GetInstance().battlePhaseManager.AddPhaseEffect(this, targetStatus.TargetEnum, action);
	}

	// 버프 리스트를 순회하며 각 버프에 지정된 액션을 실행, 순회 중 리스트 변경에 대응
	private void ProceedBuffAction(in LinkedList<Buff> buffList, Action<Buff> action) {
		List<LinkedListNode<Buff>> proceededBuffList = new List<LinkedListNode<Buff>>();
		var node = buffList.First;
		while (node != null) {
			try {
				proceededBuffList.Add(node);
				action(node.Value);
			}
			catch (System.Exception) {
				return;
			}

			if (!buffList.Contains(node.Value)) {
				node = buffList.First;
				while (proceededBuffList.Contains(node)) {
					node = node.Next;
				}
			}
			else {
				node = node.Next;
			}
		}
	}

	// 전체 버프 리스트에 대해 지정된 액션을 실행
	public void ProceedAllBuffAction(Action<Buff> action) {
		ProceedBuffAction(allBuffList, action);
	}

	// 카드 버프 리스트에 대해 지정된 액션을 실행
	public void ProceedCardBuffAction(Action<Buff> action) {
		ProceedBuffAction(cardBuffList, action);
	}

	// 턴 종료 시 턴 기반 버프와 카드 버프의 카운트를 1씩 감소
	void UpdateBuffCount() {
		ProceedBuffAction(countByTurnBuffList, (buff) => {
			buff.battleBuffScript.SubtractCount();
			SubtractBuffCountByEquipment(buff);
		});

		ProceedBuffAction(cardBuffList, (buff) => {
			if (buff.battleBuffScript.CounterType == ENUM_BUFF_COUNTER_TYPE.COUNT_BY_TURN)
				buff.battleBuffScript.SubtractCount();
		});

		// 장비에 의한 추가 카운트 감소 처리
		void SubtractBuffCountByEquipment(Buff buff) {
			if (targetStatus.TargetEnum != ENUM_BATTLE_PHASE_TARGET.PLAYER) return;
		}
	}

	// 페이즈 이펙트 콜백, 버프 카운트 감소 후 UI를 갱신
	public void OnEffectPhase(ENUM_BATTLE_PHASE_ACTION action) {
		UpdateBuffCount();
		UpdateBuffUI();
		targetStatus.UpdateUI();
	}

	// 버프를 추가하여 대상/타입에 따라 페이즈를 실행하고 동일 버프가 존재하면 카운트를 중첩
	// ActivateBuffEffect에서 호출하며, 실제 효과 발생 이후 마지막에 호출해야 UI가 갱신됨
	public void AddBuff(Buff buff) {
		if (buff.battleBuffScript.ContinuousCount <= 0) return;
		// if target is PLAYER
		if (targetStatus == BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus) {
			if (CheckImmunity(buff))
				return;

			if (buff.enumBuffType == ENUM_BUFF_TYPE.BUFF)
				BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.GAIN_BUFF);
			if (buff.enumBuffType == ENUM_BUFF_TYPE.DEBUFF)
				BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.GAIN_DEBUFF);
		}
		// if target is ENEMY
		else if (ENUM_BATTLE_PHASE_TARGET.ALL_ENEMIES.HasFlag(targetStatus.TargetEnum)) {
			if (targetStatus.DynamicValues.isHiding) {
				if (BattleManager.GetInstance().battleEnemyManager.EnemyCount < 2)
					RemoveBuff(ENUM_BUFF_INDEX.EXAMPLE_BUFF_007);
			}
			if (BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus.DynamicValues.hasEquipment_Example_006 &&
				BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus.BuffCounter.HasEnoughBuffCount(ENUM_BUFF_INDEX.EXAMPLE_BUFF_008, 12) &&
				(buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_BUFF_009 ||
				buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_BUFF_010))
				return;

			if (buff.enumBuffType == ENUM_BUFF_TYPE.BUFF)
				BattleManager.GetInstance().battlePhaseManager.ProceedPhase(targetStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.GAIN_BUFF);
			if (buff.enumBuffType == ENUM_BUFF_TYPE.DEBUFF)
				BattleManager.GetInstance().battlePhaseManager.ProceedPhase(targetStatus.TargetEnum, ENUM_BATTLE_PHASE_ACTION.GAIN_DEBUFF);
		}

		bool exist = false;
		foreach (var _buff in allBuffList) {
			if (_buff.battleBuffScript.BuffName == buff.battleBuffScript.BuffName) {
				_buff.battleBuffScript.ContinuousCount += buff.battleBuffScript.ContinuousCount;
				if (_buff.battleBuffScript.ContinuousCount > 9999) _buff.battleBuffScript.ContinuousCount = 9999;
				exist = true;
			}
		}
		if (!exist) {
			allBuffList.AddLast(buff);
			if (buff.battleBuffScript.CounterType == ENUM_BUFF_COUNTER_TYPE.COUNT_BY_TURN)
				countByTurnBuffList.AddLast(buff);
		}

		if ((ENUM_BUFF_INDEX)buff.Index != ENUM_BUFF_INDEX.EXAMPLE_BUFF_011) {
			SoundManager.GetInstance().PlayEffectSound(soundEnum: buff.enumBuffType == ENUM_BUFF_TYPE.BUFF ?
				ENUM_EFFECT_SOUND.Player_get_buff : ENUM_EFFECT_SOUND.Player_get_debuff, delay: updateUIDelay, noOverlap: true);
		}
		iconList.UpdateBuffList(updateUIDelay);
		targetStatus.UpdateUI();
	}

	// 카드 버프를 추가하여 카드 버프 리스트에 등록
	// 카드 버프는 일반 버프와 체계가 다르며 3개 제한 없이, 아이콘 표시 없이 동작
	// 버프 획득 페이즈를 실행하지 않으며, 효과는 중첩되지만 카운트는 고유값을 유지한 채 턴마다 감소
	public void AddCardBuff(Buff cardBuff) {
		cardBuffList.AddLast(cardBuff);

		UpdateBuffUI();
		targetStatus.UpdateUI();
	}

	// 지정된 버프의 카운트를 count만큼 감소시키고 실제 감소된 수치를 반환
	public int SubtractBuffCount(Buff buff, int count) {
		int change = buff.battleBuffScript.ContinuousCount;
		if (count > change) count = change;
		for (int i = 0; i < count; i++) {
			buff.battleBuffScript.SubtractCount();
		}
		change -= buff.battleBuffScript.ContinuousCount;

		UpdateBuffUI();
		targetStatus.UpdateUI();

		return change;
	}

	// 버프 인덱스로 해당 버프를 찾아 카운트를 감소시키고 실제 감소된 수치를 반환
	public int SubtractBuffCount(ENUM_BUFF_INDEX buffIndex, int count) {
		int change = 0;

		ProceedBuffAction(allBuffList, (buff) => {
			if (buff.Index.Equals((int)buffIndex)) {
				change = SubtractBuffCount(buff, count);

				throw new Exception("exit loop");
			}
		});

		return change;
	}

	// 버프를 모든 리스트에서 제거하고 효과를 종료하여 제거된 카운트를 반환
	public int RemoveBuff(Buff buff) {
		int removedCount = buff.battleBuffScript.ContinuousCount;
		countByTurnBuffList.Remove(buff);
		cardBuffList.Remove(buff);
		allBuffList.Remove(buff);
		buff.battleBuffScript.EndBuffEffect();
		BattleManager.GetInstance().battleEventManager.OnTargetLoseBuff?.Invoke(targetStatus, buff);

		UpdateBuffUI();
		targetStatus.UpdateUI();

		return removedCount;
	}

	// 버프 인덱스로 해당 버프를 찾아 제거하고 성공 여부를 반환
	public bool RemoveBuff(ENUM_BUFF_INDEX buffIndex) {
		foreach (var buff in allBuffList) {
			if (buff.Index.Equals((int)buffIndex)) {
				RemoveBuff(buff);
				return true;
			}
		}

		return false;
	}

	// 버프 인덱스로 해당 버프를 찾아 제거하고 제거된 카운트를 out으로 반환
	public bool RemoveBuff(ENUM_BUFF_INDEX buffIndex, out int removedCount) {
		foreach (var buff in allBuffList) {
			if (buff.Index.Equals((int)buffIndex)) {
				removedCount = buff.battleBuffScript.ContinuousCount;
				RemoveBuff(buff);
				return true;
			}
		}

		removedCount = 0;
		return false;
	}

	// 지정된 타입의 버프 중 랜덤으로 하나를 제거하고 제거된 버프 인덱스와 카운트를 반환
	public ENUM_BUFF_INDEX RemoveRandomBuff(ENUM_BUFF_TYPE buffType, out int removedCount) {
		removedCount = 0;
		Buff buff;
		int buffCount = buffType == ENUM_BUFF_TYPE.BUFF ? numberOfBuffs : numberOfDebuffs;

		if (allBuffList.Count == 0) return 0;
		if (buffCount == 0) return 0;
		while (true) {
			int ran = UnityEngine.Random.Range(0, allBuffList.Count);
			buff = allBuffList.ElementAt(ran);
			if (buff.enumBuffType == buffType) {
				ENUM_BUFF_INDEX buffEnum = (ENUM_BUFF_INDEX)buff.Index;
				removedCount = buff.battleBuffScript.ContinuousCount;
				RemoveBuff(buff);
				return buffEnum;
			}
		}
	}

	// 모든 버프의 효과를 종료하고 버프 리스트를 초기화
	public void ClearBuffs() {
		foreach (var item in allBuffList) {
			item.battleBuffScript.EndBuffEffect();
		}

		countByTurnBuffList.Clear();
		allBuffList.Clear();
	}

	// 시스템 버프명으로 해당 버프의 존재 여부를 확인
	public bool BuffExist(in string systemBuffName) {
		foreach (var item in allBuffList) {
			if (item.battleBuffScript.BuffName == systemBuffName) {
				return true;
			}
		}
		return false;
	}

	// 버프 인덱스로 해당 버프의 존재 여부를 확인
	public bool BuffExist(ENUM_BUFF_INDEX buffIndex) {
		foreach (var item in allBuffList) {
			if (item.Index.Equals((int)buffIndex)) {
				return true;
			}
		}
		return false;
	}

	// 카드 버프명으로 해당 카드 버프의 존재 여부를 확인
	public bool CardBuffExist(string buffName) {
		foreach (var item in cardBuffList) {
			if (item.battleBuffScript.BuffName.Equals(buffName)) {
				return true;
			}
		}
		return false;
	}

	// 지정된 버프 타입이 존재하는지 확인
	public bool BuffTypeExist(ENUM_BUFF_TYPE buffType) {
		foreach (var buff in allBuffList) {
			if (buff.enumBuffType.Equals(buffType))
				return true;
		}
		return false;
	}

	// 지정된 버프의 카운트가 count 이상인지 확인
	public bool HasEnoughBuffCount(ENUM_BUFF_INDEX buffIndex, int count) {
		foreach (var item in allBuffList) {
			if (item.Index.Equals((int)buffIndex)) {
				if (item.battleBuffScript.ContinuousCount >= count) {
					return true;
				}
			}
		}
		return false;
	}

	// 시스템 버프명으로 해당 버프 객체를 반환
	public Buff GetBuff(in string systemBuffName) {
		foreach (var item in allBuffList) {
			if (item.battleBuffScript.BuffName == systemBuffName) {
				return item;
			}
		}
		return null;
	}

	// 버프 인덱스로 해당 버프 객체를 반환
	public Buff GetBuff(ENUM_BUFF_INDEX buffIndex) {
		foreach (var item in allBuffList) {
			if (item.Index.Equals((int)buffIndex)) {
				return item;
			}
		}
		return null;
	}

	// 지정된 버프의 현재 카운트를 반환, 존재하지 않으면 0 반환
	public int GetBuffCount(ENUM_BUFF_INDEX buffIndex) {
		return GetBuff(buffIndex)?.battleBuffScript?.ContinuousCount ?? 0;
	}

	// 여러 버프 인덱스의 카운트를 합산하여 반환, 중복 집계를 방지하기 위해 매칭된 인덱스를 순차적으로 제거
	public int GetBuffsCount(params ENUM_BUFF_INDEX[] buffIndices) {
		int count = 0;
		if (buffIndices.Length == 0) return 0;
		foreach (var item in allBuffList) {
			for (int i = 0; i < buffIndices.Length; i++) {
				if (item.Index == (int)buffIndices[i]) {
					count += item.battleBuffScript.ContinuousCount;
					buffIndices = buffIndices.Where((value, idx) => idx != i).ToArray();
					i--;
				}
			}
		}

		return count;
	}

	// 장비와 버프 조건을 확인하여 해당 버프에 대한 면역 여부를 반환
	public bool CheckImmunity(Buff buff) {
		// 장비와 버프 조건에 따른 면역 검사
		if (targetStatus.DynamicValues.hasEquipment_Example_001 &&
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_001) {
			return true;
		}
		if (targetStatus.DynamicValues.hasEquipment_Example_004 &&
			(buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_004 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_003 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_005 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_006))
			return true;
		if (targetStatus.DynamicValues.hasEquipment_Example_005 &&
			(buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_007 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_001 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_008))
			return true;
		if (targetStatus.DynamicValues.hasEquipment_Example_007 &&
			(buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_004 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_009 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_005 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_001 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_010 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_011 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_008))
			return true;
		if (targetStatus.DynamicValues.hasEquipment_Example_008 &&
			(buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_009 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_003))
			return true;
		if (targetStatus.DynamicValues.hasEquipment_Example_006 &&
			HasEnoughBuffCount(ENUM_BUFF_INDEX.EXAMPLE_BUFF_008, 12) &&
			(buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_006 ||
			buff.Index == (int)ENUM_BUFF_INDEX.EXAMPLE_DEBUFF_010)) {
			return true;
		}

		return false;
	}

	// 지정된 타입의 버프 개수를 집계하여 반환
	public int GetNumberOfBuffs(ENUM_BUFF_TYPE buffType) {
		int count = 0;

		ProceedAllBuffAction((buff) => {
			if (buff.enumBuffType == buffType)
				count++;
		});

		return count;
	}

	// 버프 아이콘 UI를 갱신
	public void UpdateBuffUI() {
		iconList.UpdateBuffList();
	}
}
