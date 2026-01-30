using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Networking;

public static class JsonDataManager {
	// 실제 파일 명
	private static readonly Dictionary<ENUM_JSON_FILE, string> fileNameList = new Dictionary<ENUM_JSON_FILE, string> {
		{ ENUM_JSON_FILE.Example_Type_01, "Example_Data_01" },
		{ ENUM_JSON_FILE.Example_Type_02, "Example_Data_02" },
		{ ENUM_JSON_FILE.Example_Type_03, "Example_Data_03" },
		// ...
	};

	// 파일 암호화 여부 (디버깅 시에는 모든 암호화를 해제함.)
	private static readonly Dictionary<ENUM_JSON_FILE, bool> fileEncryptionStatus = new Dictionary<ENUM_JSON_FILE, bool> {
#if DEBUG
		{ ENUM_JSON_FILE.Example_Type_01, false },
		{ ENUM_JSON_FILE.Example_Type_02, false },
		{ ENUM_JSON_FILE.Example_Type_03, false },
		// ...
#else
		{ ENUM_JSON_FILE.Example_Type_01, false },
		{ ENUM_JSON_FILE.Example_Type_02, true }, // 암호화 필요
		{ ENUM_JSON_FILE.Example_Type_03, true }, // 암호화 필요
		// ...
#endif
	};

	// 윈도우/모바일 등 플랫폼별 영구 데이터 경로를 반환
	private static string GetFilePath(ENUM_JSON_FILE fileType) {
		string fileName = fileNameList[fileType] + ".json";
		return Path.Combine(Application.persistentDataPath, fileName);
	}

	// 언어 파일 경로 반환 (StreamingAssets)
	private static string GetFilePath(ENUM_LANGUAGE fileType) {
		string relativePath = Path.Combine("Language", fileType.ToString() + ".json");
		return Path.Combine(Application.streamingAssetsPath, relativePath);
	}

	// JSON 데이터를 읽어와 객체로 역직렬화 및 무결성 검사 수행
	public static bool ReadData<T>(ENUM_JSON_FILE fileType, out T data, bool isIntact = false) where T : new() {
		string filePath = GetFilePath(fileType);
		if (!File.Exists(filePath)) {
			data = new T();
			return false; //파일 없음
		}

		try {
			string jsonData = File.ReadAllText(filePath);
			if (fileEncryptionStatus[fileType] == true) {
				string password = fileNameList[fileType];
				jsonData = Decrypt(jsonData, password);
			}

			var settings = new JsonSerializerSettings {
				DefaultValueHandling = DefaultValueHandling.Populate
			};

			data = JsonConvert.DeserializeObject<T>(jsonData, settings);
			if (!isIntact) {
				return true; // isIntact가 false면 바로 true 반환
			}

			// 모든 필드 및 프로퍼티 존재 여부 확인
			if (!CheckIntagrity<T>(jsonData)) {
				data = new T();
				return false;
			}

			return true; // 모든 필드 및 프로퍼티가 존재하는 경우
		}
		catch (Exception ex) {
			Debug.LogError($"Error occurred on reading JSON file: {ex.Message}");
		}

		data = new T();
		return false;
	}

