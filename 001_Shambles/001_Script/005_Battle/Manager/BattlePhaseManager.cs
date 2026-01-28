using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ENUM_BATTLE_PHASE_TARGET;
using static ENUM_BATTLE_PHASE_ACTION;

/// <summary>
/// 전투 페이즈 흐름을 제어하는 매니저 클래스
/// 턴 시작/종료, 전투 시작/종료 등 페이즈별 이펙트 실행 및 콜렉터 관리
/// </summary>
public class BattlePhaseManager : MonoBehaviour {
	Dictionary<(ENUM_BATTLE_PHASE_TARGET, ENUM_BATTLE_PHASE_ACTION), List<IBattlePhaseEffect>> phaseEffectList;
	Dictionary<(ENUM_BATTLE_PHASE_TARGET, ENUM_BATTLE_PHASE_ACTION), List<IBattlePhaseEffect>> removePhaseEffectList;
	List<IBattlePhaseCollector> phaseCollectors;

	void Awake() {
		phaseEffectList = new Dictionary<(ENUM_BATTLE_PHASE_TARGET, ENUM_BATTLE_PHASE_ACTION), List<IBattlePhaseEffect>>();
		removePhaseEffectList = new Dictionary<(ENUM_BATTLE_PHASE_TARGET, ENUM_BATTLE_PHASE_ACTION), List<IBattlePhaseEffect>>();
		phaseCollectors = new List<IBattlePhaseCollector>();

		foreach (ENUM_BATTLE_PHASE_TARGET phaseTarget in Enum.GetValues(typeof(ENUM_BATTLE_PHASE_TARGET))) {
			foreach (ENUM_BATTLE_PHASE_ACTION actionEnum in Enum.GetValues(typeof(ENUM_BATTLE_PHASE_ACTION))) {
				phaseEffectList.Add((phaseTarget, actionEnum), new List<IBattlePhaseEffect>());
				removePhaseEffectList.Add((phaseTarget, actionEnum), new List<IBattlePhaseEffect>());
			}
		}
	}

	public bool IsPhaseEffectAlive(IBattlePhaseEffect effect, ENUM_BATTLE_PHASE_TARGET target, ENUM_BATTLE_PHASE_ACTION action) {
		return phaseEffectList[(target, action)].Contains(effect);
	}

	private void ProceedRemove(in ENUM_BATTLE_PHASE_TARGET target, ENUM_BATTLE_PHASE_ACTION phaseEnumAction) {
		//제거목록 제거
		for (int i = 0; i < removePhaseEffectList[(target, phaseEnumAction)].Count; i++) {
			phaseEffectList[(target, phaseEnumAction)].Remove(removePhaseEffectList[(target, phaseEnumAction)][i]);
		}
		removePhaseEffectList[(target, phaseEnumAction)].Clear();
	}

	public void ProceedPhase(in ENUM_BATTLE_PHASE_TARGET phaseEnumTargets, ENUM_BATTLE_PHASE_ACTION phaseEnumAction) {

		if (BattleManager.GetInstance().IsBattleEnd == true) {
			return;
		}
		ProceedTargetAction(phaseEnumTargets, (target) => {
			ProceedRemove(target, phaseEnumAction);
			int count = phaseEffectList[(target, phaseEnumAction)].Count;
			// Call Effect Phases
			for (int i = 0; i < count; i++) {
				if (target == NONE) continue;
				phaseEffectList[(target, phaseEnumAction)][i]?.OnEffectPhase(phaseEnumAction);
			}
			ProceedRemove(target, phaseEnumAction);

			// Turn Phases can be proceeded only once in a turn.
			//Battle Start
			if (phaseEnumAction == BATTLE_START) {
				ProceedPhase(PLAYER, TURN_START_STAND_BY);
			}
			// Start turn
			else if (phaseEnumAction == TURN_START_STAND_BY) {
				BattleManager.GetInstance().SetCurrentTurn(target);
				BattleManager.GetInstance().battleEventManager.OnTurnStart?.Invoke();
				ProceedPhase(target, TURN_START);
			}
			else if (phaseEnumAction == TURN_START) {
			}
			// After turn end
			else if (phaseEnumAction == TURN_END_STAND_BY) {
				ProceedPhase(target, TURN_END);
			}
			else if (phaseEnumAction == TURN_END) {
				if (target == PLAYER) {
					BattleManager.GetInstance().battleEnemyManager.ProceedEnemyPattern(ENEMY1);
				}
			}

			// Proceed Collectors
			if (phaseCollectors.Count > 0) {
				for (int i = 0; i < phaseCollectors.Count; i++) {
					phaseCollectors[i].OnEffectPhase(target, phaseEnumAction);

					if (phaseCollectors.Count > 0 && !phaseCollectors.Contains(phaseCollectors[i])) i--;
				}
			}
		});
	}

