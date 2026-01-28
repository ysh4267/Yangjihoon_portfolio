using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 전투 시스템의 핵심 싱글톤 매니저 클래스
/// 전투 진행, UI, 상태 관리 등 모든 전투 관련 컴포넌트를 통합 관리
/// </summary>
public class BattleManager : MonoBehaviour, IDisposable {
	static BattleManager Instance;
	// 전투 진행 핵심 매니저 클래스
	[SerializeField] public BattlePhaseManager battlePhaseManager = null;       // 현재 전투 진행 단계 (전투시작 -> 적과 플레이어의 턴 진행 -> 턴 종료 와 같은 전투 사이클을 관리)
	[SerializeField] public BattlePlayerObject battlePlayerObject = null;       // 플레이어 관련 메소드 관리자
	[SerializeField] public BattleEnemyManager battleEnemyManager = null;       // 적 관련 메소드 관리자 (타겟 지정부터 적 데이터 관리까지 거의 대부분을 담당)
	[SerializeField] public BattleEventManager battleEventManager = null;       // 전투중 특정 조건에서 실행 되어야 하는 항목의 관리
	[SerializeField] public BattleCardManager battleCardManager = null;         // 플레이어의 핸드, 덱, 묘지등과 같은 카드 관련 항목을 관리

	// UI 관련 매니저 클래스
	[SerializeField] public BattleCurrentStatus playerStatusUI = null;              // 플레이어 UI를 관리
	[SerializeField] public BattleArrangeEnemy enemyArrayManager = null;            // 적 개체들의 위치를 관리
	[SerializeField] public BattleObjectPoolBundle battleObjectPool = null;         // 카드 인스턴스등의 오브젝트 풀을 관리
	[SerializeField] public BattleSkillInputSystem battleSkill = null;              // 스킬 발동을 위한 별도의 입력시스템
	[SerializeField] public BattleDescriptionPopup battleDescriptionPopup = null;   // 카드, 키워드, 장비, 스킬등의 설명 팝업을 총괄
	[SerializeField] public BattleDialogueManager battleDialogueManager = null;     // 전투 중 적과의 대화창을 관리
	[SerializeField] public BattleStartEndAnimator battleStartEndAnimator = null;   // 전투 시작과 종료 애니메이션
	[SerializeField] public Button turnEndButton = null;                            // 턴 종료 버튼 오브젝트

	// 전투 진행상황에 따른 백그라운드 데이터
	[SerializeField] public BattleCurseManager curseManager = null;                 // 특정 난이도의 저주시스템을 관리
	[SerializeField] public BattleArchive battleArchive = null;                     // 전투중 기록되어야 할 백그라운드 데이터를 관리
	[SerializeField] public BattleStatisticsManager battleStatisticsManager = null; // 전투중 플레이어의 행동 통계를 분석하기 위해 저장 (Event로써 UnityAnalytics 서버로 전송됨)

	public bool IsBattleEnd { get; private set; }
	public ENUM_BATTLE_PHASE_TARGET CurrentTurn { get; private set; }

	Dictionary<ENUM_BATTLE_PHASE_TARGET, BattleStatus> battleStatuses = new Dictionary<ENUM_BATTLE_PHASE_TARGET, BattleStatus>();

	Battle battleData = new Battle();

	// 싱글톤 인스턴스 반환
	public static BattleManager GetInstance() {
		return Instance;
	}