	// JSON 데이터의 무결성 검증 (필드 및 프로퍼티 누락 확인)
	private static bool CheckIntagrity<T>(string jsonData) where T : new() {
		JObject jsonObj = JObject.Parse(jsonData);
		Type type = typeof(T);

		foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
			if (!jsonObj.TryGetValue(field.Name, StringComparison.OrdinalIgnoreCase, out JToken _)) {
				return false; // 필드가 존재하지 않음
			}
		}
		foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
			if (prop.CanRead && !jsonObj.TryGetValue(prop.Name, StringComparison.OrdinalIgnoreCase, out JToken _)) {
				return false; // 프로퍼티가 존재하지 않음
			}
		}

		return true; // 모든 필드 및 프로퍼티가 존재함
	}

	// 언어 데이터 읽기 (Android의 경우 UnityWebRequest 사용)
	public static bool ReadLanguageData<T>(ENUM_LANGUAGE fileType, out T data) where T : new() {
		string filePath = GetFilePath(fileType);
		try {
			string jsonData;
#if UNITY_ANDROID && !UNITY_EDITOR
			var webRequest = UnityWebRequest.Get(filePath);
			webRequest.SendWebRequest();
			while (!webRequest.isDone) {
				if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
					webRequest.result == UnityWebRequest.Result.ProtocolError) {
					string _jsonData = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileType.ToString() + ".json"));
					var _settings = new JsonSerializerSettings {
						DefaultValueHandling = DefaultValueHandling.Populate,
						Converters = new List<JsonConverter> { new StringEnumConverter() }
					};

					data = JsonConvert.DeserializeObject<T>(_jsonData, _settings);
					return false; //파일 없음
				}
			}
			jsonData = webRequest.downloadHandler.text;
			if (jsonData != null && jsonData != String.Empty) {
				File.WriteAllText(Path.Combine(Application.persistentDataPath, fileType.ToString() + ".json"), jsonData);
			}
#else
			if (!File.Exists(filePath)) {
				data = default(T);
				return false; //파일 없음
			}
			jsonData = File.ReadAllText(filePath);
#endif
			var settings = new JsonSerializerSettings {
				DefaultValueHandling = DefaultValueHandling.Populate,
				Converters = new List<JsonConverter> { new StringEnumConverter() }
			};

			data = JsonConvert.DeserializeObject<T>(jsonData, settings);
			return true;
		}
		catch (Exception ex) {
			Debug.LogError($"Error occurred on reading JSON file: {ex.Message}");
		}

		data = default(T);
		return false;
	}

	// 데이터를 JSON 파일로 저장 (암호화 옵션 적용)
	public static void WriteData<T>(ENUM_JSON_FILE fileType, in T data) {
		string filePath = GetFilePath(fileType);
		string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
		try {
			if (fileEncryptionStatus[fileType] == true) {
				string passward = fileNameList[fileType];
				jsonData = Encrypt(jsonData, passward);
			}
		}
		catch (Exception ex) {
			Debug.LogError($"Error occurred on writing JSON file: {ex.Message}");
		}
		File.WriteAllText(filePath, jsonData);
	}

	// 특정 데이터 파일 삭제
	public static void DeleteData(ENUM_JSON_FILE fileType) {
		string filePath = GetFilePath(fileType);
		if (File.Exists(filePath)) {
			File.Delete(filePath);
		}
	}

	// 모든 데이터 파일 삭제
	public static void DeleteAllData() {
		foreach (ENUM_JSON_FILE fileEnum in System.Enum.GetValues(typeof(ENUM_JSON_FILE))) {
			DeleteData(fileEnum);
		}
	}

	// 특정 데이터 파일 존재 여부 확인
	public static bool IsDataExist(ENUM_JSON_FILE fileType) {
		string filePath = GetFilePath(fileType);
		return File.Exists(filePath);
	}

	// AES 암호화 로직 (보안상 상세 구현 생략)
	static string Encrypt(string plainText, string passwardString) {
		// AES 알고리즘을 사용하여 입력된 평문을 암호화합니다.
		// 키 생성 및 IV 처리 후, 암호화된 바이트 배열을 Base64 문자열로 반환합니다.
		return "Encrypted_String_Example";
	}

	// AES 복호화 로직 (보안상 상세 구현 생략)
	static string Decrypt(string encryptedText, string passwardString) {
		// Base64 문자열을 바이트 배열로 변환합니다.
		// AES 알고리즘과 키/IV를 사용하여 원본 평문으로 복호화합니다.
		return "Decrypted_String_Example";
	}

	// 암호화 키 바이트 생성 (보안상 상세 구현 생략)
	static byte[] GetKeyBytes(string passwardString) {
		// 입력된 패스워드 문자열을 기반으로 고정 길이의 암호화 키(Byte Array)를 생성합니다.
		return new byte[32];
	}
}
