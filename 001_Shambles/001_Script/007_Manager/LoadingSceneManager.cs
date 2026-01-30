using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using TMPro;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine.Android;
using Unity.Services.Core;

// 버전 데이터 구조체
public struct VersionData {
	public string gameDataVersion;
	public string offlineDataVersion;
}

enum ENUM_LOADING_PROGRESS {
	Progress_Step_01,
	Progress_Step_02,
	Progress_Step_03,
	Progress_Step_04,
	Progress_Step_05,
	Progress_Step_06,
	Progress_Step_07,
	Progress_Step_08,
	Progress_Step_Complete
}

public class LoadingSceneManager : MonoBehaviour {
	[SerializeField] Button gameStartButton = null;
	[SerializeField] CanvasGroup titleCG = null;
	[SerializeField] MainMenuCheckPopup mainMenuCheckPopup = null;

	string targetFilePath;
	string noticeFilePath;
	string cacheFilePath;

	// 초기화 및 데이터 로드 시작
	void Start() {
		gameStartButton.interactable = false;
		titleCG.alpha = 0;
		StartCoroutine(LoadData());
	}

	// 파일 경로 초기화 및 필수 디렉토리 생성 (경로명 일반화)
	void ResetFilePath() {
		targetFilePath = Path.Combine(Application.persistentDataPath, "Example_Dir", "Example_Data.dat");
		noticeFilePath = Path.Combine(Application.persistentDataPath, "Example_Notice", "example_notice.json");
		cacheFilePath = Path.Combine(Application.persistentDataPath, "Example_Cache/");

		CreateDirectoryIfNeeded(targetFilePath);
		CreateDirectoryIfNeeded(noticeFilePath);
		CreateDirectoryIfNeeded(cacheFilePath);

		// 초기 파일이 없을 경우 기본 파일 복제
		if (SQLiteManager.IsDBExist(ENUM_DATABASE_PATH.USER_DATA) == false)
			SQLiteManager.CloneDataFile(ENUM_DATABASE_PATH.USER_DATA);
		if (SQLiteManager.IsDBExist(ENUM_DATABASE_PATH.GAME_DATA) == false)
			SQLiteManager.CloneDataFile(ENUM_DATABASE_PATH.GAME_DATA);
		if (SQLiteManager.IsDBExist(ENUM_DATABASE_PATH.PLAYER_DATA) == false)
			SQLiteManager.CloneDataFile(ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 디렉토리가 존재하지 않으면 생성
	void CreateDirectoryIfNeeded(string filePath) {
		string directory = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directory)) {
			Directory.CreateDirectory(directory);
		}
	}

	// 전체 데이터 로딩 및 초기화 프로세스 관리
	IEnumerator LoadData() {
		try {
			// 프레임 대기
			yield return null;
			
			// 리소스 다운로드 확인
#if PLATFORM_ANDROID && !UNITY_EDITOR
			// (안드로이드 에셋 팩 다운로드 로직 - 상세 생략)
			yield return new WaitUntil(() => AndroidAssetPacks.coreUnityAssetPacksDownloaded == true);
#endif
			
			// 분석 툴 초기화
			GameManager.GetInstance().InitalizeAnalytics();
			
			// 초기 데이터 설정 및 경로 확인
			ResetFilePath();
			mainMenuCheckPopup.StartAction(); // 팝업 액션 시작

			// 사용자 설정 데이터 로드
			yield return null;
			SettingManager.GetSettingData(); // 설정 불러오기
			SettingManager.RefreshPlatformData(); // 플랫폼별 설정 갱신

			// 소셜 플랫폼 로그인 시도
			yield return SocialLogin();

			// 서버 버전 확인 및 무결성 검사 이후 게임 데이터베이스(DB) 로드 및 검증
			yield return CheckVersion();

			// DLC혹은 인게임 상품 구매여부 검증 및 복구용 로더 호출
			ProductPurchasedStatusLoader productPurchasedStatusLoader = new ProductPurchasedStatusLoader();
			yield return productPurchasedStatusLoader.LoadProductData();

			// 공지사항 및 업데이트 패치 적용
			yield return DownloadNotice();
		}
		finally {
			// 모든 로딩 완료: UI 활성화 및 시작 대기 상태로 전환
			gameStartButton.interactable = true;
			DOTween.To(() => titleCG.alpha, x => titleCG.alpha = x, 1, 0.5f);
		}
	}

