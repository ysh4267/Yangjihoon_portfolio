using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// 도감 테이블에 포함되는 타입의 항목들을 관리하기 위한 부모 클래스
public class CollectionDao {

	private static Dictionary<Type, KeyValuePair<string, string>> tableAndIndices = new Dictionary<Type, KeyValuePair<string, string>>() {
		[typeof(Card)] = new KeyValuePair<string, string>(DataBaseTableDefine.UnlockedCardTable, "card_index"),
		[typeof(Character)] = new KeyValuePair<string, string>(DataBaseTableDefine.UnlockedCharacterTable, "character_index"),
		[typeof(Ending)] = new KeyValuePair<string, string>(DataBaseTableDefine.UnlockedEndingTable, "ending_index"),
		[typeof(Equipment)] = new KeyValuePair<string, string>(DataBaseTableDefine.UnlockedEquipmentTable, "equipment_index"),
		[typeof(Journal)] = new KeyValuePair<string, string>(DataBaseTableDefine.UnlockedJournalTable, "journal_index"),
		[typeof(Achievement)] = new KeyValuePair<string, string>(DataBaseTableDefine.UnlockedAchievementTable, "achievement_primarykey"), // 업적 테이블의 기본키는 achievement_primarykey
		[typeof(ExplorerInfo)] = new KeyValuePair<string, string>(DataBaseTableDefine.ExplorerInfoTable, "explorer_index"),
	};

	// 타입에 해당하는 테이블 이름과 인덱스 컬럼명 페어 반환
	private static KeyValuePair<string, string> GetTableIndexPair(Type type) {
		if (tableAndIndices.TryGetValue(type, out var pair)) return pair;
		else {
			Debug.LogError("Type doesn't exist in dictionary");
			return new KeyValuePair<string, string>(null, null); // default
		}
	}

	// 미확인(NEW) 상태인 항목들의 인덱스 리스트 반환
	private static List<int> GetUncheckedIndexList(KeyValuePair<string, string> pair) {
		if (pair.Key == null || pair.Value == null) return null;
		var tableName = pair.Key;
		var indexName = pair.Value;

		SQLiteManager.ExistOrMakeColumn(tableName, "is_checked", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);
		string query =
			$"SELECT " +
			$"{indexName} AS '{indexName}' " +
			$"FROM {tableName} " +
			$"WHERE is_checked = 'false' " +
			$"OR is_checked IS NULL";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		List<int> list = new List<int>();
		if (false == it.Read()) {
			return list;
		}

		do {
			var index = it.GetSafeValue<int>(0);
			list.Add(index);
		} while (true == it.Read());

		return list;
	}

