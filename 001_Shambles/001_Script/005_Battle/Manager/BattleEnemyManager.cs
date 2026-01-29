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
	[SerializeField] BattleEnemyObject[] enemyPrefabs; // 적 생성을 위한 프리팹 풀
	SortedSet<BattleEnemyObject> enemyObjects = new SortedSet<BattleEnemyObject>(new Comparer()); // 현재 활성화된 적들의 리스트 (우선순위 큐 역할)
	SortedSet<BattleEnemyObject> deadEnemyObjects = new SortedSet<BattleEnemyObject>(new Comparer()); // 비활성화된적들의 풀

	[HideInInspector] public List<BattleEnemyObject> BattleEnemyObjectList => enemyObjects.ToList(); // 외부 접근용 적 리스트
	public int EnemyCount => enemyObjects.Count; // 현재 살아있는 적 개체 수

	//for enemy turn active coroutine
	[HideInInspector] public bool isEnemyPlaying = false; // 적의 행동이 진행 중인지 여부
	WaitWhile waitWhileEnemyPlaying;
	WaitForSeconds waitForSeconds5000 = new WaitForSeconds(5f); //애니메이션 오류 방지용 타이머
	WaitForSeconds waitForSeconds1000 = new WaitForSeconds(1f);

	// 순서를 정렬하기 위한 비교자
	private class Comparer : IComparer<BattleEnemyObject> {
		public int Compare(BattleEnemyObject e1, BattleEnemyObject e2) {
			return (int)e1.enemyPhaseTargetEnum - (int)e2.enemyPhaseTargetEnum;
		}
	}

	// 시작시 적 데이터 리스트를 받아 전투 시작 시 초기 적들을 생성하고 배치
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

	// 새로운 적을 전장에 추가
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


	// 디버그용: 특정 인덱스의 적을 강제로 추가
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

	// 디버그용: 전장에 있는 가장 앞쪽 적을 강제로 제거
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

	// 적들의 턴을 순차적으로 진행
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
		
		//의도치 않은 이슈로 게임이 멈추는걸 방지하기 위한 타이머
		IEnumerator ForceTurnEnd() {
			yield return waitForSeconds5000;
			Camera.main.SetCameraBack();
		}
	}

	// 살아있는 적 중에서 타겟에 해당하는 적들에게 특정 액션을 일괄 실행
	public void ProceedEnemyAction(ENUM_BATTLE_PHASE_TARGET enemyTargets, Action<BattleEnemyObject> action) {
		for (int i = 0; i < BattleEnemyObjectList.Count; i++) {
			var targetEnemy = BattleEnemyObjectList[i];
			if (enemyTargets.HasFlag(targetEnemy.enemyPhaseTargetEnum)) {
				action(targetEnemy);

				if (!BattleEnemyObjectList.Contains(targetEnemy)) i--;
			}
		}
	}

	// 적 사망 처리 시 딜레이를 주어 부활 등의 변수를 체크
	public async void KillEnemyDelay(BattleEnemyObject enemyObject) {
		enemyObject.GetComponent<BoxCollider>().enabled = false;
		await Task.Delay(100); // 사망 처리 강제 지연. 독 데미지로 인해 체력이 0보다 작아진 직후 체력을 회복하는 등 되살아날 수 있는 판정에 대해 예외처리.
		if (enemyObject.enemyStatus.CurrentHp > 0) {
			enemyObject.GetComponent<BoxCollider>().enabled = true;
			return;
		}

		KillEnemy(enemyObject);
	}

	// 적 사망 확정 처리
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

	// 필드 위의 적 개체를 다른 적으로 교체
	public void ChangeEnemy(BattleEnemyObject fromObject, int toIndex) {
		var toObject = EnemyDao.GetEnemy(toIndex);
		Destroy(fromObject.enemySprite);
		fromObject.enemyStatus.BuffCounter.ClearBuffs();
		fromObject.Initialize(toObject);
	}

	// 카드를 드래그 하는 등 타겟팅 상태일 때 적에게 타겟팅 효과 표시
	public void SetEnemiesTargeted(ENUM_BATTLE_PHASE_TARGET? target, IBattleFactor factor, bool isTargeting, BattleDamage battleDamage = default) {
		if (target == null) return;

		foreach (var enemy in enemyObjects) {
			if (target.Value.HasFlag(enemy.enemyPhaseTargetEnum))
				enemy.SetTargeted(factor, isTargeting, battleDamage);
		}
	}

	// 특정 타겟 Enum에 해당하는 적 객체 반환
	public BattleEnemyObject GetEnemyObject(ENUM_BATTLE_PHASE_TARGET target) {
		foreach (var enemy in enemyObjects) {
			if (target.Equals(enemy.enemyPhaseTargetEnum))
				return enemy;
		}

		return null;
	}

	// 특정 데이터 인덱스를 가진 적 객체 반환
	public BattleEnemyObject GetEnemyObject(int enemyIndex) {
		foreach (var enemy in enemyObjects) {
			if (enemyIndex == enemy.enemyData.Index)
				return enemy;
		}

		return null;
	}

	// 특정 인덱스를 가진 적들을 찾아 합쳐진 BattleStatus 반환
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

	// 특정 인덱스를 제외한 나머지 적들을 찾아 합쳐진 BattleStatus 반환
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

	// 적들의 다음 행동 UI를 갱신
	public void UpdateEnemyActionUI(ENUM_BATTLE_PHASE_TARGET target = ENUM_BATTLE_PHASE_TARGET.ALL_ENEMIES) {
		ProceedEnemyAction(target, (enemy) => {
			enemy.enemyScript.UpdateEnemyActionUI();
		});
	}
}
