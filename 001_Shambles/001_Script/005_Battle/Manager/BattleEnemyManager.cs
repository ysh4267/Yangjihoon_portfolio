using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using static ENUM_BATTLE_PHASE_TARGET;
using static ENUM_BATTLE_PHASE_ACTION;

/// <summary>
/// 전투 중 적 개체들의 생명주기와 행동을 총괄하는 매니저 클래스
/// 적 생성/사망, 턴 진행, 타겟팅, UI 갱신 등을 담당
/// </summary>
public class BattleEnemyManager : MonoBehaviour {
	[SerializeField] BattleEnemyObject[] enemyPrefabs;
	SortedSet<BattleEnemyObject> enemyObjects = new SortedSet<BattleEnemyObject>(new Comparer()); // 우선순위 큐를 대신하기 위해 SortedSet 사용
	SortedSet<BattleEnemyObject> deadEnemyObjects = new SortedSet<BattleEnemyObject>(new Comparer());

	[HideInInspector] public List<BattleEnemyObject> BattleEnemyObjectList => enemyObjects.ToList();
	public int EnemyCount => enemyObjects.Count;

	//for enemy turn active coroutine
	[HideInInspector] public bool isEnemyPlaying = false;
	WaitWhile waitWhileEnemyPlaying;
	WaitForSeconds waitForSeconds5000 = new WaitForSeconds(5f);
	WaitForSeconds waitForSeconds1000 = new WaitForSeconds(1f);

	private class Comparer : IComparer<BattleEnemyObject> {
		public int Compare(BattleEnemyObject e1, BattleEnemyObject e2) {
			return (int)e1.enemyPhaseTargetEnum - (int)e2.enemyPhaseTargetEnum;
		}
	}

	public void InitializeEnemyList(in List<Enemy> enemyDataList) {
		for (int i = 0; i < enemyPrefabs.Length; i++) {
			deadEnemyObjects.Add(enemyPrefabs[i]);
		}
		for (int i = 0; i < enemyDataList.Count; i++) {
			AddEnemy(enemyDataList[i]);
		}
		BattleManager.GetInstance().battleEnemyManager.UpdateEnemyActionUI();
		waitWhileEnemyPlaying = new WaitWhile(() => isEnemyPlaying);
	}

	public bool AddEnemy(Enemy enemyData) {
		if (deadEnemyObjects.Count <= 0) return false;

		BattleEnemyObject enemyObject = deadEnemyObjects.First();
		Debug.Log(enemyObject.name);

		//자리가 부족할 경우 생성 취소
		if (deadEnemyObjects.Count <= 1 && enemyObject.enemySpriteTransform.childCount != 0) {
			Debug.Log("cancel");
			return false;
		}
		//자리가 2자리 이상 있고, 생성하려는 자리에 아직 몹이 살아있을 때
		else if (deadEnemyObjects.Count > 1 && enemyObject.enemySpriteTransform.childCount != 0) {
			enemyObject = deadEnemyObjects.Last();
			Debug.Log("cancel2");

			//여기도 차있으면 생성 취소
			if (enemyObject.enemySpriteTransform.childCount != 0) {
				Debug.Log("cancel2");
				return false;
			}
		}
		enemyObject.gameObject.SetActive(true);
		enemyObject.GetComponent<BoxCollider>().enabled = true;
		enemyObject.Initialize(enemyData);

		enemyObjects.Add(enemyObject);
		deadEnemyObjects.Remove(enemyObject);
		// 적이 3마리 이상인지 여부를 기록
		if (BattleManager.GetInstance().battleArchive.IsThreeEnemiesBattle == false)
			BattleManager.GetInstance().battleArchive.IsThreeEnemiesBattle = enemyObjects.Count >= 3;
		BattleManager.GetInstance().battlePhaseManager.ProceedPhase(enemyObject.enemyPhaseTargetEnum, SPAWN);
		return true;
	}

	public void DebugAddEnemy(int enemyIndex) {
		if (EnemyCount > 2) {
			DebugRemoveEnemy();
		}

		AddEnemy(EnemyDao.GetEnemy(enemyIndex));
	}

	public void DebugAddEnemies(int e1, int e2, int e3) {
		while (enemyObjects.Count > 0) {
			DebugRemoveEnemy();
		}

		if (e1 > 0)
			AddEnemy(EnemyDao.GetEnemy(e1));
		if (e2 > 0)
			AddEnemy(EnemyDao.GetEnemy(e2));
		if (e3 > 0)
			AddEnemy(EnemyDao.GetEnemy(e3));
	}