	// 제네릭 타입을 받아 미확인 항목 인덱스 리스트 반환
	public static List<int> GetUncheckedIndexList<T>() where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T)); // 컴파일 타임에 객체 타입 확인
		return GetUncheckedIndexList(pair);
	}

	// ICollectionDTO 구현 객체를 받아 미확인 항목 인덱스 리스트 반환
	public static List<int> GetUncheckedIndexList(ICollectionDTO obj) {
		var pair = GetTableIndexPair(obj.GetType()); // 런타임에 인스턴스 타입 확인
		return GetUncheckedIndexList(pair);
	}

	// ICollectionDTO 구현 객체를 받아 해금된 항목 인덱스 리스트 반환
	public static List<int> GetUnlockedIndexList(ICollectionDTO obj) {
		var pair = GetTableIndexPair(obj.GetType()); // 런타임에 인스턴스 타입 확인
		return GetUnlockedIndexList(pair);
	}

	// 해금된 항목들의 인덱스 리스트 반환 (내부 로직)
	protected static List<int> GetUnlockedIndexList(KeyValuePair<string, string> pair) {
		if (pair.Key == null || pair.Value == null) return null;
		var tableName = pair.Key;
		var indexName = pair.Value;
		string query =
			$"SELECT " +
			$"{indexName} AS '{indexName}' " +
			$"FROM {tableName} " +
			$"WHERE is_unlocked = 'true'";
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		List<int> list = new List<int>();
		if (false == it.Read()) {
			return list;
		}
		do {
			var index = it.GetSafeValue<int>(0);
			list.Add(index);
		} while (true == it.Read());
		return list;
	}

	// 제네릭 타입을 받아 해금된 항목 인덱스 리스트 반환
	public static List<int> GetUnlockedIndexList<T>() where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return null;
		var tableName = pair.Key;
		var indexName = pair.Value;
		string query =
			$"SELECT " +
			$"{indexName} AS '{indexName}' " +
			$"FROM {tableName} " +
			$"WHERE is_unlocked = 'true'";
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		List<int> list = new List<int>();
		if (false == it.Read()) {
			return list;
		}
		do {
			var index = it.GetSafeValue<int>(0);
			list.Add(index);
		} while (true == it.Read());
		return list;
	}

	// 테이블의 전체 항목 개수 반환
	protected static int GetTotalCount(string tableName) {
		string query =
			$"SELECT COUNT(*) " +
			$"FROM {tableName}";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		if (false == it.Read()) return 0;
		return it.GetSafeValue<int>(0);
	}

	// 캐릭터 전체 항목 개수 반환 (부모 캐릭터 기준)
	protected static int GetCharacterTotalCount(string tableName) {
		string query =
		   $"SELECT COUNT(*) " +
		   $"FROM {tableName} " +
		   $"WHERE parent_index IS NULL OR parent_index = character_index";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		if (false == it.Read()) return 0;
		return it.GetSafeValue<int>(0);
	}

	// 유저 데이터 테이블의 전체 항목 개수 반환
	protected static int GetUserTotalCount(string tableName) {
		string query =
			$"SELECT COUNT(*) " +
			$"FROM {tableName}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) return 0;
		return it.GetSafeValue<int>(0);
	}

	// 해금된 항목 개수 조회
	protected static int GetUnlockedCount<T>() where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return 0;
		var tableName = pair.Key;

		string query =
			$"SELECT COUNT(*) " +
			$"FROM {tableName} " +
			$"WHERE is_unlocked = 'true'";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) return 0;
		return it.GetSafeValue<int>(0);
	}

	// 미확인(NEW) 항목 개수 조회
	protected static int GetUncheckedCount<T>() where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return 0;
		var tableName = pair.Key;

		SQLiteManager.ExistOrMakeColumn(tableName, "is_checked", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);
		string query =
			$"SELECT COUNT(*) " +
			$"FROM {tableName} " +
			$"WHERE is_checked = 'false' " +
			$"OR is_checked IS NULL";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) return 0;
		return it.GetSafeValue<int>(0);
	}

	// 특정 항목 해금 처리 (반환값: 신규 해금 여부)
	protected static bool Unlock<T>(int _index, bool isUnlock = true) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return false;
		var tableName = pair.Key;
		var indexName = pair.Value;

		bool isNew = false;

		// 해당 인덱스가 잠금 해제 상태에서 잠금 해제 작업을 수행하는 경우 isNew 값을 true로 설정
		if (isUnlock && !IsUnlocked<T>(_index)) {
			isNew = true;
		}

		// 만약 해당 인덱스가 테이블에 존재하지 않는다면
		if (!IsExist<T>(_index)) {
			// 테이블에 "is_checked" 컬럼이 존재하지 않으면 추가
			SQLiteManager.ExistOrMakeColumn(tableName, "is_checked", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);
			// 해당 인덱스를 새로운 레코드로 추가
			string query =
				$"INSERT " +
				$"INTO {tableName} ({indexName}, is_unlocked, is_checked) " +
				$"VALUES ({_index}, '{isUnlock.BooleanToString()}', 'false')";
			SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
		}
		else {
			// 해당 인덱스가 이미 존재하는 경우, 잠금 상태를 업데이트
			string query =
				$"UPDATE " +
				$"{tableName} " +
				$"SET is_unlocked = '{isUnlock.BooleanToString()}' " +
				$"WHERE {indexName} = {_index}";
			SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
		}

		return isNew;  // isNew 값을 반환
	}

	// 도감 항목 확인(NEW 표시 제거) 처리
	protected static void Check<T>(int _index, bool isChecked = false) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return;
		var tableName = pair.Key;
		var indexName = pair.Value;

		// 해당 인덱스가 테이블에 존재하는지 확인하고, 존재하지 않으면 메서드를 종료합니다.
		if (!IsExist<T>(_index)) return;

		// "is_checked" 컬럼이 테이블에 존재하는지 확인하고, 없으면 추가합니다.
		SQLiteManager.ExistOrMakeColumn(tableName, "is_checked", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);

		// 해당 인덱스의 레코드를 업데이트하여 "is_checked" 컬럼의 값을 isChecked로 설정합니다.
		string query =
			$"UPDATE " + // UPDATE 문은 레코드의 특정 컬럼을 수정하므로 레코드의 순서가 바뀌지 않음
			$"{tableName} " +
			$"SET is_checked = '{isChecked.BooleanToString()}' " +
			$"WHERE {indexName} = {_index}";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
	}

	// 스포일러 상태 업데이트
	protected static void UpdateSpoiler<T>(int _index, bool isSpoiler = false) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return;
		var tableName = pair.Key;
		var indexName = pair.Value;

		// 해당 인덱스의 레코드를 업데이트하여 "is_spoiler" 컬럼의 값을 isSpoiler로 설정합니다.
		string query =
			$"UPDATE " + // UPDATE 문은 레코드의 특정 컬럼을 수정하므로 레코드의 순서가 바뀌지 않음
			$"{tableName} " +
			$"SET is_spoiler = '{isSpoiler.BooleanToString()}' " +
			$"WHERE {indexName} = {_index}";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
	}
	// 업적 진행도 1 증가
	protected static void IncrementAchievementProgress<T>(int _index) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return;
		var tableName = pair.Key;
		var indexName = pair.Value;

		// 해당 인덱스의 레코드를 업데이트하여 "progress_value" 컬럼의 값을 progress 설정합니다.
		string query =
			$"UPDATE " + // UPDATE 문은 레코드의 특정 컬럼을 수정하므로 레코드의 순서가 바뀌지 않음
			$"{tableName} " +
			$"SET progress_value = '{GetAchievementProgress<T>(_index) + 1}' " +
			$"WHERE {indexName} = {_index}";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
	}
	// 업적 진행도 설정
	protected static void SetAchievementProgress<T>(int _index, int progress) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return;
		var tableName = pair.Key;
		var indexName = pair.Value;

		// 해당 인덱스의 레코드를 업데이트하여 "progress_value" 컬럼의 값을 progress 설정합니다.
		string query =
			$"UPDATE " + // UPDATE 문은 레코드의 특정 컬럼을 수정하므로 레코드의 순서가 바뀌지 않음
			$"{tableName} " +
			$"SET progress_value = '{progress}' " +
			$"WHERE {indexName} = {_index}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
		if (false == it.Read()) {
			var achievement = AchievementDao.GetAchievement(_index, true);
			string spoilerJson = achievement.spoiler.ToString().ToLower(); // bool 값을 소문자로 변환하여 문자열로 저장
			query = $"INSERT OR IGNORE INTO {DataBaseTableDefine.UnlockedAchievementTable} " +
				$"(achievement_primarykey, achievement_index, progress_value, is_unlocked, is_checked, is_spoiler) " +
				$"VALUES ({achievement.achievementPrimarykey}, {achievement.Index}, {((achievement.maxProgressValue != null && achievement.maxProgressValue >= 0) ? 0 : "NULL")}, 'false', 'false', '{spoilerJson}');";
			SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
		}
	}

	// 업적 진행도 조회
	protected static int? GetAchievementProgress<T>(int _index) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return null;
		var tableName = pair.Key;
		var indexName = pair.Value;

		string query =
			$"SELECT progress_value " +
			$"FROM {tableName} " +
			$"WHERE {indexName} = {_index} ";
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) {
			return null;
		}

		return it.GetSafeValue<int>(0);
	}

	// 항목의 해금 여부 확인
	protected static bool IsUnlocked<T>(int _index) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return false;
		var tableName = pair.Key;
		var indexName = pair.Value;

		string query =
			$"SELECT is_unlocked " +
			$"FROM {tableName} " +
			$"WHERE {indexName} = {_index} " +
			$"AND is_unlocked = 'true'";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) {
			return false;
		}

		return true;
	}

	// 항목의 확인 여부(NEW 아님) 확인
	protected static bool IsChecked<T>(int _index) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return false;
		var tableName = pair.Key;
		var indexName = pair.Value;

		if (IsExist<T>(_index)) {
			SQLiteManager.ExistOrMakeColumn(tableName, "is_checked", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);
			string query =
				$"SELECT is_checked " +
				$"FROM {tableName} " +
				$"WHERE {indexName} = {_index} " +
				$"AND is_checked = 'true'";

			CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

			if (false == it.Read()) {
				return false;
			}

			return true;
		}

		return true;
	}

	// 항목의 스포일러 여부 확인
	protected static bool IsSpoiler<T>(int _index) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return false;
		var tableName = pair.Key;
		var indexName = pair.Value;

		if (IsExist<T>(_index)) {
			SQLiteManager.ExistOrMakeColumn(tableName, "is_spoiler", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);
			string query =
				$"SELECT is_spoiler " +
				$"FROM {tableName} " +
				$"WHERE {indexName} = {_index} " +
				$"AND is_spoiler = 'true'";

			CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

			if (false == it.Read()) {
				return false;
			}

			return true;
		}

		return true;
	}

	// 항목이 테이블에 존재하는지 확인
	protected static bool IsExist<T>(int _index) where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return false;
		var tableName = pair.Key;
		var indexName = pair.Value;
		string query =
			$"SELECT {indexName} " +
			$"FROM {tableName} " +
			$"WHERE {indexName} = {_index}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) {
			return false;
		}

		return true;
	}

	// 모든 항목을 확인 상태로 변경
	protected static void ResetAllToChecked<T>() where T : ICollectionDTO {
		var pair = GetTableIndexPair(typeof(T));
		if (pair.Key == null || pair.Value == null) return;
		var tableName = pair.Key;

		SQLiteManager.ExistOrMakeColumn(tableName, "is_checked", ENUM_DB_DATA_TYPE.TEXT, ENUM_DATABASE_PATH.USER_DATA);
		string query =
			$"UPDATE " + // UPDATE 문은 레코드의 특정 컬럼을 수정하므로 레코드의 순서가 바뀌지 않음
			$"{tableName} " +
			$"SET is_checked = 'true'";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
	}
}