	// 서버 버전 정보 확인 및 업데이트 필요 여부 체크
	IEnumerator CheckVersion() {
		// 현재 버전 정보 로드
		VersionData currentVersion;
		if (!JsonDataManager.ReadData<VersionData>(ENUM_JSON_FILE.VersionData, out currentVersion)) {
			currentVersion = new VersionData();
			currentVersion.gameDataVersion = "0.0.0";
			currentVersion.offlineDataVersion = "0.0.0";
		}
		
		// 오프라인 버전 갱신 체크
		if (currentVersion.offlineDataVersion != Application.version) {
			currentVersion.offlineDataVersion = Application.version;
			currentVersion.gameDataVersion = "0.0.0";
			SQLiteManager.CloneDataFile(ENUM_DATABASE_PATH.GAME_DATA);
		}
		SQLiteManager.CloseDB();
		
		var _versionData = Version.Parse(Application.version);
		string resourceType = "Default";
#if PLATFORM_A
		resourceType = "Platform_A";
#elif PLATFORM_B
		resourceType = "Platform_B";
		// ...
#endif
		// 버전 확인 URL
		string versionUrl = $"https://example.com/api/{resourceType}/{_versionData.Major}.{_versionData.Minor}/version.json";
#if DEMO_VERSION
		versionUrl = $"https://example.com/api/Demo/version.json";
#elif DEBUG_VERSION
		versionUrl = "https://example.debug.com/api/version.json";
		// ...
#endif

		// 웹 요청을 통해 최신 버전 확인
		using (UnityWebRequest webRequest = UnityWebRequest.Get(versionUrl)) {
			yield return webRequest.SendWebRequest();
			if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
				Debug.LogError("Error: " + webRequest.error);
			}
			else {
				VersionData latestVersion = JsonUtility.FromJson<VersionData>(webRequest.downloadHandler.text);

				// 버전 불일치 시 업데이트 다운로드
				if (latestVersion.gameDataVersion != currentVersion.gameDataVersion) {
					yield return StartCoroutine(DownloadGameData(currentVersion, latestVersion));
				}
			}
		}
	}

	// 최신 게임 데이터 다운로드 및 적용
	IEnumerator DownloadGameData(VersionData currentVersion, VersionData latestVersion) {
		string resourceType = "Default";
#if PLATFORM_A
		resourceType = "Mobile";
#elif PLATFORM_B
		resourceType = "PC";
		// ...
#endif
		var _versionData = Version.Parse(Application.version);
		
		// 데이터 다운로드 URL
		string fileUrl = $"https://example.com/api/{resourceType}/{_versionData.Major}.{_versionData.Minor}/Example_Data.dat";
#if DEMO_VERSION
		fileUrl = $"https://example.com/api/Demo/Example_Data.dat";
#elif DEBUG_VERSION
		fileUrl = "https://example.debug.com/api/Example_Data.dat";
		// ...
#endif

		// 파일 다운로드 및 적용
		using (UnityWebRequest webRequest = UnityWebRequest.Get(fileUrl)) {
			webRequest.downloadHandler = new DownloadHandlerFile(targetFilePath);
			yield return webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
				Debug.LogError("Error downloading game data: " + webRequest.error);
			}
			else {
				// 버전 정보 갱신 및 UI 업데이트
				currentVersion.gameDataVersion = latestVersion.gameDataVersion;
				JsonDataManager.WriteData<VersionData>(ENUM_JSON_FILE.VersionData, in currentVersion);
				var versionText = GameObject.FindObjectOfType<VersionTracker>();
				if (versionText != null) {
					versionText.UpdateUI();
				}
			}
		}
	}

	// 공지사항 데이터 및 이미지 다운로드
	IEnumerator DownloadNotice() {
		// 공지사항 URL
		string fileUrl = $"https://example.com/api/Notice/notice.json";
		string textureBaseUrl = "";
		string noticeString = "";
		bool refreshCache = false;

#if NOTICE_TYPE_A
		fileUrl = $"https://example.com/api/Notice/example_ver_A/notice.json";
		textureBaseUrl = "https://example.com/api/Notice/example_ver_A/";
#elif NOTICE_TYPE_B
		fileUrl = $"https://example.com/api/Notice/example_ver_B/notice.json"; 
		textureBaseUrl = "https://example.com/api/Notice/example_ver_B/";
		// ...
#endif

		using (UnityWebRequest webRequest = UnityWebRequest.Get(fileUrl)) {
			yield return webRequest.SendWebRequest();
			if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
				Debug.LogWarning("Error: " + webRequest.error);
				yield break;
			}
			else {
				if (File.Exists(noticeFilePath)) { 
					if (File.ReadAllText(noticeFilePath) == webRequest.downloadHandler.text) { 
						noticeString = File.ReadAllText(noticeFilePath);
					}
					else {
						noticeString = webRequest.downloadHandler.text;
						File.WriteAllText(noticeFilePath, noticeString);
						refreshCache = true;
					}
				}
				else {
					noticeString = webRequest.downloadHandler.text;
					File.WriteAllText(noticeFilePath, noticeString);
				}
			}
		}

		if (File.Exists(noticeFilePath)) {
			List<NoticeRawData> rawData = JsonConvert.DeserializeObject<List<NoticeRawData>>(noticeString);
			List<NoticeData> noticeData = new List<NoticeData>();

			if (refreshCache) {
				// 캐시 초기화
				string[] existingCacheFiles = Directory.GetFiles(cacheFilePath, "*.png");
				foreach (string file in existingCacheFiles) File.Delete(file);
				existingCacheFiles = Directory.GetFiles(cacheFilePath, "*.jpg");
				foreach (string file in existingCacheFiles) File.Delete(file);
			}

			// 공지사항 리소스 로드
			foreach (var item in rawData) {
				if (item == null) continue;
				NoticeData newData = new NoticeData();

				foreach (ENUM_LANGUAGE lang in Enum.GetValues(typeof(ENUM_LANGUAGE))) {
					if (item.linkUrls != null && item.linkUrls.ContainsKey(lang.ToString())) {
						newData.redirectURL[lang.ToString()] = item.linkUrls[lang.ToString()];
					}
					else {
						newData.redirectURL[lang.ToString()] = "";
					}
				}

				if (item.dlcShopRedirect != "")
					newData.dlcIndex = int.Parse(item.dlcShopRedirect);

				// 이미지 다운로드 및 캐싱 로직
				string cachePathPng = Path.Combine(cacheFilePath, $"{item.imageKey}_{SettingManager.CurrentLanguageString}.png");
				string cachePathJpg = Path.Combine(cacheFilePath, $"{item.imageKey}_{SettingManager.CurrentLanguageString}.jpg");

				Texture2D texture = null;

				if (File.Exists(cachePathPng)) {
					byte[] fileData = File.ReadAllBytes(cachePathPng);
					texture = new Texture2D(2, 2);
					texture.LoadImage(fileData);
				}
				else if (File.Exists(cachePathJpg)) {
					byte[] fileData = File.ReadAllBytes(cachePathJpg);
					texture = new Texture2D(2, 2);
					texture.LoadImage(fileData);
				}
				else {
					// PNG 시도
					UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureBaseUrl + $"{item.imageKey}_{SettingManager.CurrentLanguageString}.png");
					yield return www.SendWebRequest();

					if (www.result == UnityWebRequest.Result.Success) {
						texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
						byte[] textureBytes = texture.EncodeToPNG();
						File.WriteAllBytes(cachePathPng, textureBytes);
					}
					else {
						// JPG 시도
						www = UnityWebRequestTexture.GetTexture(textureBaseUrl + $"{item.imageKey}_{SettingManager.CurrentLanguageString}.jpg");
						yield return www.SendWebRequest();

						if (www.result == UnityWebRequest.Result.Success) {
							texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
							byte[] textureBytes = texture.EncodeToJPG();
							File.WriteAllBytes(cachePathJpg, textureBytes);
						}
						else {
							continue;
						}
					}
				}

				if (texture != null) {
					newData.noticeSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
				}
				noticeData.Add(newData);
			}
			NoticeManager.GetInstance().SetNoticeData(noticeData);
		}
	}

	// 로컬 DB 및 테이블 구조 업데이트 체크
	IEnumerator UpdatePatchData() {
		// 로컬 데이터베이스의 테이블 구조를 검사하고
		// 버전 변경에 따른 스키마 업데이트 및 데이터 마이그레이션을 수행합니다.
		// (보안상 상세 구현부는 생략되었습니다.)
		yield return null;
	}

	// 소셜 플랫폼 로그인 처리
	IEnumerator SocialLogin() {
		yield return null;
#if APP_STORE
		bool authCompleted = false;
		Social.localUser.Authenticate(success => {
			authCompleted = true;
		});
		yield return new WaitUntil(() => authCompleted);
#endif
	}

}