	public void DebugRemoveEnemy() {
		var targetEnemy = enemyObjects.Min();
		targetEnemy.enemyStatus.BuffCounter.ClearBuffs();
		targetEnemy.enemyStatus.SetHP(0);
		targetEnemy.Destroy();
		BattleManager.GetInstance().RemoveBattleStatus(targetEnemy.enemyStatus);
		ProceedEnemyAction(ALL_ENEMIES, (enemy) => enemy.enemyStatusUI.RemoveHealCard(targetEnemy.enemyPhaseTargetEnum));
		deadEnemyObjects.Add(targetEnemy);
		enemyObjects.Remove(targetEnemy);
		CheckHidingEnemies();
	}

	public void ProceedEnemyPattern(ENUM_BATTLE_PHASE_TARGET current) {
		if (BattleManager.GetInstance().IsBattleEnd == true) return;
		if (current > ENEMY3) {
			BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, TURN_START_STAND_BY);
			return;
		}

		var targetEnemy = GetEnemyObject(current);
		if (targetEnemy == null) {
			ProceedEnemyPattern((ENUM_BATTLE_PHASE_TARGET)((int)current << 1));
			return;
		}

		StartCoroutine(ProceedEnemy(targetEnemy));

		IEnumerator ProceedEnemy(BattleEnemyObject currentEnemy) {
			isEnemyPlaying = true;
			// Enemy turn start
			BattleManager.GetInstance().battlePhaseManager.ProceedPhase(currentEnemy.enemyPhaseTargetEnum, TURN_START_STAND_BY);
			yield return null; // Damaged 애니메이션 트리거 발동 후 1프레임 대기

			if (!currentEnemy.enemyStatus.DynamicValues.isStunned &&
				!currentEnemy.enemyStatus.DynamicValues.isStopped) {

				var turnEndCoroutine = StartCoroutine(ForceTurnEnd());
				currentEnemy.ProceedEnemyAction();
				yield return waitWhileEnemyPlaying;
				StopCoroutine(turnEndCoroutine);
			}
			else if (enemyObjects.Count == 1) {
				yield return waitForSeconds1000;
			}

			BattleManager.GetInstance().battlePhaseManager.ProceedPhase(currentEnemy.enemyPhaseTargetEnum, TURN_END_STAND_BY);
			currentEnemy.UpdateUI();
			ProceedEnemyPattern((ENUM_BATTLE_PHASE_TARGET)((int)currentEnemy.enemyPhaseTargetEnum << 1));
		}

