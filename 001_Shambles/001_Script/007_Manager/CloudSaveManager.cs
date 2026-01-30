using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Collections;
using Newtonsoft.Json;
using System;

#if UNITY_ANDROID && PLAY_STORE
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#elif UNITY_IOS && APP_STORE
using Apple.GameKit;
#endif

public enum CLOUD_SAVE_RESULT {
	SUCCESS = 0,
	ERROR_BAD_CONNECTION = 1,
	ERROR_BAD_DATA_TYPE = 2,
	ERROR_NO_DATA = 3,
	ERROR_LOGIN_FAIL = 4,
	ERROR_UNITY_SERVIECE_INITIALIZING_FAIL = 5
}

/// <summary>
/// Unity Cloud Save 서비스를 사용하여 클라우드 저장 및 불러오기 기능을 관리하는 클래스입니다.
/// Google Play Games, Apple Game Center 및 Unity Authentication 로직을 포함합니다.
/// </summary>
public class CloudSaveManager : MonoBehaviour {
	const string PlayerDataKey = "Example_Player_Data_Key";

	private static CloudSaveManager _instance;
	public static CloudSaveManager Instance {
		get {
			if (_instance == null) {
				var existing = FindObjectOfType<CloudSaveManager>();
				if (existing != null) {
					_instance = existing;
				}
				else {
					GameObject cloudsaveManager = new GameObject("CloudSaveManager");
					_instance = cloudsaveManager.AddComponent<CloudSaveManager>();
				}
			}
			return _instance;
		}
		private set { _instance = value; }
	}

	void Awake() {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (_instance != this) {
			Destroy(gameObject);
		}
	}

	// Google Play Games 로그인 및 Unity Authentication 연동을 비동기로 수행
	public async Task GooglePlayGamesInitializeAndLoginAsync() {
#if PLAY_STORE
		PlayGamesPlatform.Activate();

		var taskEndCall = new TaskCompletionSource<bool>();

		PlayGamesPlatform.Instance.Authenticate(status => {
			if (status == SignInStatus.Success) {
				PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code => {
					try {
						AuthenticationService.Instance.SignOut();
						await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
						taskEndCall.TrySetResult(true);
					}
					catch (System.Exception e) {
						Debug.LogError($"Auth Error: {e.Message}");
						taskEndCall.TrySetResult(false);
					}
				});
			}
			else {
				Debug.Log("Login Unsuccessful");
				taskEndCall.TrySetResult(false);
			}
		});