	private void ProceedTargetAction(ENUM_BATTLE_PHASE_TARGET phaseEnumTargets, Action<ENUM_BATTLE_PHASE_TARGET> action) {
		foreach (ENUM_BATTLE_PHASE_TARGET target in Enum.GetValues(typeof(ENUM_BATTLE_PHASE_TARGET))) {
			if (phaseEnumTargets.HasFlag(target)) {
				action(target);
			}
		}
	}

	public void AddPhaseEffect(IBattlePhaseEffect battlePhaseEffect, in ENUM_BATTLE_PHASE_TARGET battleTargets, params ENUM_BATTLE_PHASE_ACTION[] battlePhaseActionList) {
		if (battlePhaseActionList.Length == 0) {
			Debug.LogError("Error : AddPhaseEvent has no Action");
			return;
		}

		ProceedTargetAction(battleTargets, (target) => {
			foreach (var action in battlePhaseActionList) {
				phaseEffectList[(target, action)].Add(battlePhaseEffect);
			}
		});
	}

	public void RemovePhaseEffectRequest(IBattlePhaseEffect battlePhaseEffect, in ENUM_BATTLE_PHASE_TARGET battleTargets, params ENUM_BATTLE_PHASE_ACTION[] battlePhaseActionList) {
		if (battlePhaseActionList.Length == 0) {
			Debug.LogError("Error : RemovePhaseEvent has no Action");
			return;
		}

		ProceedTargetAction(battleTargets, (target) => {
			foreach (var action in battlePhaseActionList) {
				if (phaseEffectList[(target, action)].Contains(battlePhaseEffect))
					removePhaseEffectList[(target, action)].Add(battlePhaseEffect);
			}
		});
	}

	public IBattlePhaseEffect AddDisposablePhaseEffect(Action effectMethod, ENUM_BATTLE_PHASE_TARGET battleTargets, ENUM_BATTLE_PHASE_ACTION battlePhaseAction, int count = 1) {
		// n턴 후 페이즈 효과를 실행하는 일회용 페이즈 효과를 위한 메서드. 기본값: 1턴 후 제거됨.
		DisposablePhaseEffect phaseEffect = new DisposablePhaseEffect();
		phaseEffect.InitializeEffectMethod(effectMethod, () => RemovePhaseEffectRequest(phaseEffect, battleTargets, battlePhaseAction), count);
		AddPhaseEffect(phaseEffect, battleTargets, battlePhaseAction);
		return phaseEffect;
	}

	public IBattlePhaseEffect AddDisposablePhaseEffect(Action effectMethod, ENUM_BATTLE_PHASE_TARGET battleTargets, params ENUM_BATTLE_PHASE_ACTION[] battlePhaseActionList) {
		DisposablePhaseEffect phaseEffect = new DisposablePhaseEffect();
		phaseEffect.InitializeEffectMethod(effectMethod, () => RemovePhaseEffectRequest(phaseEffect, battleTargets, battlePhaseActionList), 1);
		AddPhaseEffect(phaseEffect, battleTargets, battlePhaseActionList);
		return phaseEffect;
	}

	class DisposablePhaseEffect : IBattlePhaseEffect {
		int count;
		Action effectMethod;
		Action removeEffectMethod;

		public void InitializeEffectMethod(Action effectMethod, Action removeEffectMethod, int count) {
			this.effectMethod = effectMethod;
			this.removeEffectMethod = removeEffectMethod;
			this.count = count;
		}

		public void OnEffectPhase(ENUM_BATTLE_PHASE_ACTION action) {
			if (--count <= 0) {
				effectMethod();
				removeEffectMethod();
			}
		}
	}

	// 페이즈 콜렉터는 특정 시점과 시점 사이 발생한 페이즈 이벤트를 수집하는 객체
	// 원하는 시점에 콜렉터를 등록하고 종료 시점(종료 이벤트 발생)에 콜렉터 자신을 등록해제. 이때 이벤트로 등록한 경우 inclusive 등록이벤트 호출/exclusive 종료이벤트 호출(호출안함)
	// 사용 예시) 적 1의 페이즈 시작부터 적 3의 페이즈 종료까지 발생한 이벤트를 수집하고 발생한 이벤트의 개수 만큼 무언가를 처리하고 싶을 때
	public void AddPhaseCollector(IBattlePhaseCollector collector, ENUM_BATTLE_PHASE_TARGET target, params ENUM_BATTLE_PHASE_ACTION[] endActions) {
		phaseCollectors.Add(collector);

		AddDisposablePhaseEffect(() => {
			phaseCollectors.Remove(collector);
		}, target, endActions);
	}

	public void RemovePhaseCollector(IBattlePhaseCollector collector) {
		phaseCollectors.Remove(collector);
	}
}