		IEnumerator ForceTurnEnd() {
			yield return waitForSeconds5000;
			Camera.main.SetCameraBack();
		}
	}

	//살아있는 적에게 특정 action 을 실행
	public void ProceedEnemyAction(ENUM_BATTLE_PHASE_TARGET enemyTargets, Action<BattleEnemyObject> action) {
		for (int i = 0; i < BattleEnemyObjectList.Count; i++) {
			var targetEnemy = BattleEnemyObjectList[i];
			if (enemyTargets.HasFlag(targetEnemy.enemyPhaseTargetEnum)) {
				action(targetEnemy);

				if (!BattleEnemyObjectList.Contains(targetEnemy)) i--;
			}
		}
	}

	public async void KillEnemyDelay(BattleEnemyObject enemyObject) {
		enemyObject.GetComponent<BoxCollider>().enabled = false;
		await Task.Delay(100); // 사망 처리 강제 지연. 독 데미지로 인해 체력이 0보다 작아진 직후 체력을 회복하는 등 되살아날 수 있는 판정에 대해 예외처리.
		if (enemyObject.enemyStatus.CurrentHp > 0) {
			enemyObject.GetComponent<BoxCollider>().enabled = true;
			return;
		}

		KillEnemy(enemyObject);
	}

	public async void KillEnemy(BattleEnemyObject enemyObject) {
		#region 적 처치 업적
		// 예시업적_015 : 현실 난이도에서 적 다수 처치
		if (SettingManager.GetSettingData().difficulty == ENUM_DIFFICULTY.Reality) {
			AchievementEventManager.IncrementAchievementProgress(ENUM_ACHIEVEMENT.example_achievement_015);
		}
		// 예시업적_016 : 특정 유형의 적 처치
		if (enemyObject.enemyData.Index == 0000 || enemyObject.enemyData.Index == 0000 || enemyObject.enemyData.Index == 0000) {
			AchievementEventManager.IncrementAchievementProgress(ENUM_ACHIEVEMENT.example_achievement_016);
		}
		// 예시업적_020 : 다수의 적과 전투 시 특정 조건 달성
		if (BattleManager.GetInstance().battleArchive.IsThreeEnemiesBattle) {
			if (BattleManager.GetInstance().battleArchive.DamagedByTarget.Count == 1) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_020);
			}
		}
		// 예시업적_021 : 절망적 난이도에서 적 다수 처치 (단계 1)
		if (SettingManager.GetSettingData().difficulty == ENUM_DIFFICULTY.DesperateReality) {
			AchievementEventManager.IncrementAchievementProgress(ENUM_ACHIEVEMENT.example_achievement_021);
		}
		// 예시업적_022 : 절망적 난이도에서 적 다수 처치 (단계 2)
		if (SettingManager.GetSettingData().difficulty == ENUM_DIFFICULTY.DesperateReality) {
			AchievementEventManager.IncrementAchievementProgress(ENUM_ACHIEVEMENT.example_achievement_022);
		}
		#endregion

		BattleManager.GetInstance().battleStatisticsManager.AddEnemiesKilledValueOne();

		if (!enemyObject.enemyStatus.DynamicValues.noPlayDie)
			enemyObject.enemySprite?.GetComponent<Animator>().PlayDie();
		//diable
		enemyObjects.Remove(enemyObject);
		deadEnemyObjects.Add(enemyObject);
		BattleManager.GetInstance().RemoveBattleStatus(enemyObject.enemyStatus);
		BattleManager.GetInstance().CheckBattleEnd();

		//wait for enemy dying animation
		await Task.Delay(1500);

		//frontend proceed
		BattleManager.GetInstance().battleEventManager.OnEnemyDead?.Invoke(enemyObject);
		BattleManager.GetInstance().battlePhaseManager.ProceedPhase(enemyObject.enemyPhaseTargetEnum, DEAD);
		ProceedEnemyAction(ALL_ENEMIES, (enemy) => enemy.enemyStatusUI.RemoveHealCard(enemyObject.enemyPhaseTargetEnum));
		enemyObject.enemyStatus.BuffCounter.ClearBuffs();
		enemyObject.Destroy();
		CheckHidingEnemies();
	}

	public void ChangeEnemy(BattleEnemyObject fromObject, int toIndex) {
		var toObject = EnemyDao.GetEnemy(toIndex);
		Destroy(fromObject.enemySprite);
		fromObject.enemyStatus.BuffCounter.ClearBuffs();
		fromObject.Initialize(toObject);
	}

	public void SetEnemiesTargeted(ENUM_BATTLE_PHASE_TARGET? target, IBattleFactor factor, bool isTargeting, BattleDamage battleDamage = default) {
		if (target == null) return;

		foreach (var enemy in enemyObjects) {
			if (target.Value.HasFlag(enemy.enemyPhaseTargetEnum))
				enemy.SetTargeted(factor, isTargeting, battleDamage);
		}
	}

	public BattleEnemyObject GetEnemyObject(ENUM_BATTLE_PHASE_TARGET target) {
		foreach (var enemy in enemyObjects) {
			if (target.Equals(enemy.enemyPhaseTargetEnum))
				return enemy;
		}

		return null;
	}

	public BattleEnemyObject GetEnemyObject(int enemyIndex) {
		foreach (var enemy in enemyObjects) {
			if (enemyIndex == enemy.enemyData.Index)
				return enemy;
		}

		return null;
	}

	public BattleStatus GetEnemies(params int[] enemyIndices) {
		ENUM_BATTLE_PHASE_TARGET enemiesEnum = 0;

		foreach (var enemy in enemyObjects) {
			foreach (var index in enemyIndices) {
				if (index == enemy.enemyData.Index && !enemy.enemyStatus.DynamicValues.isResting) {
					enemiesEnum |= enemy.enemyPhaseTargetEnum;
					break;
				}
			}
		}

		return BattleManager.GetInstance().GetBattleStatus(enemiesEnum);
	}

	public BattleStatus GetEnemiesExcept(params int[] enemyIndices) {
		ENUM_BATTLE_PHASE_TARGET enemiesEnum = ALL_ENEMIES;

		foreach (var enemy in enemyObjects) {
			foreach (var index in enemyIndices) {
				if (index == enemy.enemyData.Index) {
					enemiesEnum &= ~enemy.enemyPhaseTargetEnum;
					break;
				}
			}
		}

		return BattleManager.GetInstance().GetBattleStatus(enemiesEnum);
	}

	// 적 사망 직후 '엄폐'상태인 적만 남아있다면 남은 적들의 '엄폐'를 강제로 해제함.
	private void CheckHidingEnemies() {
		int hidingCount = 0;
		foreach (var enemy in BattleEnemyObjectList) {
			if (enemy.enemyStatus.DynamicValues.isBuffFlag_003)
				hidingCount++;
		}
		if (hidingCount == EnemyCount)
			ProceedEnemyAction(ALL_ENEMIES, (enemy) => {
				if (enemy.enemyStatus.DynamicValues.isBuffFlag_003)
					enemy.enemyStatus.BuffCounter.RemoveBuff(ENUM_BUFF_INDEX.EXAMPLE_BUFF_007);
			});
	}

	public void UpdateEnemyActionUI(ENUM_BATTLE_PHASE_TARGET target = ENUM_BATTLE_PHASE_TARGET.ALL_ENEMIES) {
		ProceedEnemyAction(target, (enemy) => {
			enemy.enemyScript.UpdateEnemyActionUI();
		});
	}
}
