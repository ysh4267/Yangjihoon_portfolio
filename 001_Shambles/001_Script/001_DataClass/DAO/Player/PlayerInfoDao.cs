using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public class PlayerInfoDao {
	private static readonly int playerInfoIndex = 1;//현재는 하나의 데이터만 저장하기로
	static readonly string nullString = "NULL";

	#region Player Info
	// 기본 플레이어 정보 인덱스 반환
	public static int GetDefaultPlayerInfoIndex() {
		return playerInfoIndex;
	}

	// 플레이어 원시 정보 조회
	public static PlayerRawInfo GetPlayerRawInfo() {
		PlayerRawInfo playerRawInfo = new PlayerRawInfo();
		playerRawInfo.playerInfoIndex = playerInfoIndex;

		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.PlayerDeckTable}.temporary_id, " +
			$"{DataBaseTableDefine.PlayerDeckTable}.card_index, " +
			$"{DataBaseTableDefine.PlayerDeckTable}.is_fixed_in_deck, " +
			$"{DataBaseTableDefine.PlayerDeckTable}.is_in_deck " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable}";

		using (CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA)) {
			if (it.Read()) {
				playerRawInfo.playerDeckIndexList = new List<CardLiteDBData>();
				playerRawInfo.playerBagIndexList = new List<CardLiteDBData>();

				do {
					if (it.GetBoolFromString(3) == true) {
						playerRawInfo.playerDeckIndexList.Add(
							new CardLiteDBData(
								it.GetSafeValue<int>(0),
								it.GetSafeValue<int>(1),
								it.GetBoolFromString(2),
								it.GetBoolFromString(3)
							));
					}
					else {
						playerRawInfo.playerBagIndexList.Add(
							new CardLiteDBData(
								it.GetSafeValue<int>(0),
								it.GetSafeValue<int>(1),
								it.GetBoolFromString(2),
								it.GetBoolFromString(3)
							));
					}
				} while (it.Read());
			}
			it.Close();
		}

		query =
		$"SELECT " +
		$"{DataBaseTableDefine.PlayerStatusTable}.name AS 'name', " +
		$"{DataBaseTableDefine.PlayerStatusTable}.gold AS 'gold', " +
		$"{DataBaseTableDefine.PlayerStatusTable}.level AS 'level', " +
		$"{DataBaseTableDefine.PlayerStatusTable}.exp AS 'exp', " +
		$"{DataBaseTableDefine.PlayerStatusTable}.skill_point AS 'skill_point', " +
		$"{DataBaseTableDefine.PlayerStatusTable}.current_hp AS 'current_hp' " +
		$"FROM {DataBaseTableDefine.PlayerStatusTable} WHERE {DataBaseTableDefine.PlayerStatusTable}.player_index = {playerInfoIndex}";

		using (CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA)) {
			if (false == it.Read()) {
				Debug.LogError($"{DataBaseTableDefine.PlayerStatusTable} is NULL");
				return default;
			}

			playerRawInfo.name = it.GetSafeValue<string>(0);
			playerRawInfo.gold = it.GetSafeValue<int>(1);
			playerRawInfo.level = it.GetSafeValue<int>(2);
			playerRawInfo.exp = it.GetSafeValue<int>(3);
			playerRawInfo.skillPoint = it.GetSafeValue<int>(4);
			playerRawInfo.currentHp = it.GetSafeValue<int>(5);

			it.Close();
		}

		var battleStatus = PlayerBattleStatusDao.GetPlayerBattleStatus();
		playerRawInfo.statHp = battleStatus.status[(int)ENUM_STATUS.HP];
		playerRawInfo.statStr = battleStatus.status[(int)ENUM_STATUS.STR];
		playerRawInfo.statDex = battleStatus.status[(int)ENUM_STATUS.DEX];
		playerRawInfo.statInt = battleStatus.status[(int)ENUM_STATUS.INT];
		playerRawInfo.statAp = battleStatus.max_ap;
		playerRawInfo.statExtraDraw = battleStatus.extra_draw;
		playerRawInfo.maxHp = battleStatus.max_hp;
		playerRawInfo.maxAp = battleStatus.max_ap;
		playerRawInfo.currentAp = battleStatus.current_ap;

		query =
		  $"SELECT " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.player_index, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.cleared_area_list, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_equipment_list, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_active_skill_list, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_passive_skill_list, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_event_list, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.new_unlocked_journal_list, " +
		  $"{DataBaseTableDefine.PlayerUnlockListTable}.new_unlocked_character_list " +
		  $"FROM {DataBaseTableDefine.PlayerUnlockListTable} WHERE {DataBaseTableDefine.PlayerUnlockListTable}.player_index = {playerInfoIndex}";

		using (CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA)) {
			if (false == it.Read()) {
				Debug.LogError($"{DataBaseTableDefine.PlayerUnlockListTable} is NULL");
				return default;
			}

			playerRawInfo.clearedAreaIndexList = it.GetTextValueToIntList(1);
			playerRawInfo.unlockedEquipmentIndexList = it.GetTextValueToIntList(2);
			playerRawInfo.unlockedActiveSkillIndexList = it.GetTextValueToIntList(3);
			playerRawInfo.unlockedPassiveSkillIndexList = it.GetTextValueToIntList(4);
			playerRawInfo.playedEventIndexList = it.GetTextValueToIntList(5);
			playerRawInfo.newUnlockedJournalIndexList = it.GetTextValueToIntList(6);
			playerRawInfo.newUnlockedCharacterIndexList = it.GetTextValueToIntList(7);
			it.Close();
		}

		query =
			$"SELECT* FROM {DataBaseTableDefine.PlayerInfoTable} WHERE player_index = {playerInfoIndex}";

		using (CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA)) {
			if (false == it.Read()) {
				Debug.LogError($"{DataBaseTableDefine.PlayerInfoTable} is NULL");
				return default;
			}

			playerRawInfo.playerSkillIndex = it.GetSafeValue<int>(1);
			playerRawInfo.starterPackIndex = it.GetSafeValue<int?>(2);
			playerRawInfo.portraitIndex = it.GetSafeValue<int?>(3);
			playerRawInfo.currentSceneIndex = it.GetSafeValue<int?>(4);
			playerRawInfo.seedValue = it.GetSafeValue<int?>(5);
			playerRawInfo.currentMapIndex = it.GetSafeValue<int?>(6);

			it.Close();
		}

		query =
		$"SELECT " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.player_index AS 'player_index', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.story_event_index_queue_list AS 'story_event_index_queue_list', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_area_index AS 'current_area_index', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_event_index AS 'current_event_index', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_battle_index AS 'current_battle_index', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.story_selection_index_list AS 'story_selection_index_list', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_event_count AS 'current_event_count', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_ending_index_list AS 'current_ending_index_list', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_dead_ending_index AS 'current_dead_ending_index', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_dead_ending_sentence_index AS 'current_dead_ending_sentence_index', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_revealed_area_list AS 'current_revealed_area_list', " +
		$"{DataBaseTableDefine.PlayerStoryEventTable}.current_encounter_reroll_count AS 'current_encounter_reroll_count' " +
		$"FROM {DataBaseTableDefine.PlayerStoryEventTable} WHERE {DataBaseTableDefine.PlayerStoryEventTable}.player_index = {playerInfoIndex}";

		using (CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA)) {
			if (false == it.Read()) {
				Debug.LogError($"{DataBaseTableDefine.PlayerStoryEventTable} is NULL");
				return default;
			}

			playerRawInfo.queuedEventIndexList = it.GetTextValueToIntList(1);
			playerRawInfo.currentAreaIndex = it.GetSafeValue<int?>(2);
			playerRawInfo.currentEventIndex = it.GetSafeValue<int?>(3);
			playerRawInfo.currentBattleIndex = it.GetSafeValue<int?>(4);
			playerRawInfo.selectedStorySelectionIndexList = it.GetTextValueToIntList(5);
			playerRawInfo.currentEventCount = it.GetSafeValue<int?>(6);
			playerRawInfo.currentEndingIndexList = it.GetTextValueToIntList(7);
			playerRawInfo.currentDeadEndingIndex = it.GetSafeValue<int?>(8);
			playerRawInfo.currentDeadEndingSentenceIndex = it.GetSafeValue<int?>(9);
			playerRawInfo.currentRevealedAreaIndexList = it.GetTextValueToIntList(10);
			playerRawInfo.currentEncounterRerollCount = it.GetSafeValue<int?>(11);

			it.Close();
		}

		query =
			$"SELECT* FROM {DataBaseTableDefine.PlayerEquipTable} WHERE player_index = {playerInfoIndex}";

		using (CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA)) {
			if (false == it.Read()) {
				Debug.LogError($"{DataBaseTableDefine.PlayerEquipTable} is NULL");
				return default;
			}

			//Equipment
			int?[] EquipmentData = new int?[(int)System.Enum.GetValues(typeof(ENUM_EQUIPMENT_PART)).Length];
			EquipmentData[(int)ENUM_EQUIPMENT_PART.HEAD] = it.GetSafeValue<int?>(1);
			EquipmentData[(int)ENUM_EQUIPMENT_PART.SHIRT] = it.GetSafeValue<int?>(2);
			EquipmentData[(int)ENUM_EQUIPMENT_PART.PANTS] = it.GetSafeValue<int?>(3);
			EquipmentData[(int)ENUM_EQUIPMENT_PART.WEAPON] = it.GetSafeValue<int?>(4);
			EquipmentData[(int)ENUM_EQUIPMENT_PART.TRINKET] = it.GetSafeValue<int?>(5);
			EquipmentData[(int)ENUM_EQUIPMENT_PART.ETC] = it.GetSafeValue<int?>(6);

			playerRawInfo.equipedEquipment = EquipmentData;

			it.Close();
		}

		return playerRawInfo;
	}

	// 플레이어 정보 조회
	public static PlayerInfo GetPlayerInfo() {
		PlayerInfo playerInfo = new PlayerInfo();
		playerInfo.playerInfoIndex = playerInfoIndex;
		//player_card_list
		playerInfo.cardDeckList = PlayerCardListDao.GetPlayerDeckList(playerInfoIndex);
		playerInfo.cardBagList = PlayerCardListDao.GetPlayerBagList();

		//player_status
		PlayerStatus status = PlayerStatusDao.GetPlayerStatus(playerInfoIndex);
		playerInfo.name = status.name;
		playerInfo.gold = status.gold;
		playerInfo.level = status.level;
		playerInfo.exp = status.exp;
		playerInfo.skillPoint = status.skillPoint;

		//player_battle_status
		PlayerBattleStatus battleStatus = PlayerBattleStatusDao.GetPlayerBattleStatus(playerInfoIndex);
		playerInfo.statHp = battleStatus.status[(int)ENUM_STATUS.HP];
		playerInfo.statStr = battleStatus.status[(int)ENUM_STATUS.STR];
		playerInfo.statInt = battleStatus.status[(int)ENUM_STATUS.INT];
		playerInfo.statDex = battleStatus.status[(int)ENUM_STATUS.DEX];
		playerInfo.statAp = battleStatus.max_ap;
		playerInfo.statExtraDraw = battleStatus.extra_draw;
		playerInfo.maxHp = battleStatus.max_hp;
		playerInfo.currentHp = battleStatus.current_hp;
		playerInfo.maxMp = battleStatus.max_ap;
		playerInfo.currentMp = battleStatus.current_ap;

		//player_unlockList
		PlayerUnlockList unlockList = PlayerUnlockListDao.GetPlayerUnlockList(playerInfoIndex);

		List<Skill> skillList = new List<Skill>();
		for (int i = 0; i < unlockList.unlockedActiveSkillList.Count; i++) {
			skillList.Add(SkillDao.GetActiveSkillInfo(unlockList.unlockedActiveSkillList[i]));
		}

		List<Equipment> equipmentList = new List<Equipment>();
		for (int i = 0; i < unlockList.unlockedEquipmentList.Count; i++) {
			equipmentList.Add(EquipmentDao.GetEquipmentInfo(unlockList.unlockedEquipmentList[i]));
		}

		List<Area> areaList = new List<Area>();
		for (int i = 0; i < unlockList.clearedAreaList.Count; i++) {
			areaList.Add(AreaDao.GetArea(unlockList.clearedAreaList[i]));
		}

		List<StoryEvent> eventList = new List<StoryEvent>();
		for (int i = 0; i < unlockList.playedStoryEventList.Count; i++) {
			eventList.Add(StoryEventDao.GetStoryEvent(unlockList.playedStoryEventList[i]));
		}

		playerInfo.unlockedSkillList = skillList;
		playerInfo.unlockedEquipmentList = equipmentList;
		playerInfo.clearedAreaList = areaList;
		playerInfo.playedEventList = eventList;
		playerInfo.newUnlockedJournalIndexList = unlockList.newUnlockedJournalList;
		playerInfo.newUnlockedCharacterIndexList = unlockList.newUnlockedCharacterList;

		// Player Info Table
		string query =
			$"SELECT* FROM {DataBaseTableDefine.PlayerInfoTable} WHERE player_index = {playerInfoIndex}";
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			Debug.LogError($"{DataBaseTableDefine.PlayerInfoTable} is NULL");
			return default;
		}

		//Skill
		int skillNumber = it.GetSafeValue<int>(1);
		Skill skill = SkillDao.GetActiveSkillInfo(skillNumber);
		playerInfo.playerSkill = skill;

		//Starter Pack
		int starterPackNumber = it.GetSafeValue<int>(2);
		StarterPack starterPack = StarterPackDao.GetStarterPack(starterPackNumber);
		playerInfo.starterPack = starterPack;

		//Portrait
		int PortraitNumber = it.GetSafeValue<int>(3);
		Portrait portrait = PortraitDao.GetPortraitInfo(PortraitNumber);
		playerInfo.portrait = portrait;

		playerInfo.currentSceneIndex = it.GetSafeValue<int?>(4);
		playerInfo.seedValue = it.GetSafeValue<int?>(5);
		playerInfo.currentMapIndex = it.GetSafeValue<int?>(6);

		PlayerStoryEvent playerStoryEvent = PlayerStoryEventDao.GetPlayerStoryEvent(playerInfoIndex);
		playerInfo.queuedEventList = playerStoryEvent?.storyEventIndexQueueList;
		playerInfo.currentAreaIndex = playerStoryEvent?.currentAreaIndex;
		playerInfo.currentEventIndex = playerStoryEvent?.currentEventIndex;
		playerInfo.currentBattleIndex = playerStoryEvent?.currentBattleIndex;
		playerInfo.selectedStorySelectionIndexList = playerStoryEvent?.selectedStorySelectionIndexList;
		playerInfo.currentEventCount = playerStoryEvent?.currentEventCount;
		playerInfo.currentEndingIndexList = playerStoryEvent?.currentEndingIndexList;
		playerInfo.currentDeadEndingIndex = playerStoryEvent?.currentDeadEndingIndex;
		playerInfo.currentRevealedAreaIndexList = playerStoryEvent?.currentRevealedAreaIndexList;
		playerInfo.currentEncounterRerollCount = playerStoryEvent?.currentEncounterRerollCount;

		query =
			$"SELECT* FROM {DataBaseTableDefine.PlayerEquipTable} WHERE player_index = {playerInfoIndex}";

		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		//Equipment
		playerInfo.equipmentStatusDict[ENUM_EQUIPMENT_PART.HEAD] = EquipmentDao.GetEquipmentInfo(it.GetSafeValue<int>(1));
		playerInfo.equipmentStatusDict[ENUM_EQUIPMENT_PART.SHIRT] = EquipmentDao.GetEquipmentInfo(it.GetSafeValue<int>(2));
		playerInfo.equipmentStatusDict[ENUM_EQUIPMENT_PART.PANTS] = EquipmentDao.GetEquipmentInfo(it.GetSafeValue<int>(3));
		playerInfo.equipmentStatusDict[ENUM_EQUIPMENT_PART.WEAPON] = EquipmentDao.GetEquipmentInfo(it.GetSafeValue<int>(4));
		playerInfo.equipmentStatusDict[ENUM_EQUIPMENT_PART.TRINKET] = EquipmentDao.GetEquipmentInfo(it.GetSafeValue<int>(5));
		playerInfo.equipmentStatusDict[ENUM_EQUIPMENT_PART.ETC] = EquipmentDao.GetEquipmentInfo(it.GetSafeValue<int>(6));

		return playerInfo;
	}

	// 플레이어 정보 설정
	public static void SetPlayerInfo(PlayerRawInfo playerRawInfo) {
		if (playerRawInfo == null) return;
		UpdatePlayerPortrait(playerRawInfo.portraitIndex);
		UpdatePlayerName(playerRawInfo.name);
		UpdateStarterPack(playerRawInfo.starterPackIndex);
		UpdatePlayerCurrentHp(playerRawInfo.currentHp);
		SetPlayerCards(playerRawInfo.playerDeckIndexList, playerRawInfo.playerBagIndexList);
		//UpdateCurrentCardList(playerRawInfo.cardIndexList.ListToListQueryString());
		UpdateSkillEquipInfo(playerRawInfo.playerSkillIndex);
		UpdateEquipmentEquipInfo(playerRawInfo.equipedEquipment);
		UpdatePlayerGold(playerRawInfo.gold);
		UpdateClearedAreaList(playerRawInfo.clearedAreaIndexList);
		UpdateUnlockedEquipmentList(playerRawInfo.unlockedEquipmentIndexList.ListToListQueryString());
		UpdateUnlockedSkillList(playerRawInfo.unlockedActiveSkillIndexList.ListToQueryString(), playerRawInfo.unlockedPassiveSkillIndexList.ListToQueryString(), playerRawInfo.skillPoint);
		UpdateUnlockedEventList(playerRawInfo.playedEventIndexList.ListToListQueryString());
		UpdateNewUnlockedCharacterList(playerRawInfo.newUnlockedCharacterIndexList);
		UpdateNewUnlockedJournalList(playerRawInfo.newUnlockedJournalIndexList);
		UpdateEventIndexQueueList(playerRawInfo.queuedEventIndexList.ListToListQueryString());
		UpdateCurrentAreaIndex(playerRawInfo.currentAreaIndex);
		UpdateCurrentStoryEventIndex(playerRawInfo.currentEventIndex);
		UpdateCurrentBattleIndex(playerRawInfo.currentBattleIndex);
		UpdateCurrentEventCount(playerRawInfo.currentEventCount);
		UpdateCurrentSelectedSelectionList(playerRawInfo.selectedStorySelectionIndexList.ListToListQueryString());
		UpdatePlayerEndingIndexList(playerRawInfo.currentEndingIndexList.ListToListQueryString());
		UpdatePlayerDeadEndingIndex(playerRawInfo.currentDeadEndingIndex);
		UpdatePlayerDeadEndingSentenceIndex(playerRawInfo.currentDeadEndingSentenceIndex);
		UpdateRandomSeedValue(playerRawInfo.seedValue);
		UpdateCurrentSceneIndex(playerRawInfo.currentSceneIndex);
		UpdateCurrentMapIndex(playerRawInfo.currentMapIndex);
		UpdateRevealedAreaList(playerRawInfo.currentRevealedAreaIndexList.ListToListQueryString());
		UpdateCurrentEncounterRerollCount(playerRawInfo.currentEncounterRerollCount);
		UpdatePlayerStatistics(playerRawInfo.statistics);
	}

	// 스타터 팩 정보 조회
	public static int GetStarterPackInfo(int playerIndex = 1) {
		string query =
			$"SELECT {DataBaseTableDefine.PlayerInfoTable}.starter_pack_index " +
			$"FROM {DataBaseTableDefine.PlayerInfoTable} " +
			$"WHERE {DataBaseTableDefine.PlayerInfoTable}.player_index = {playerIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}

		return it.GetSafeValue<int>(0);
	}
	// 플레이어 스킬 정보 조회
	public static int GetPlayerSkillInfo(int playerIndex = 1) {
		string query =
			$"SELECT {DataBaseTableDefine.PlayerInfoTable}.skill_index AS 'skill_index' " +
			$"FROM {DataBaseTableDefine.PlayerInfoTable} " +
			$"WHERE {DataBaseTableDefine.PlayerInfoTable}.player_index = {playerIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}

		return it.GetSafeValue<int>(0);
	}

	// 플레이어 초상화 인덱스 조회
	public static int? GetPlayerPortraitIndex(int playerIndex = 1) {
		string query =
			$"SELECT {DataBaseTableDefine.PlayerInfoTable}.portrait_index AS 'portrait_index' " +
			$"FROM {DataBaseTableDefine.PlayerInfoTable} " +
			$"WHERE {DataBaseTableDefine.PlayerInfoTable}.player_index = {playerIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) return default;

		return it.GetSafeValue<int?>(0);
	}

	// 플레이어 스킬 포인트 조회
	public static int GetPlayerSkillPoint(int playerIndex = 1) {
		string query =
			 $"SELECT {DataBaseTableDefine.PlayerStatusTable}.skill_point AS 'skill_point' " +
			 $"FROM {DataBaseTableDefine.PlayerStatusTable} " +
			 $"WHERE {DataBaseTableDefine.PlayerStatusTable}.player_index = {playerIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}

		return it.GetSafeValue<int>(0);
	}

	// 플레이어 레벨 조회
	public static int GetPlayerLevel(int playerIndex = 1) {
		string query =
			$"SELECT level " +
			$"FROM {DataBaseTableDefine.PlayerStatusTable} " +
			$"WHERE player_index = {playerIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}

		return it.GetSafeValue<int>(0);
	}
	#endregion

	// 시드 값 조회
	public static int GetSeedValue() {
		string query =
			$"SELECT {DataBaseTableDefine.PlayerInfoTable}.seed_value AS 'seed_value' " +
			$"FROM {DataBaseTableDefine.PlayerInfoTable} " +
			$"WHERE {DataBaseTableDefine.PlayerInfoTable}.player_index = {playerInfoIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}

		return it.GetSafeValue<int>(0);
	}

	// 랜덤 시드 값 업데이트
	public static int UpdateRandomSeedValue() {
		int ran = Random.Range(0, 90000000);
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"seed_value = {ran} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		return ran;
	}

	// 랜덤 시드 값 업데이트 (값 지정)
	public static int UpdateRandomSeedValue(int? value) {
		if (value == null) value = Random.Range(0, 90000000);
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"seed_value = {value} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		return value.Value;
	}

	// 현재 씬 인덱스 업데이트
	public static void UpdateCurrentSceneIndex(int? currentSceneIndex = null) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"current_scene_index = {currentSceneIndex.ValueToString()} " +
			$"WHERE player_index = {playerInfoIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	#region Generation Scene

	// 플레이어 이름 업데이트
	public static void UpdatePlayerName(string name) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"name = '{name ?? nullString}' " +
			$"WHERE player_index = {playerInfoIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 플레이어 초상화 업데이트
	public static void UpdatePlayerPortrait(Portrait portrait) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"portrait_index = {portrait?.Index.ToString() ?? nullString} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 플레이어 초상화 업데이트 (인덱스)
	public static void UpdatePlayerPortrait(int? portraitIndex) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"portrait_index = {portraitIndex.Value.ToString() ?? nullString} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 스타터 팩 업데이트
	public static void UpdateStarterPack(StarterPack starterPack) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"starter_pack_index = {starterPack?.Index.ToString() ?? nullString} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 스타터 팩 업데이트 (인덱스)
	public static void UpdateStarterPack(int? starterPackIndex) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"starter_pack_index = {starterPackIndex.Value.ToString() ?? nullString} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}


	// 현재 스킬 포인트 업데이트
	public static void UpdateCurrentSkillPoint(int skillPoint) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"skill_point = {skillPoint} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}
	#endregion

	#region InsideArea Scene

	// 현재 스토리 이벤트 인덱스 업데이트
	public static void UpdateCurrentStoryEventIndex(int? currentEventIndex = null) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_event_index = {currentEventIndex.ValueToString()} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 현재 선택된 선택지 리스트 업데이트
	public static void UpdateCurrentSelectedSelectionList(string selectionListString = null) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"story_selection_index_list = {selectionListString.ValueToString()} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 이벤트 인덱스 큐 리스트 업데이트
	public static void UpdateEventIndexQueueList(string eventListString = null) {
		string query =
		$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
		$"story_event_index_queue_list = {eventListString.ValueToString()} " +
		$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 현재 전투 인덱스 업데이트
	public static void UpdateCurrentBattleIndex(int? battleIndex) {
		string query =
		$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
		$"current_battle_index = {battleIndex.ValueToString()} " +
		$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}
	// return levelUpCount
	public static int UpdatePlayerExp(int? expAmount = 0) {
		string query;
		CustomDataReader it;

		PlayerStatus status = PlayerStatusDao.GetPlayerStatus(playerInfoIndex);
		int levelUpCount = 0;
		int expCapacity = LevelUpExpDao.GetExpDataByLevel(status.level).requiredExp;
		int _currentExp = status.exp + expAmount.Value;

		while (_currentExp >= expCapacity) {
			levelUpCount++;
			_currentExp -= expCapacity;
			UpdatePlayerLevelUp();
		}

		query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"exp = {_currentExp} " +
			$"WHERE player_index = {playerInfoIndex} ";

		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		return levelUpCount;
	}

	// 플레이어 레벨업 처리
	public static void UpdatePlayerLevelUp() {
		string query;
		CustomDataReader it;

		int skillPointGainValue = 1;
		if (GetPlayerRawInfo().portraitIndex == 9) // 카비르 초상화 효과
			skillPointGainValue = Random.Range(0, 4);

		query =
		$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
		$"level = level + 1, " +
		$"skill_point = skill_point + {skillPointGainValue} " +
		$"WHERE player_index = {playerInfoIndex} ";

		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

	}

	// 플레이어 현재 HP 업데이트
	public static void UpdatePlayerCurrentHp(int currentHp) {
		var query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"current_hp = {currentHp} " +
			$"WHERE player_index = {playerInfoIndex} ";

		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 플레이어 데드 엔딩 인덱스 업데이트
	public static void UpdatePlayerDeadEndingIndex(int? endingIndex) {
		string query;
		CustomDataReader it;
		if (endingIndex.HasValue) {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_dead_ending_index = {endingIndex} " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
		else {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_dead_ending_index = NULL " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
	}

	// 플레이어 데드 엔딩 문장 인덱스 업데이트
	public static void UpdatePlayerDeadEndingSentenceIndex(int? sentenceIndex) {
		string query;
		CustomDataReader it;
		if (sentenceIndex.HasValue) {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_dead_ending_sentence_index = {sentenceIndex} " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
		else {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_dead_ending_sentence_index = NULL " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
	}

	// 플레이어 엔딩 인덱스 리스트 업데이트
	public static void UpdatePlayerEndingIndexList(string endingListString) {
		string query;
		CustomDataReader it;
		if (endingListString.Length != 0) {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_ending_index_list = {endingListString} " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
		else {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_ending_index_list = NULL " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
	}

	// 선택 보상 업데이트
	public static void UpdateSelectionReward(int? rewardGold = null, List<int> rewardCardList = null, string playerUnlockedEquipmentList = null, Reputation changedReputation = null, List<int> unlockedJounalIndexList = null, List<int> unlockedCharacterIndexList = null, int? hp = null) {
		string query;
		CustomDataReader it;
		PlayerRawInfo info = PlayerInfoDao.GetPlayerRawInfo();
		if (rewardGold.HasValue) {
			int gold = info.gold + (rewardGold ?? 0);

			query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"gold = {gold} " +
			$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
			PlayerStatisticsDao.AddValue(ENUM_PLAYER_STATISTICS_TYPE.GAINED_GOLD, rewardGold.Value);

			#region 골드 획득 업적
			AchievementEventManager.SetAchievementsProgress(gold, ENUM_ACHIEVEMENT.Show_Me_The_Money, ENUM_ACHIEVEMENT.Greed_Is_Good, ENUM_ACHIEVEMENT.Millionaires_Dream);
			#endregion
		}

		if (rewardCardList.HasValue()) {
			PlayerCardListDao.AddNewCardsInBag(rewardCardList);
			foreach (var card in rewardCardList)
				CardDao.Unlock(card);
		}


		if (!playerUnlockedEquipmentList.IsNullQueryString()) {
			query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"unlocked_equipment_list = {playerUnlockedEquipmentList} " +
			$"WHERE player_index = {playerInfoIndex}";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}

		if (changedReputation != null) {
			query =
			$"UPDATE {DataBaseTableDefine.ReputationTable} SET " +
			$"reputation_value = {changedReputation.value} " +
			$"WHERE reputation_index = {changedReputation.Index} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

			#region 평판 획득 업적
			// 공공의 적: 임페리얼과 산마지카에게 동시에 수배되십시오. 평판 10이 30이하, 평판 13이 30 이하
			if (ReputationDao.GetReputation(10).value <= -30 && ReputationDao.GetReputation(13).value <= -30) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.Public_Enemy);
			}
			#endregion
		}

		if (unlockedJounalIndexList != null) {
			List<int> newUnlockedList = new List<int>(unlockedJounalIndexList);

			foreach (var index in unlockedJounalIndexList) {
				if (JournalDao.Unlock(index) == false) {
					newUnlockedList.Remove(index);
				}
			}
			query =
				$"SELECT new_unlocked_journal_list " +
				$"FROM {DataBaseTableDefine.PlayerUnlockListTable} " +
				$"WHERE player_index = {playerInfoIndex}";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

			var list = it.GetTextValueToIntList(0);
			newUnlockedList.AddRange(list);

			query =
				$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
				$"new_unlocked_journal_list = {newUnlockedList.ListToListQueryString()} " +
				$"WHERE player_index = {playerInfoIndex}";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

			#region 일지 획득 업적
			// 벙커 전문가 : 벙커에서 얻을 수 있는 모든 일지를 획득하십시오. 일지 1번~7번
			if (JournalDao.IsUnlocked(1) && JournalDao.IsUnlocked(2) && JournalDao.IsUnlocked(3) && JournalDao.IsUnlocked(4) && JournalDao.IsUnlocked(5) && JournalDao.IsUnlocked(6) && JournalDao.IsUnlocked(7)) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.Bunker_Expert);
			}
			// 훈장 수집가 : 일지 43,46 획득
			// 일지를 얻었을 때, 이미 획득한 업적이라면 카운트를 증가시키지 않는다.
			if (JournalDao.IsUnlocked(43) && JournalDao.IsUnlocked(46)) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.Medal_Collector);
			}
			// 노망난 셰익스피어: 일지 49,50,51 획득
			if (JournalDao.IsUnlocked(49) && JournalDao.IsUnlocked(50) && JournalDao.IsUnlocked(51)) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.Apocalyptic_Shakespeare);
			}
			// 이웃 집들이: 일지 64 획득
			if (JournalDao.IsUnlocked(64)) {
				AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.Neighbors_Housewarming);
			}
			// 책벌레: 일지를 30개 이상 획득, 유스테아 도서관: 일지를 60개 이상 획득
			AchievementEventManager.SetAchievementsProgress(JournalDao.GetUnlockedCount(), ENUM_ACHIEVEMENT.Bookworm, ENUM_ACHIEVEMENT.Library_Of_Eustea);
			#endregion
		}

		if (unlockedCharacterIndexList != null) {
			List<int> newUnlockedList = new List<int>(unlockedCharacterIndexList);

			foreach (var index in unlockedCharacterIndexList) {
				if (CharacterDao.Unlock(index) == false) {
					newUnlockedList.Remove(index);
				}
			}

			query =
				$"SELECT new_unlocked_character_list " +
				$"FROM {DataBaseTableDefine.PlayerUnlockListTable} " +
				$"WHERE player_index = {playerInfoIndex}";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

			var list = it.GetTextValueToIntList(0);
			newUnlockedList.AddRange(list);

			query =
				$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
				$"new_unlocked_character_list = {newUnlockedList.ListToListQueryString()} " +
				$"WHERE player_index = {playerInfoIndex}";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

			#region 캐릭터 도감 획득 업적
			// 캐릭터 도감 획득 업적
			AchievementEventManager.SetAchievementsProgress(CharacterDao.GetUnlockedCount(), ENUM_ACHIEVEMENT.A_Great_Step_Forward, ENUM_ACHIEVEMENT.Walking_Encyclopedia, ENUM_ACHIEVEMENT.Akinator);
			#endregion
		}

		if (hp != null) {
			PlayerRawInfo playerRawInfo = GetPlayerRawInfo();

			int mod_hp = playerRawInfo.currentHp + hp.Value;

			if (mod_hp > playerRawInfo.maxHp) {
				mod_hp = playerRawInfo.maxHp;
			}
			if (mod_hp < 0) {
				mod_hp = 0;
			}

			query =
				$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
				$"current_hp = {mod_hp} " +
				$"WHERE player_index = {playerInfoIndex} ";

			it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
			PlayerStatisticsDao.AddValue(ENUM_PLAYER_STATISTICS_TYPE.RECOVERED_HP, mod_hp - playerRawInfo.currentHp);
		}
	}

	// 새로 해금된 캐릭터 추가
	public static void UpdateNewUnlockedCharacterList(int characterIndex) {
		if (CharacterDao.Unlock(characterIndex) == false) {
			return;
		}

		var query =
			$"SELECT new_unlocked_character_list " +
			$"FROM {DataBaseTableDefine.PlayerUnlockListTable} " +
			$"WHERE player_index = {playerInfoIndex}";

		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		var list = it.GetTextValueToIntList(0);
		list.AddDistinct(characterIndex);

		query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"new_unlocked_character_list = {list.ListToListQueryString()} " +
			$"WHERE player_index = {playerInfoIndex}";

		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 새로 해금된 캐릭터 리스트 업데이트
	public static void UpdateNewUnlockedCharacterList(List<int> newUnlockedList) {
		if (!newUnlockedList.HasValue()) return;

		var query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"new_unlocked_character_list = {newUnlockedList.ListToListQueryString()} " +
			$"WHERE player_index = {playerInfoIndex}";

		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 새로 해금된 일지 리스트 업데이트
	public static void UpdateNewUnlockedJournalList(List<int> newUnlockedList) {
		if (!newUnlockedList.HasValue()) return;

		var query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"new_unlocked_journal_list = {newUnlockedList.ListToListQueryString()} " +
			$"WHERE player_index = {playerInfoIndex}";

		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 해금된 이벤트 리스트 업데이트
	public static void UpdateUnlockedEventList(string eventIndexList = null) {
		string query =
		$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
		$"unlocked_event_list = {eventIndexList.ValueToString()} " +
		$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 공개된 지역 리스트 업데이트
	public static void UpdateRevealedAreaList(string areaIndexList = null) {
		string query =
		$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
		$"current_revealed_area_list = {areaIndexList.ValueToString()} " +
		$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 현재 인카운터 리롤 횟수 업데이트
	public static void UpdateCurrentEncounterRerollCount(int? count = null) {
		if (count.HasValue == false) count = 0;
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_encounter_reroll_count = {count.Value} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 현재 이벤트 카운트 업데이트
	public static void UpdateCurrentEventCount(int? currentEventCount = null) {
		string query =
		$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
		$"current_event_count = {currentEventCount.ValueToString()} " +
		$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 플레이어 골드 업데이트
	public static void UpdatePlayerGold(int gold) {
		if (gold <= 0) gold = 0;
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"gold = {gold} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		#region 골드 획득 업적
		AchievementEventManager.SetAchievementsProgress(gold, ENUM_ACHIEVEMENT.Show_Me_The_Money, ENUM_ACHIEVEMENT.Greed_Is_Good, ENUM_ACHIEVEMENT.Millionaires_Dream);
		#endregion
	}

	#endregion

	#region Maintenance Scene

	// 플레이어 카드 설정 (덱/가방)
	public static void SetPlayerCards(List<CardLiteDBData> deckList, List<CardLiteDBData> bagList) {
		PlayerCardListDao.RemoveAllCardsInDeck();

		PlayerCardListDao.AddNewCardsInDeck(deckList);
		PlayerCardListDao.AddNewCardsInBag(bagList);
	}

	// 해금된 스킬 리스트 업데이트 (액티브/패시브 통합)
	public static void UpdateUnlockedSkillList(string activeSkills, string passiveSkills, int skillPoint) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"skill_point = {skillPoint} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"unlocked_active_skill_list = {activeSkills}, " +
			$"unlocked_passive_skill_list = {passiveSkills} " +
			$"WHERE player_index = {playerInfoIndex} ";


		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}
	// 해금된 액티브 스킬 리스트 업데이트
	public static void UpdateUnlockedActiveSkillList(string activeSkillIndexList, int skillPoint) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"skill_point = {skillPoint} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"unlocked_active_skill_list = {activeSkillIndexList} " +
			$"WHERE player_index = {playerInfoIndex} ";


		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 해금된 패시브 스킬 리스트 업데이트
	public static void UpdateUnlockedPassiveSkillList(string passiveSkillIndexList, int skillPoint) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStatusTable} SET " +
			$"skill_point = {skillPoint} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"unlocked_passive_skill_list = {passiveSkillIndexList} " +
			$"WHERE player_index = {playerInfoIndex} ";

		it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 스킬 장착 정보 업데이트
	public static void UpdateSkillEquipInfo(int? skillIndex = null) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"skill_index = {skillIndex.ValueToString()} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 해금된 장비 리스트 업데이트
	public static void UpdateUnlockedEquipmentList(string equipmentIndexList) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"unlocked_equipment_list = {equipmentIndexList} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 장비 장착 정보 업데이트 (Dictionary)
	public static void UpdateEquipmentEquipInfo(Dictionary<ENUM_EQUIPMENT_PART, Equipment> currentEquipmentDict) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerEquipTable} SET " +
			$"current_equipment_head = {(currentEquipmentDict[ENUM_EQUIPMENT_PART.HEAD] == null ? nullString : currentEquipmentDict[ENUM_EQUIPMENT_PART.HEAD].Index.ToString())}, " +
			$"current_equipment_shirt = {(currentEquipmentDict[ENUM_EQUIPMENT_PART.SHIRT] == null ? nullString : currentEquipmentDict[ENUM_EQUIPMENT_PART.SHIRT].Index.ToString())}, " +
			$"current_equipment_pants = {(currentEquipmentDict[ENUM_EQUIPMENT_PART.PANTS] == null ? nullString : currentEquipmentDict[ENUM_EQUIPMENT_PART.PANTS].Index.ToString())}, " +
			$"current_equipment_weapon = {(currentEquipmentDict[ENUM_EQUIPMENT_PART.WEAPON] == null ? nullString : currentEquipmentDict[ENUM_EQUIPMENT_PART.WEAPON].Index.ToString())}, " +
			$"current_equipment_trinket = {(currentEquipmentDict[ENUM_EQUIPMENT_PART.TRINKET] == null ? nullString : currentEquipmentDict[ENUM_EQUIPMENT_PART.TRINKET].Index.ToString())}, " +
			$"current_equipment_etc = {(currentEquipmentDict[ENUM_EQUIPMENT_PART.ETC] == null ? nullString : currentEquipmentDict[ENUM_EQUIPMENT_PART.ETC].Index.ToString())} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 장비 장착 정보 업데이트 (Index Array)
	public static void UpdateEquipmentEquipInfo(int?[] currentEquipmentIndexList) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerEquipTable} SET " +
			$"current_equipment_head = {(currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.HEAD] == null ? nullString : currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.HEAD].Value.ToString())}, " +
			$"current_equipment_shirt = {(currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.SHIRT] == null ? nullString : currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.SHIRT].Value.ToString())}, " +
			$"current_equipment_pants = {(currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.PANTS] == null ? nullString : currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.PANTS].Value.ToString())}, " +
			$"current_equipment_weapon = {(currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.WEAPON] == null ? nullString : currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.WEAPON].Value.ToString())}, " +
			$"current_equipment_trinket = {(currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.TRINKET] == null ? nullString : currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.TRINKET].Value.ToString())}, " +
			$"current_equipment_etc = {(currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.ETC] == null ? nullString : currentEquipmentIndexList[(int)ENUM_EQUIPMENT_PART.ETC].Value.ToString())} " +
			$"WHERE player_index = {playerInfoIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	#endregion

	#region AreaSelection Scene

	// 현재 지역 인덱스 업데이트
	public static void UpdateCurrentAreaIndex(int? currentAreaIndex = null) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerStoryEventTable} SET " +
			$"current_area_index = {currentAreaIndex.ValueToString()} " +
			$"WHERE player_index = {playerInfoIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 클리어한 지역 리스트 업데이트
	public static void UpdateClearedAreaList(List<int> clearedAreaList) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerUnlockListTable} SET " +
			$"cleared_area_list = {(clearedAreaList == null ? nullString : clearedAreaList.ListToQueryString())} " +
			$"WHERE player_index = {playerInfoIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 현재 맵 인덱스 업데이트
	public static void UpdateCurrentMapIndex(int? mapIndex = null) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerInfoTable} SET " +
			$"current_map_index = {mapIndex.ValueToString()} " +
			$"WHERE player_index = {playerInfoIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	#endregion

	// 플레이어 통계 업데이트
	public static void UpdatePlayerStatistics(Dictionary<ENUM_PLAYER_STATISTICS_TYPE, string> statistics) {
		foreach (ENUM_PLAYER_STATISTICS_TYPE enumType in System.Enum.GetValues(typeof(ENUM_PLAYER_STATISTICS_TYPE))) {
			string query =
				$"UPDATE {DataBaseTableDefine.PlayerStatisticsTable} SET " +
				$"value = {statistics[enumType] ?? "NULL"} " +
				$"WHERE type = '\"{enumType}\"'";

			CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
		}
	}
}
