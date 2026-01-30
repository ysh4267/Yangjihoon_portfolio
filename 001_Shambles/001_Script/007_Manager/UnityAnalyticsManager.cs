using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;

/// <summary>
/// Unity Analytics 서비스를 사용하여 게임 내 이벤트를 전송하고 관리하는 클래스입니다.
/// </summary>
public static class UnityAnalyticsManager {
	public enum EVENT_KEY {
		Example_Event_Test,
		Example_Event_01,
		Example_Event_02,
		Example_Event_03
	}
	private static Dictionary<EVENT_KEY, string> eventName;
	private static Dictionary<string, Dictionary<string, object>> eventParameters;

	static UnityAnalyticsManager() {
		eventName = new Dictionary<EVENT_KEY, string>() {
			{ EVENT_KEY.Example_Event_Test, "example_event_test" },
			{ EVENT_KEY.Example_Event_01, "example_event_01" },
			{ EVENT_KEY.Example_Event_02, "example_event_02" },
			{ EVENT_KEY.Example_Event_03, "example_event_03" }
		};

		eventParameters = new Dictionary<string, Dictionary<string, object>>() {
			{ eventName[EVENT_KEY.Example_Event_Test], new Dictionary<string, object>() },
			{ eventName[EVENT_KEY.Example_Event_01], new Dictionary<string, object>() },
			{ eventName[EVENT_KEY.Example_Event_02], new Dictionary<string, object>() },
			{ eventName[EVENT_KEY.Example_Event_03], new Dictionary<string, object>() }
		};
	}

	/// <summary>
	/// Unity Analytics 이벤트 전송 데이터 포맷을 정의하는 내부 클래스입니다.
	/// </summary>
	public class UnityAnalyticsEvent : Event {
		public UnityAnalyticsEvent(string EventName) : base(EventName) {
		}
		
		// 변수가 있을 때 SetParameter로 값을 설정하고 없을 때는 설정하지 않음
		public int? example_param_int { set { if (value.HasValue) SetParameter("example_param_int", value.Value); } }
		public string example_param_str_1 { set { if (!string.IsNullOrEmpty(value)) SetParameter("example_param_str_1", value); } }
		public string example_param_str_2 { set { if (!string.IsNullOrEmpty(value)) SetParameter("example_param_str_2", value); } }
		public string example_param_str_3 { set { if (!string.IsNullOrEmpty(value)) SetParameter("example_param_str_3", value); } }
		
		// 새로운 이벤트 파라미터가 추가될 때마다 이곳에 정의
	}

	// Unity Analytics로 이벤트 전송 수행
	public static async void SendEvent(EVENT_KEY keyType, params KeyValuePair<string, object>[] pairs) {
		// 데이터 수집 동의 여부 확인 (예제용 키 사용)
		if (UnityEngine.PlayerPrefs.GetInt("Example_Collect_Data_Key", 1) != 1)
			return;

		// 초기화 여부 확인 후 필요시 수행
		if (UnityServices.State != ServicesInitializationState.Initialized) {
			try {
				await UnityServices.InitializeAsync();
			}
			catch (Exception e) {
				UnityEngine.Debug.LogError($"[Analytics] UnityServices 초기화 실패: {e.Message}");
				return;
			}
		}

		// 파라미터 구성
		var name = eventName[keyType];
		var paramDict = eventParameters[name];

		foreach (var pair in pairs) {
			if (paramDict.ContainsKey(pair.Key))
				paramDict[pair.Key] = pair.Value;
			else
				paramDict.Add(pair.Key, pair.Value);
		}

		// 이벤트 객체 생성 및 파라미터 매핑
		var unityEvent = new UnityAnalyticsEvent(name) {
			example_param_int = paramDict.ContainsKey("example_param_int") ? (int?)paramDict["example_param_int"] : null,
			example_param_str_1 = paramDict.ContainsKey("example_param_str_1") ? (string)paramDict["example_param_str_1"] : null,
			example_param_str_2 = paramDict.ContainsKey("example_param_str_2") ? (string)paramDict["example_param_str_2"] : null,
			example_param_str_3 = paramDict.ContainsKey("example_param_str_3") ? (string)paramDict["example_param_str_3"] : null
		};

		try {
			AnalyticsService.Instance.RecordEvent(unityEvent);
		}
		catch (Exception e) {
			UnityEngine.Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
		}
	}
}