	// 싱글톤 인스턴스 초기화
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
	}

	// 전투 데이터 초기화 및 시작 코루틴 실행
	void Start() {
		battleData = BattleDao.GetBattle(PlayerStoryEventDao.GetPlayerStoryEvent(1).currentBattleIndex ?? 1000308) ?? BattleDao.GetBattle(1000308);
		Random.InitState(PlayerInfoDao.GetSeedValue() + battleData.Index);

		try {
			Instantiate(Resources.Load<GameObject>(battleData.battlefield.prefabData.path));
		}
		catch (Exception e) {
			Debug.Log(e.Message);
			Instantiate(Resources.Load<GameObject>("Prefabs/BattleBackground/100001_Battlefield_Bunker"));
		}
		battleStatisticsManager.Initialize();
		battleArchive.Initialize();
		battlePlayerObject.Initialize();
		battleSkill.Initialize(battlePlayerObject.battlePlayerStatus);
		battleEnemyManager.InitializeEnemyList(battleData.enemyList);
		battleCardManager.InitializeCardManager();
		battleDialogueManager.InitializeData(battleData.dialogueList);
		battleStartEndAnimator.Initialize();
		playerStatusUI.Initialize();

		//sound
		BattleBackgroundSoundPlay();
		StartCoroutine(BattleStart());
	}

	// 인스턴스 해제
	protected void OnDestroy() {
		Instance = null;
	}

	// 리소스 해제
	public void Dispose() {

	}

	#region Battle Logic
	// 전투 시작 시 초기 세팅 및 연출 진행
	IEnumerator BattleStart() {
		bool isCurseChoosed = true;

		battleStartEndAnimator.PlayBattleStart();
		yield return new WaitForSeconds(2f);

		if (SettingManager.GetSettingData().difficulty >= ENUM_DIFFICULTY.Reality) {
			isCurseChoosed = false;
			curseManager.Initialize(() => {
				isCurseChoosed = true;
			});
		}

		if (SettingManager.GetSettingData().difficulty >= ENUM_DIFFICULTY.Reality) {
			yield return new WaitUntil(() => isCurseChoosed);
			yield return new WaitForSeconds(1f);
		}

		battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.BATTLE_START);

		ProcessBattleStartAchievements();
	}

	// 전투 종료 조건 확인 및 처리
	public void CheckBattleEnd() {
		if (IsBattleEnd) return;

		if (battlePlayerObject.battlePlayerStatus.CurrentHp <= 0 || battleEnemyManager.EnemyCount == 0) {
			SetUIInteraction(false);
			BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.BATTLE_END);
			IsBattleEnd = true;
			turnEndButton.interactable = false;
			battleArchive.SaveAllChanges();

			if (battlePlayerObject.battlePlayerStatus.CurrentHp <= 0) {
				BattleEndDelay(DEAD_TYPE.PLAYER_DEAD);
				GetInstance().battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.DEAD);
			}
			else if (battleEnemyManager.EnemyCount == 0)
				BattleEndDelay(DEAD_TYPE.ENEMY_DEAD);
		}

		async void BattleEndDelay(DEAD_TYPE deadType) {
			await Task.Delay(1500);

			if (deadType == DEAD_TYPE.PLAYER_DEAD) {
				var currentDeadEndingIndex = PlayerStoryEventDao.GetPlayerStoryEvent(1).currentDeadEndingIndex;
				if (currentDeadEndingIndex == null)
					PlayerInfoDao.UpdatePlayerDeadEndingIndex(1);
				PlayerInfoDao.UpdateCurrentSceneIndex((int)ENUM_SCENE.Ending);
				battleStartEndAnimator.PlayBattleLose();
			}
			else if (deadType == DEAD_TYPE.ENEMY_DEAD) {
				ProcessBattleEndAchievements();
				battleStartEndAnimator.PlayBattleVictory();
			}
		}
	}

	// 현재 페이즈 설정
	public void SetCurrentTurn(ENUM_BATTLE_PHASE_TARGET target) {
		CurrentTurn = target;
	}
	#endregion

	#region UI
	// UI 상호작용 활성화/비활성화 설정
	public void SetUIInteraction(bool isActive) {
		if (isActive) { // 꺼져있는걸 켤 때
			if (IsBattleEnd || battleDialogueManager.isDialoguePlaying)
				return;
		}

		battleSkill.SetCollider(isActive);
		battleCardManager.SetHandCardsCollider(isActive);
	}

	// 전투 배경음악 재생
	public void BattleBackgroundSoundPlay() {
		if (battleData?.battlefield == null) {
			Debug.LogError("battleData.battlefield가 null입니다. 기본 BGM을 재생합니다.");
			SoundManager.GetInstance().PlayBattleBackgroundSound(ENUM_BATTLEFIELD.BUNKER); // 기본값 설정
			return;
		}

		if (battleData.battlefield.bgmPath != null && battleData.battlefield.bgmPath != string.Empty) {
			SoundManager.GetInstance().PlayBattleBackgroundSound(ResourceCache<AudioClip>.Load(battleData.battlefield.bgmPath));
		}
		else {
			SoundManager.GetInstance().PlayBattleBackgroundSound((ENUM_BATTLEFIELD)battleData.battlefield.Index);
		}
	}
	#endregion

	#region Battle Status
	// 특정 대상의 전투 상태 정보 반환
	public BattleStatus GetBattleStatus(ENUM_BATTLE_PHASE_TARGET targetEnum) {
		if (battleStatuses.ContainsKey(targetEnum)) {
			return battleStatuses[targetEnum];
		}
		else {
			var newStatus = new BattleStatus(targetEnum);
			battleStatuses.Add(targetEnum, newStatus); // 적이 1, 2 밖에 없는 상태에서 ALL ENEMIES를 값으로 받으면 여기에는 적 1, 2만 저장됨. 즉 목표 대상과 실제 대상의 차이가 발생.
			return newStatus;
		}
	}

	// 인터페이스 대상의 전투 상태 정보 반환
	public BattleStatus GetBattleStatus(IBattleStatus target) {
		return GetBattleStatus(target?.TargetEnum ?? ENUM_BATTLE_PHASE_TARGET.NONE);
	}

	// 전투 상태 정보에 대상 추가
	public void AddBattleStatus(IBattleStatus target) {
		if (battleStatuses.ContainsKey(target.TargetEnum)) {
			foreach (var key in battleStatuses.Keys) { // 기존의 대상이 새로운 대상으로 교체된 경우 기존의 대상을 포함한 키값도 새로 갱신해주어야 함.
				if (key.HasFlag(target.TargetEnum)) {
					battleStatuses[key].ChangeTarget(target);
				}
			}
		}
		else {
			foreach (var key in battleStatuses.Keys) { // 다중 대상 개체에 대해 목표 대상(enum값)과 실제 대상(Targets 리스트)의 차이가 발생할 수 있기 때문에 갱신 필요.
				if (key.HasFlag(target.TargetEnum)) {
					battleStatuses[key].AddTarget(target);
				}
			}
			battleStatuses.Add(target.TargetEnum, new BattleStatus(target));
		}
	}

	// 전투 상태 정보에서 대상 제거
	public void RemoveBattleStatus(IBattleStatus target) {
		battleStatuses.Remove(target.TargetEnum);
		foreach (var key in battleStatuses.Keys) { // 기존의 대상을 제거한 경우 기존의 대상을 포함한 키값도 새로 갱신해주어야 함.
			if (key.HasFlag(target.TargetEnum)) {
				battleStatuses[key].RemoveTarget(target);
			}
		}
	}
	#endregion

	#region Achievement
	// 전투 시작 시 달성되는 업적 처리
	void ProcessBattleStartAchievements() {
		// 예시업적_001 : 특정 전투 번호와 엔딩 조건 달성 시
		if (battleData.Index == 0000000) {
			bool isUnlock = false;
			for (int i = 000; i <= 000; i++) {
				if (EndingDao.IsUnlocked(i)) {
					isUnlock = true;
				}
			}
			if (isUnlock)
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_001);
		}
	}

	// 전투 승리 시 달성되는 업적 처리
	void ProcessBattleEndAchievements() {
		// 예시업적_003 : 덱 장수가 40장 이상일 때 승리
		if (PlayerCardListDao.GetPlayerDeckCount() >= 40) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_003);
		}
		// 예시업적_004 : 레벨 5 이상이고 덱 장수가 10장일 때 승리
		if (PlayerInfoDao.GetPlayerLevel() >= 5 && PlayerCardListDao.GetPlayerDeckCount() == 10) {
			AchievementEventManager.IncrementAchievementProgress(ENUM_ACHIEVEMENT.example_achievement_004);
		}
		// 예시업적_005 : 장비를 아무것도 착용하지 않았을 때 승리
		var equip = PlayerEquipDao.GetPlayerEquipInfo();
		bool isNoEquip = true;
		for (int i = 0; i < Enum.GetValues(typeof(ENUM_EQUIPMENT_PART)).Length; i++) {
			if (equip.currentEquipments[i] != null) {
				isNoEquip = false;
			}
		}
		if (isNoEquip) {
			AchievementEventManager.IncrementAchievementProgress(ENUM_ACHIEVEMENT.example_achievement_005);
		}
		// 예시업적_006 : 적이 3명 이상일 때 공격 카드를 사용하지 않고 승리
		if (battleArchive.IsThreeEnemiesBattle) {
			bool isAttackCardUsed = true;
			foreach (var useCard in battleArchive.UsedCards) {
				if (useCard.CardType == ENUM_CARD_TYPE.ATTACK) {
					isAttackCardUsed = false;
				}
			}
			if (isAttackCardUsed) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_006);
			}
		}
		// 예시업적_007 : 피해를 받지 않고 승리
		if (BattleManager.GetInstance().battleArchive.HP_DAMAGED == 0) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_007);
		}
		// 예시업적_008 : 적이 3명 이상일 때 공격 카드만 사용하여 승리
		if (battleArchive.IsThreeEnemiesBattle) {
			bool isAttackCardUsed = true;
			foreach (var useCard in battleArchive.UsedCards) {
				if (useCard.CardType != ENUM_CARD_TYPE.ATTACK) {
					isAttackCardUsed = false;
				}
			}
			if (isAttackCardUsed) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_008);
			}
		}
		// 예시업적_009 : 30턴 이상 진행 후 승리
		if (battleArchive.TurnCount >= 30) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_009);
		}
		// 예시업적_010 : 1턴 이내에 승리
		if (battleArchive.TurnCount <= 1) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_010);
		}
		// 예시업적_011 : 방어 카드가 없는 덱으로 승리
		if (PlayerCardListDao.GetPlayerDeckList().FindAll(card => card.CardType == ENUM_CARD_TYPE.SHIELD).Count == 0) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_011);
		}
		// 예시업적_012 : 특정 전투에서 승리
		if (battleData.Index == 0000000) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_012);
		}

		// 예시업적_014 : 특정 적 처치
		if (BattleDao.GetBattleEnemyIndex(battleData.Index).Contains(0000)) {
			AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_014);
		}

		//절망적인 난이도 에서
		if (SettingManager.GetSettingData().difficulty == ENUM_DIFFICULTY.DesperateReality) {
			// 예시업적_023 : 특정 적 처치 (절망적 난이도)
			if (BattleDao.GetBattleEnemyIndex(battleData.Index).Contains(0000)) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.example_achievement_023);
			}
		}
	}
	#endregion

	#region Debug
	// 강제 전투 종료 처리 (디버그용)
	public void DebugEndBattle() {
		int enemyCount = battleEnemyManager.EnemyCount;
		for (int i = 0; i < enemyCount; i++) {
			battleEnemyManager.KillEnemy(battleEnemyManager.BattleEnemyObjectList[0]);
		}
	}
	#endregion

}