		await taskEndCall.Task; // 로그인 완료까지 대기
		Debug.Log("로그인 프로세스 완료");
#endif
	}

	// Apple Game Center 로그인 및 Unity Authentication 연동을 비동기로 수행
	private async Task GameCenterInitializeAndLoginAsync()
	{
#if APP_STORE
		try {
			if (Social.localUser.authenticated == false) {
				var socialLoginEndCall = new TaskCompletionSource<bool>();
				Social.localUser.Authenticate(success => {
					socialLoginEndCall.SetResult(true);
				});
				await socialLoginEndCall.Task;
			}

			if (!GKLocalPlayer.Local.IsAuthenticated) {
				var player = await GKLocalPlayer.Authenticate();
			}

			var localPlayer = GKLocalPlayer.Local;
			var fetchItemsResponse = await GKLocalPlayer.Local.FetchItems();

			string signature;
			string teamPlayerID;
			string salt;
			string publicKeyUrl;
			ulong timestamp;

			signature = Convert.ToBase64String(fetchItemsResponse.GetSignature());
			teamPlayerID = localPlayer.TeamPlayerId;
			salt = Convert.ToBase64String(fetchItemsResponse.GetSalt());
			publicKeyUrl = fetchItemsResponse.PublicKeyUrl;
			timestamp = fetchItemsResponse.Timestamp;

			await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(signature, teamPlayerID, publicKeyUrl, salt, timestamp);

		}
		catch (System.Exception e) {
			Debug.LogError($"Auth Error: {e.Message}");
		}
#endif
	}

	// 플랫폼별 초기화 및 로그인 수행
	public async Task InitializeAndLoginAsync() {
		await UnityServices.InitializeAsync();
#if UNITY_ANDROID && !UNITY_EDITOR
		await GooglePlayGamesInitializeAndLoginAsync();
#elif UNITY_IOS && !UNITY_EDITOR
		await GameCenterInitializeAndLoginAsync();
#else
		await AuthenticationService.Instance.SignInAnonymouslyAsync();
#endif
	}

	// 플레이어 데이터를 클라우드에 저장
	public void SaveData(Action<CLOUD_SAVE_RESULT> onErrorCallback) {
		PlayerCloudSaveData saveData = new PlayerCloudSaveData();
		StartCoroutine(SaveDataToCloud(saveData.GetPlayerData().GetPlayerDataJson(), onErrorCallback));
	}

	// 테스트용 데이터 로드
	public void TestLoad() {
		PlayerCloudSaveData saveData = new PlayerCloudSaveData();
		StartCoroutine(LoadDataFromCloud((json) => {
			if (!string.IsNullOrEmpty(json)) {
				var data = JsonConvert.DeserializeObject<PlayerCloudSaveData>(json);
				UserInfoDao.LoadCloudSaveData(saveData);
			}
		}));
	}

	// 클라우드에서 데이터를 로드하고 콜백 실행
	public void LoadData(System.Action<PlayerCloudSaveData> action) {
		PlayerCloudSaveData saveData = null;
		StartCoroutine(LoadDataFromCloud((json) => {
			if (!string.IsNullOrEmpty(json)) {
				try {
					saveData = JsonConvert.DeserializeObject<PlayerCloudSaveData>(json);
				}
				catch (System.Exception ex) {
					saveData = null;
					Debug.LogWarning(ex);
				}
			}
			action.Invoke(saveData);
		}));
	}

	// 클라우드 저장 실패 시 에러 팝업 출력
	public void ErrorPause(CLOUD_SAVE_RESULT result) {
		//팝업 프리펩 생성후 에러에 따른 메세지 출력
	}

	// 데이터를 클라우드에 저장하는 코루틴 래퍼
	public IEnumerator SaveDataToCloud(string json, Action<CLOUD_SAVE_RESULT> onErrorCallback) {
		yield return SaveInternal(json, onErrorCallback);
	}

	// 클라우드 데이터를 로드하는 코루틴 래퍼
	public IEnumerator LoadDataFromCloud(System.Action<string> onLoaded) {
		var task = LoadInternal();
		yield return new WaitUntil(() => task.IsCompleted);

		if (task.Exception != null) {
			Debug.LogWarning("Cloud Load 실패: " + task.Exception.Message);
			onLoaded?.Invoke(null);
		}
		else {
			onLoaded?.Invoke(task.Result);
		}
	}

	// 내부 저장 비동기 로직
	async Task SaveInternal(string json, Action<CLOUD_SAVE_RESULT> onErrorCallback) {
		await UnityServices.InitializeAsync();
		if (!AuthenticationService.Instance.IsSignedIn) {
			await InitializeAndLoginAsync();
			Debug.Log("로그인처리 완료");
			if (!AuthenticationService.Instance.IsSignedIn) {
				onErrorCallback?.Invoke(CLOUD_SAVE_RESULT.ERROR_LOGIN_FAIL);
				Debug.LogWarning("CloudSave 실패: 로그인되지 않음");
				return;
			}
		}

		await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
			{ PlayerDataKey, json }
		});
	}

	// 내부 로드 비동기 로직
	async Task<string> LoadInternal() {
		await UnityServices.InitializeAsync();
		if (!AuthenticationService.Instance.IsSignedIn) {
			await InitializeAndLoginAsync();
			if (!AuthenticationService.Instance.IsSignedIn) {
				Debug.LogWarning("CloudSave 실패: 로그인되지 않음");
				return null;
			}
		}

		var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { PlayerDataKey });

		if (result.TryGetValue(PlayerDataKey, out var item)) {
			var jsonString = item.Value.GetAs<string>();
			return jsonString;
		}

		Debug.LogWarning("CloudLoad: 저장된 데이터 없음");
		return null;
	}
}
