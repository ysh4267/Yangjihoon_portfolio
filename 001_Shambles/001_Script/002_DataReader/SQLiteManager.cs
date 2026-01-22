using Mono.Data.Sqlite;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SQLiteManager : IDisposable {
	private static List<CustomDataReader> readerList = new List<CustomDataReader>();
	private static SqliteConnection[] connection = new SqliteConnection[3];
	private static string[] dbPath = new string[3] {"DB/game_data.db",
													"DB/player_data.db",
													"DB/user_data.db" };


	static SQLiteManager() {
		SceneManager.sceneUnloaded += CloseDataReaders;

		if (!Directory.Exists(Application.persistentDataPath + "/DB")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/DB");
		}
	}

	#region Read
	// SQL 쿼리를 실행하여 결과 리더를 반환
	public static CustomDataReader SelectQuery(string query, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
		if (query == null) return null;

		try {
			OpenConnection(enumDataBasePath);
			using (SqliteCommand cmd = new SqliteCommand(query, connection[(int)enumDataBasePath])) {
				CustomDataReader customReader = new CustomDataReader(cmd.ExecuteReader());
				readerList.Add(customReader);
				return customReader;
			}
		}
		catch (Exception e) {
			Debug.LogError(query);
			Debug.LogError(e.Message);
			HandleError();
			return null;
		}

		void HandleError() {
			if (enumDataBasePath == ENUM_DATABASE_PATH.PLAYER_DATA) {
				Debug.LogError("Player DB 충돌 혹은 버전 업데이트 후 Player DB가 변경되었지만 로컬에는 적용되지 않음. (로컬 Player DB 제거 필요)");
				DeleteDB(ENUM_DATABASE_PATH.PLAYER_DATA);
			}
			else if (enumDataBasePath == ENUM_DATABASE_PATH.USER_DATA) {
				Debug.LogError("User DB 충돌 혹은 버전 업데이트 후 User DB가 변경되었지만 로컬에는 적용되지 않음. (로컬 User DB 제거 필요)");
				// 기존 User데이터를 복구하는 메소드 실행. 
			}

			var popup = Resources.Load<GameObject>("Prefabs/ErrorPopup");
			UnityEngine.MonoBehaviour.Instantiate(popup);
		}
	}

	// 커넥션을 시작하는 메소드. (파일이 없으면 생성/복사
	static void OpenConnection(ENUM_DATABASE_PATH _enumDataBasePath) {
		// db 파일이 존재하지 않을 경우, 플랫폼에 따라 다른 방식으로 db를 복사하여 생성한다.
		if (!IsDBExist(dbPath[(int)_enumDataBasePath])) {
			if (_enumDataBasePath == ENUM_DATABASE_PATH.GAME_DATA) {
				JsonDataManager.DeleteData(ENUM_JSON_FILE.VersionData);
			}
			CloneDataFile(_enumDataBasePath);
		}
		// DB 폴더와 db 파일이 모두 존재하는 경우, db 파일을 연다.
		if (connection[(int)_enumDataBasePath] == null) {
			string conn = "Data Source=" + Path.Combine(Application.persistentDataPath, dbPath[(int)_enumDataBasePath]);
			connection[(int)_enumDataBasePath] = new SqliteConnection(conn);
		}
		if (connection[(int)_enumDataBasePath].State == ConnectionState.Closed) {
			connection[(int)_enumDataBasePath].Open();
		}
	}
	#endregion

	#region Structure
	// 특정 테이블에 특정 컬럼이 존재하는지 확인
	public static bool IsColumnExist(string table, string column, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
		OpenConnection(enumDataBasePath);
		using (SqliteCommand cmd = new SqliteCommand(connection[(int)enumDataBasePath])) {
			cmd.CommandText = $"PRAGMA table_info({table})";
			using (SqliteDataReader reader = cmd.ExecuteReader()) {
				while (reader.Read())
					if (reader["name"].ToString() == column) return true;
			}
		}
		return false;
	}

	// 테이블에 컬럼이 존재하지 않으면 컬럼을 생성
	public static bool ExistOrMakeColumn(string table, string column, ENUM_DB_DATA_TYPE dataType, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
		if (!IsColumnExist(table, column, enumDataBasePath)) {
			string query =
				$"ALTER TABLE {table} ADD COLUMN {column} {dataType}";
			SelectQuery(query, enumDataBasePath);
			return false;
		}
		return true;
	}

	// 테이블이 존재하지 않으면 테이블을 생성
	public static bool ExistOrMakeTable(string tableName, Dictionary<string, (ENUM_DB_DATA_TYPE, List<ENUM_DB_COLUMN_CONSTRAINTS>)> tableStructure, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
		// 테이블 존재 여부 확인
		if (!IsTableExist(tableName, enumDataBasePath)) {
			// 테이블이 존재하지 않는 경우, 새로운 테이블 생성
			string createTableQuery = $"CREATE TABLE {tableName} ({GetTableStructureString(tableStructure)})";
			SelectQuery(createTableQuery, enumDataBasePath);
			return false; // 테이블 생성 필요
		}
		return true; // 테이블이 이미 존재

		string GetTableStructureString(Dictionary<string, (ENUM_DB_DATA_TYPE, List<ENUM_DB_COLUMN_CONSTRAINTS>)> structure) {
			var columnDefinitions = structure.Select(kvp =>
				$"{kvp.Key} {DataTypeToString(kvp.Value.Item1)}{ConstraintsToString(kvp.Value.Item2)}").ToArray();
			return string.Join(", ", columnDefinitions);
		}

		string ConstraintsToString(List<ENUM_DB_COLUMN_CONSTRAINTS> constraints) {
			if (constraints.HasValue()) {
				return " " + string.Join(" ", constraints.Select(ConstraintToString));
			}
			return string.Empty;
		}

		string DataTypeToString(ENUM_DB_DATA_TYPE dataType) {
			return dataType switch {
				ENUM_DB_DATA_TYPE.INTEGER => "INTEGER",
				ENUM_DB_DATA_TYPE.TEXT => "TEXT",
				ENUM_DB_DATA_TYPE.BLOB => "BLOB",
				ENUM_DB_DATA_TYPE.REAL => "REAL",
				ENUM_DB_DATA_TYPE.NUMERIC => "NUMERIC",
				_ => throw new ArgumentException("Invalid data type")
			};
		}

		string ConstraintToString(ENUM_DB_COLUMN_CONSTRAINTS constraint) {
			return constraint switch {
				ENUM_DB_COLUMN_CONSTRAINTS.PRIMARY_KEY => " PRIMARY KEY",
				ENUM_DB_COLUMN_CONSTRAINTS.NOT_NULL => " NOT NULL",
				ENUM_DB_COLUMN_CONSTRAINTS.UNIQUE => " UNIQUE",
				_ => throw new ArgumentException("Invalid constraint")
			};
		}
	}

	// 특정 테이블을 삭제
	public static void DeleteTable(string tableName, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
		// 테이블 존재 여부 확인
		if (IsTableExist(tableName, enumDataBasePath)) {
			// 테이블이 존재하는 경우 삭제
			CloseDB();
			string createTableQuery = $"DROP TABLE {tableName}";
			SelectQuery(createTableQuery, enumDataBasePath);
		}
	}

	// 테이블의 존재 여부를 확인
	public static bool IsTableExist(string tableName, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
		OpenConnection(enumDataBasePath);
		using (SqliteCommand cmd = new SqliteCommand(connection[(int)enumDataBasePath])) {
			cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
			using (SqliteDataReader reader = cmd.ExecuteReader()) {
				return reader.Read(); // 테이블이 존재하면 true, 아니면 false 반환
			}
		}
	}
	#endregion

	#region File Management
	// 특정 경로에 DB 파일이 존재하는지 확인
	public static bool IsDBExist(string _dbPath) {
		// DB 폴더가 존재하지 않을 경우, DB 폴더를 만든다.
		if (!Directory.Exists(Application.persistentDataPath + "/DB")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/DB");
			return false;
		}

		return File.Exists(Path.Combine(Application.persistentDataPath, _dbPath));
	}

	// 열거형 경로에 해당하는 DB 파일이 존재하는지 확인
	public static bool IsDBExist(ENUM_DATABASE_PATH _databaseEnum) {
		return IsDBExist(dbPath[(int)_databaseEnum]);
	}

	// 로컬 DB와 스트리밍 에셋 DB가 동일한지(용량 비교) 확인
	public static bool IsSameDB(string _dbPath) {
		//db 파일의 용량이 같은지를 확인
		var _cFile = new FileInfo(Path.Combine(Application.persistentDataPath, _dbPath));
		var _oFile = new FileInfo(Path.Combine(Application.streamingAssetsPath, _dbPath));
		return _cFile.Length == _oFile.Length;
	}

	// streamingAssetsPath에서 persistentDataPath경로로 DB 파일을 복사
	static public void CloneDataFile(ENUM_DATABASE_PATH _enumDataBasePath) {
		if (Application.platform == RuntimePlatform.Android) {
			var webRequest = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, dbPath[(int)_enumDataBasePath]));
			webRequest.SendWebRequest();
			while (!webRequest.isDone) {
				if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
					webRequest.result == UnityWebRequest.Result.ProtocolError)
					break;
			}
			File.WriteAllBytes(Path.Combine(Application.persistentDataPath, dbPath[(int)_enumDataBasePath]), webRequest.downloadHandler.data);
		}
		else {
			File.Copy(Path.Combine(Application.streamingAssetsPath, dbPath[(int)_enumDataBasePath]), Path.Combine(Application.persistentDataPath, dbPath[(int)_enumDataBasePath]), true);
		}
	}

	// 특정 DB 파일을 삭제
	public static void DeleteDB(ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.PLAYER_DATA) {
		try {
			string path = Path.Combine(Application.persistentDataPath, dbPath[(int)enumDataBasePath]);
			if (IsDBExist(dbPath[(int)enumDataBasePath])) {
				CloseDataReaders();
				CloseConnection((int)enumDataBasePath); // 모든 DB의 command 및 datareader가 해제된 후 connection 해제
				File.Delete(path); // 모든 db가 해제된 후, 즉 IOException을 일으키지 않고 db 삭제
			}
			else
				Debug.LogWarning($"Delete {dbPath[(int)enumDataBasePath]} Failed: DB file does not exist in persistent data path");
		}
		catch (Exception ex) {
			if (ex.GetType() == typeof(System.IO.IOException)) {
				Debug.LogError(ex.Message);
				var popup = Resources.Load<GameObject>("Prefabs/ErrorPopup");
				UnityEngine.MonoBehaviour.Instantiate(popup);
			}
			else {
				Debug.LogError("DB 오류가 발생했습니다: " + ex.Message);
			}
		}
	}
	#endregion

	#region Close
	// 씬이 언로드될 때 호출
	private static void CloseDataReaders(Scene scene) {
		CloseDataReaders();
	}

	// 활성화된 모든 데이터 리더를 종료
	private static void CloseDataReaders() {
		foreach (var reader in readerList) {
			reader.Close();
		}
		readerList.Clear();
	}

	// 특정 DB 연결을 종료
	private static void CloseConnection(int connIndex) { // DB 닫는 과정에서 순서가 중요하므로 절대 외부에서 참조 금지
		if (connection[connIndex] != null && connection[connIndex].State == ConnectionState.Open) {
			connection[connIndex].Close();
		}
	}

	// 모든 DB 연결과 리더를 종료
	public static void CloseDB() {
		for (int i = 0; i < 3; i++) {
			if (IsDBExist(dbPath[i])) {
				CloseDataReaders();
				CloseConnection(i); // 모든 DB의 command 및 datareader가 해제된 후 connection 해제
			}
			else
				Debug.LogWarning($"Close {dbPath[i]} Failed: DB file does not exist in persistent data path");
		}

	}

	// 객체가 소멸될 때 리소스를 해제
	public void Dispose() {
		if (connection != null) {
			for (int i = 0; i < connection.Length; i++) {
				if (connection[i] != null) {
					connection[i].Close();
					connection[i] = null;
				}
			}
			connection = null;
		}
	}
	#endregion
}
